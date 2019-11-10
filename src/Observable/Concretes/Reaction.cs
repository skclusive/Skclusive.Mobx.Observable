using System;
using System.Collections.Generic;

namespace Skclusive.Mobx.Observable
{
    public class Reaction : IDerivation, IReactionPublic
    {
        public int RunId { set; get; }

        public int DiffValue { set; get; }

        public int UnboundDepsCount { set; get; }

        public string MapId { set; get; } = $"#{Globals.NextId}";

        public TraceMode Mode { set; get; } = TraceMode.NONE;

        public string Name { set; get; }

        public IList<IObservable> Observings { set; get; } = new List<IObservable>();

        public IList<IObservable> NewObservings { set; get; } = new List<IObservable>();

        public DerivationState DependenciesState { set; get; } = DerivationState.NOT_TRACKING;

        private Action<Reaction> OnInvalidate { set; get; }

        private Action<object, IDerivation> ErrorHandler { set; get; }

        public bool IsScheduled { private set; get; }

        public bool IsRunning { private set; get; }

        public bool IsTrackPending { private set; get; }

        public bool IsDisposed { private set; get; }

        public Reaction(string name, Action<Reaction> onInvalidate, Action<object, IDerivation> errorHandler)
        {
            Name = name ?? $"Reaction@{Globals.NextId}";

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
                    Globals.Transaction(() => this.ClearObservings());
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
                Globals.State.PendingReactions.Add(this);
                Globals.RunReactions();
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

            Globals.Transaction(() =>
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

            Globals.Transaction(() =>
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

        private void ReportExceptionInDerivation(object error)
        {
            if (ErrorHandler != null)
            {
                ErrorHandler(error, this);

                return;
            }

            if (Globals.State.DisableErrorBoundaries)
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

            foreach (var handler in Globals.State.ReactionErrorHandlers)
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
