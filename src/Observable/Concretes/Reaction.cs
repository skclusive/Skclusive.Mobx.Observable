using System;
using System.Collections.Generic;

namespace Skclusive.Mobx.Observable
{
    public class Reaction : IDerivation, IReactionPublic, IDepTreeNodeClassifier
    {
        public int RunId { set; get; }

        DepTreeNodeType IDepTreeNodeClassifier.AtomType => DepTreeNodeType.Reaction;

        IDepTreeNode IDepTreeNodeClassifier.Node => this;

        public int DiffValue { set; get; }

        public int UnboundDepsCount { set; get; }

        public string MapId { set; get; } = $"#{States.NextId}";

        public TraceMode Mode { set; get; } = TraceMode.NONE;

        public string Name { set; get; }

        public IList<IObservable> Observings { set; get; } = new List<IObservable>();

        public IList<IObservable> NewObservings { set; get; } = new List<IObservable>();

        public DerivationState DependenciesState { set; get; } = DerivationState.NOT_TRACKING;

        private Action<Reaction> OnInvalidate { set; get; }

        private Action<Exception, IDerivation> ErrorHandler { set; get; }

        public bool IsScheduled { private set; get; }

        public bool IsRunning { private set; get; }

        public bool IsTrackPending { private set; get; }

        public bool IsDisposed { private set; get; }

        public Reaction(string name, Action<Reaction> onInvalidate, Action<Exception, IDerivation> errorHandler)
        {
            Name = name ?? $"Reaction@{States.NextId}";

            OnInvalidate = onInvalidate;

            ErrorHandler = errorHandler;
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;

                if (!IsRunning)
                {
                    // if disposed while running, clean up later. Maybe not optimal, but rare case
                    Reactions.Transaction(() => this.ClearObservings());
                }
            }
        }

        public void OnBecomeStale()
        {
            Schedule();
        }

        public void Schedule()
        {
            if (!IsScheduled)
            {
                IsScheduled = true;
                States.State.PendingReactions.Add(this);
                States.RunReactions();
            }
        }

        /**
     * internal, use schedule() if you intend to kick off a reaction
     */
        public void RunReaction()
        {
            if (IsDisposed)
            {
                return;
            }

            Reactions.Transaction(() =>
            {
                IsScheduled = false;
                if (this.ShouldCompute())
                {
                    IsTrackPending = true;

                    try
                    {
                        OnInvalidate(this);
                    }
                    catch (Exception ex)
                    {
                        ReportExceptionInDerivation(ex);
                    }
                }
            });
        }

        public void Trace(bool enterBreakPoint = false)
        {
            throw new NotImplementedException();
        }

        public void Track(Func<object> func)
        {
            if (IsDisposed)
            {
                return;
            }

            Reactions.Transaction(() =>
            {

                IsRunning = true;
                CaughtException exception = null;
                try
                {
                    var result = this.TrackedDerivedFunction<object>(func, context: null);
                }
                catch (CaughtException ex)
                {
                    exception = ex;
                }
                IsRunning = false;
                IsTrackPending = false;

                if (IsDisposed)
                {
                    // disposed during last run. Clean up everything that was bound after the dispose call.
                    this.ClearObservings();
                }

                if (exception != null)
                {
                    ReportExceptionInDerivation(exception.Cause);
                }
            });
        }

        private void ReportExceptionInDerivation(Exception error)
        {
            if (ErrorHandler != null)
            {
                ErrorHandler(error, this);

                return;
            }

            if (States.State.DisableErrorBoundaries)
            {
                return;
            }

            Console.WriteLine($"[mobx] Encountered an uncaught exception that was thrown by a reaction or observer component, in: {this}");

            //if (isSpyEnabled())
            //{
            //    spyReport({
            //        type: "error",
            //    name: this.name,
            //    message,
            //    error: "" + error
            //    })
            //}

            foreach (var handler in States.State.ReactionErrorHandlers)
            {
                handler(error, this);
            }
        }

        public IReactionDisposable GetDisposable()
        {
            return new ReactionDisposable(this);
        }

        public override string ToString()
        {
            return $"Reaction[{Name}]";
        }
    }
}
