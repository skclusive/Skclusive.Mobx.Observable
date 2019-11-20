using System;
using System.Collections.Generic;

namespace Skclusive.Mobx.Observable
{
    /**
     * A node in the state dependency root that observes other nodes, and can be observed itself.
     *
     * ComputedValue will remember the result of the computation for the duration of the batch, or
     * while being observed.
     *
     * During this time it will recompute only when one of its direct dependencies changed,
     * but only when it is being accessed with `ComputedValue.get()`.
     *
     * Implementation description:
     * 1. First time it's being accessed it will compute and remember result
     *    give back remembered result until 2. happens
     * 2. First time any deep dependency change, propagate POSSIBLY_STALE to all observers, wait for 3.
     * 3. When it's being accessed, recompute if any shallow dependency changed.
     *    if result changed: propagate STALE to all observers, that were POSSIBLY_STALE from the last step.
     *    go to step 2. either way
     *
     * If at any point it's outside batch and it isn't observed: reset everything and go to 1.
     */
    public class ComputedValue<T> : IComputedValue<T>, IObservable, IDerivation, IDepTreeNodeClassifier
    {

        /**
        * Create a new computed value based on a function expression.
        *
        * The `name` property is for debug purposes only.
        *
        * The `equals` property specifies the comparer function to use to determine if a newly produced
        * value differs from the previous value. Two comparers are provided in the library; `defaultComparer`
        * compares based on identity comparison (===), and `structualComparer` deeply compares the structure.
        * Structural comparison can be convenient if you always produce a new aggregated object and
        * don't want to notify observers if it is structurally the same.
        * This is useful for working with vectors, mouse coordinates etc.
        */
        public ComputedValue(IComputedValueOptions<T> options)
        {
            _Value = default(T);
            Derivation = options.Derivation;
            Name = options.Name ?? $"ComputedValue@{States.NextId}";
            Scope = options.Context;
            RequiresReaction = options.RequiresReaction;
            KeepAlive = options.KeepAlive;
            Comparer = options.Comparer ?? EqualityComparer<T>.Default;
        }

        public static IComputedValue<T> From(Func<T> compute)
        {
            return new ComputedValue<T>(new ComputedValueOptions<T> { Derivation = compute });
        }

        public object Scope { private set; get; }

        DepTreeNodeType IDepTreeNodeClassifier.AtomType => DepTreeNodeType.Computed;

        IDepTreeNode IDepTreeNodeClassifier.Node => this;

        private IEqualityComparer<T> Comparer { set; get; }

        public int DiffValue { set; get; }

        public int LastAccessedBy { set; get; }

        public bool IsBeingObserved { set; get; }

        public DerivationState LowestObserverState { set; get; } = DerivationState.UP_TO_DATE;

        public IList<IDerivation> Observers { private set; get; } = new List<IDerivation>();

        public bool IsPendingUnobservation { set; get; }

        public string Name { private set; get; }

        // nodes we are looking at. Our value depends on these nodes
        public IList<IObservable> Observings { set; get; } = new List<IObservable>();

        // during tracking it's an array with new observed observers
        public IList<IObservable> NewObservings { set; get; }

        public DerivationState DependenciesState { set; get; } = DerivationState.NOT_TRACKING;

        public int RunId { set; get; }

        public int UnboundDepsCount { set; get; }

        public string MapId { set; get; } = $"#{States.NextId}";

        public string TriggeredBy { private set; get; }

        public bool IsComputing { private set; get; }

        private Func<T> Derivation { get; set; }

        private bool RequiresReaction { set; get; }

        private bool KeepAlive { set; get; }

        private bool FirstGet { set; get; } = true;

        public TraceMode Mode { set; get; } = TraceMode.NONE;

        private object _Value { set; get; }

        object IValueReader.Value { get => Value; }


        public T Peek
        {
            get => Compute(false);
        }

        public T Value
        {
            get
            {
                if (KeepAlive && FirstGet)
                {
                    FirstGet = false;

                    Reactions.Autorun((view) => { var x = Value; });
                }

                if (IsComputing)
                {
                    throw new InvalidOperationException($"Cycle detected in computation {Name}");
                }

                if (States.State.InBatch == 0 && Observers.Count == 0)
                {
                    if (this.ShouldCompute())
                    {
                        WarnAboutUntrackedRead();
                        // See perf test 'computed memoization'
                        _Value = Reactions.Transaction(() => Compute(false));
                    }
                }
                else
                {
                    this.ReportObserved();
                    if (this.ShouldCompute() && TrackAndCompute())
                    {
                        this.PropagateChangeConfirmed();
                    }
                }

                return (T)_Value;
            }
        }

        private void WarnAboutUntrackedRead()
        {
            if (RequiresReaction)
            {
                throw new Exception($"[mobx] Computed value {Name} is read outside a reactive context");
            }

            if (Mode != TraceMode.NONE)
            {
                throw new Exception($"[mobx.trace] {Name} is being read outside a reactive context. Doing a full recompute");
            }

            if (States.State.ComputedRequiresReaction)
            {
                throw new Exception($"[mobx] Computed value {Name} is being read outside a reactive context. Doing a full recompute");
            }
        }

        private T Compute(bool track)
        {
            IsComputing = true;

            States.State.ComputationDepth++;

            T result;

            try
            {
                if (track)
                {
                    result = this.TrackedDerivedFunction<T>(Derivation, Scope);
                }
                else
                {
                    result = Derivation();
                }
            }
            finally
            {
                States.State.ComputationDepth--;
                IsComputing = false;
            }
            return result;
        }

        public bool TrackAndCompute()
        {
            var oldValue = (T)_Value;
            var wasSuspended = DependenciesState == DerivationState.NOT_TRACKING;
            var newValue = Compute(true);

            var changed = wasSuspended || !EqualityComparer<T>.Default.Equals(oldValue, newValue);

            if (changed)
            {
                _Value = newValue;
            }

            return changed;
        }

        public void OnBecomeStale()
        {
            this.PropagateMaybeChanged();
        }

        public void OnBecomeObserved()
        {
        }

        public void OnBecomeUnobserved()
        {
        }

        public void Suspend()
        {
            this.ClearObservings();

            _Value = default(T); // don't hold on to computed value!
        }

        public IDisposable Observe(Action<IValueDidChange<T>> listener, bool force = false)
        {
            var firstTime = true;
            T previous = default(T);
            return Reactions.Autorun((reaction) =>
            {
                var newvalue = Value;
                if (!firstTime || force)
                {
                    var tracked = States.UntrackedStart();
                    listener(new ValueDidChange<T>(newvalue, previous, this));
                    States.UntrackedEnd(tracked);
                }
                firstTime = false;
                previous = newvalue;
            });
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
