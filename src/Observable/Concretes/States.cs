using System;
using System.Linq;
using System.Threading;

namespace Skclusive.Mobx.Observable
{
    internal class States
    {
        // private static AsyncLocal<State> _LocalState = new AsyncLocal<State>();

        // internal static State State { get => (_LocalState.Value ?? (_LocalState.Value = new State())); }

        private static ThreadLocal<State> _LocalState = new ThreadLocal<State>(() => new State());

        internal static State State { get => _LocalState.Value; }

        private static int MAX_REACTION_ITERATIONS = 100;

        private static Action<Action> ReactionScheduler = (action) => action();

        public static void SetReactionScheduler(Action<Action> scheduler)
        {
            var baseScheduler = ReactionScheduler;

            ReactionScheduler = (action) => scheduler(() => baseScheduler(action));
        }

        public static int NextId
        {
            get => ++State.Guid;
        }

        public static void RunReactions()
        {
            if (State.InBatch > 0 || State.IsRunningReactions)
            {
                return;
            }
            ReactionScheduler(RunReactionsHelper);
        }

        private static void RunReactionsHelper()
        {
            State.IsRunningReactions = true;
            var allReactions = State.PendingReactions;
            var iterations = 0;

            // While running reactions, new reactions might be triggered.
            // Hence we work with two variables and check whether
            // we converge to no remaining reactions after a while.
            while (allReactions.Count > 0)
            {
                if (++iterations == MAX_REACTION_ITERATIONS)
                {
                    Console.WriteLine($"Reaction doesn't converge to a stable state after {MAX_REACTION_ITERATIONS} iterations. Probably there is a cycle in the reactive function: { allReactions[0]}");

                    allReactions.Clear(); // clear reactions
                }
                var remainingReactions = allReactions.ToList();
                allReactions.Clear();
                foreach (var reaction in remainingReactions)
                {
                    reaction.RunReaction();
                }
            }
            State.IsRunningReactions = false;
        }

        public static IDisposable OnReactionError(Action<object, IDerivation> action)
        {
            State.ReactionErrorHandlers.Add(action);

            return new Disposable(() => State.ReactionErrorHandlers.Remove(action));
        }

        /**
        * Batch starts a transaction, at least for purposes of memoizing ComputedValues when nothing else does.
        * During a batch `onBecomeUnobserved` will be called at most once per observable.
        * Avoids unnecessary recalculations.
        */
        internal static void StartBatch()
        {
            State.InBatch++;
        }

        internal static void EndBatch()
        {
            if (--State.InBatch == 0)
            {
                RunReactions();

                // the batch is actually about to finish, all unobserving should happen here.
                var list = State.PendingUnobservations.ToList();
                foreach (var observable in list)
                {
                    observable.IsPendingUnobservation = false;

                    if (observable.Observers.Count == 0)
                    {
                        if (observable.IsBeingObserved)
                        {
                            // if this observable had reactive observers, trigger the hooks
                            observable.IsBeingObserved = false;
                            observable.OnBecomeUnobserved();
                        }
                        if (observable is IComputedValue computed)
                        {
                            // computed values are automatically teared down when the last observer leaves
                            // this process happens recursively, this computed might be the last observabe of another, etc..
                            computed.Suspend();
                        }
                    }
                }
                State.PendingUnobservations.Clear();
            }
        }

        /**
        * NOTE: current propagation mechanism will in case of self reruning autoruns behave unexpectedly
        * It will propagate changes to observers from previous run
        * It's hard or maybe impossible (with reasonable perf) to get it right with current approach
        * Hopefully self reruning autoruns aren't a feature people should depend on
        * Also most basic use cases should be ok
        */
        public static IDerivation UntrackedStart(IDerivation current = null)
        {
            var previous = State.TrackingDerivation;

            State.TrackingDerivation = current;

            return previous;
        }

        public static void UntrackedEnd(IDerivation previous)
        {
            State.TrackingDerivation = previous;
        }

        public static T Untracked<T>(Func<T> func)
        {
            var previous = UntrackedStart();
            try
            {
                return func();
            }
            finally
            {
                UntrackedEnd(previous);
            }
        }

        public static bool IsComputingDerivation()
        {
            return State.TrackingDerivation != null;
        }

        public static int NextRunId
        {
            get => ++State.RunId;
        }
    }
}
