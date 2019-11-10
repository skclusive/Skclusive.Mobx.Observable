using System;
using System.Collections.Generic;

namespace Skclusive.Mobx.Observable
{
    public class State
    {
        /**
        * States version.
        * compatiblity with other versions loaded in memory as long as this version matches.
        * It indicates that the global state still stores similar information
        */
        public int Version { get => 1; }

        /**
         * Currently running derivation
         */
        public IDerivation TrackingDerivation { set; get; }

        /**
         * Are we running a computation currently? (not a reaction)
         */
        public int ComputationDepth { get; set; } = 0;

        /**
        * Each time a derivation is tracked, it is assigned a unique run-id
        */
        public int RunId { get; set; } = 0;

        /**
         * 'guid' for general purpose. Will be persisted amongst resets.
         */
        public int Guid { get; set; } = 0;

        /**
         * Are we in a batch block? (and how many of them)
         */
        public int InBatch { get; set; } = 0;

        /**
        * Observables that don't have observers anymore, and are about to be
        * suspended, unless somebody else accesses it in the same batch
        *
        * @type {IObservable[]}
        */
        public IList<IObservable> PendingUnobservations { get; set; } = new List<IObservable>();

        /**
        * List of scheduled, not yet executed, reactions.
        */
        public IList<Reaction> PendingReactions { get; set; } = new List<Reaction>();


        /**
        * Are we currently processing reactions?
        */
        public bool IsRunningReactions { set; get; }

        /**
        * Is it allowed to change observables at this point?
        * In general, MobX doesn't allow that when running computations and React.render.
        * To ensure that those functions stay pure.
        */
        public bool AllowStateChanges { set; get; }

        /**
        * If strict mode is enabled, state changes are by default not allowed
        */
        public bool EnforceActions { set; get; }

        public IList<Action<object>> SpyListeners { get; set; } = new List<Action<object>>();

        /**
        * Globally attached error handlers that react specifically to errors in reactions
        */
        public IList<Action<object, IDerivation>> ReactionErrorHandlers { get; set; } = new List<Action<object, IDerivation>>();

        /**
        * Warn if computed values are accessed outside a reactive context
        */
        public bool ComputedRequiresReaction { get; set; }

        /*
         * Don't catch and rethrow exceptions. This is useful for inspecting the state of
         * the stack when an exception occurs while debugging.
         */
        public bool DisableErrorBoundaries { get; set; }

        /*
        * If true, we are already handling an exception in an action. Any errors in reactions should be supressed, as
        * they are not the cause, see: https://github.com/mobxjs/mobx/issues/1836
        */
        public bool SuppressReactionErrors { get; set; }
    }
}
