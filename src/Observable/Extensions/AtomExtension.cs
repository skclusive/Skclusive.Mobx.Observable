using System;

namespace Skclusive.Mobx.Observable
{
    public static class AtomExtension
    {
        public static void CheckIfStateModificationsAreAllowed(this IAtom atom)
        {
            var hasObservers = atom.Observers.Count > 0;
            // Should never be possible to change an observed observable from inside computed, see #798
            if (States.State.ComputationDepth > 0 && hasObservers)
            {
                throw new Exception($"Computed values are not allowed to cause side effects by changing observables that are already being observed. Tried to modify: {atom.Name}");
            }

            // TODO: investigate why below code commented
            // Should not be possible to change observed state outside strict mode, except during initialization, see #563
            //if (!States.State.AllowStateChanges && (hasObservers || States.State.EnforceActions))
            //{
            //    throw new Exception((States.State.EnforceActions ? "Since strict-mode is enabled, changing observed observable values outside actions is not allowed. Please wrap the code in an `action` if this change is intended. Tried to modify: "
            //        : "Side effects like changing state are not allowed at this point. Are you trying to modify state from, for example, the render function of a React component? Tried to modify: ") +
            //        atom.Name);
            //}
        }
    }
}
