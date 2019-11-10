using System;
using System.Collections.Generic;

namespace Skclusive.Mobx.Observable
{
    public static class DerivationExtension
    {
        public static bool ShouldCompute(this IDerivation derivation)
        {
            switch (derivation.DependenciesState)
            {
                case DerivationState.UP_TO_DATE:
                    return false;
                case DerivationState.NOT_TRACKING:
                case DerivationState.STALE:
                    return true;
                case DerivationState.POSSIBLY_STALE:
                    {
                        var prevUntracked = States.UntrackedStart(); // no need for those computeds to be reported, they will be picked up in trackDerivedFunction.
                        foreach (var observer in derivation.Observings)
                        {
                            if (observer is IComputedValue computed)
                            {
                                if (States.State.DisableErrorBoundaries)
                                {
                                    var x = computed.Value;
                                }
                                else
                                {
                                    try
                                    {
                                        var y = computed.Value;
                                    }
                                    catch
                                    {
                                        // we are not interested in the value *or* exception at this moment, but if there is one, notify all
                                        States.UntrackedEnd(prevUntracked);
                                        return true;
                                    }
                                }
                                // if ComputedValue `obj` actually changed it will be computed and propagated to its observers.
                                // and `derivation` is an observer of `obj`
                                // invariantShouldCompute(derivation)
                                if (derivation.DependenciesState == DerivationState.STALE)
                                {
                                    States.UntrackedEnd(prevUntracked);
                                    return true;
                                }
                            }
                        }
                        derivation.ChangeDependenciesStateTo0();
                        States.UntrackedEnd(prevUntracked);
                        return false;
                    }
                default:
                    return false;
            }
        }

        public static void ChangeDependenciesStateTo0(this IDerivation derivation)
        {
            if (derivation.DependenciesState == DerivationState.UP_TO_DATE)
            {
                return;
            }
            derivation.DependenciesState = DerivationState.UP_TO_DATE;

            var observings = derivation.Observings;
            var i = observings.Count;
            while (i-- > 0)
            {
                observings[i].LowestObserverState = DerivationState.UP_TO_DATE;
            }
        }

        public static void ClearObservings(this IDerivation derivation)
        {
            var observings = derivation.Observings;
            derivation.Observings = new List<IObservable>();
            var i = observings.Count;
            while (i-- > 0)
            {
                observings[i].RemoveObserver(derivation);
            }
            derivation.DependenciesState = DerivationState.NOT_TRACKING;
        }

        /**
         * diffs newObserving with observing.
         * update observing to be newObserving with unique observables
         * notify observers that become observed/unobserved
         */
        public static void BindDependencies(this IDerivation derivation)
        {
            var prevObserving = derivation.Observings;
            var observings = (derivation.Observings = derivation.NewObservings);
            var lowestNewObservingDerivationState = DerivationState.UP_TO_DATE;

            // Go through all new observables and check diffValue: (this list can contain duplicates):
            //   0: first occurrence, change to 1 and keep it
            //   1: extra occurrence, drop it
            int i0 = 0,
                l = derivation.UnboundDepsCount;
            for (var i = 0; i < l; i++)
            {
                var dep = observings[i];
                if (dep.DiffValue == 0)
                {
                    dep.DiffValue = 1;
                    if (i0 != i)
                    {
                        observings[i0] = dep;
                    }
                    i0++;
                }

                // Upcast is 'safe' here, because if dep is IObservable, `dependenciesState` will be undefined,
                // not hitting the condition
                if (dep is IDerivation obsderivation)
                {
                    if (obsderivation.DependenciesState > lowestNewObservingDerivationState)
                    {
                        lowestNewObservingDerivationState = obsderivation.DependenciesState;
                    }
                }
            }
            for (var x = i0; x < observings.Count; x++)
            {
                observings.RemoveAt(x);
            }
            derivation.NewObservings = new List<IObservable>(); // newObserving shouldn't be needed outside tracking (statement moved down to work around FF bug, see #614)

            // Go through all old observables and check diffValue: (it is unique after last bindDependencies)
            //   0: it's not in new observables, unobserve it
            //   1: it keeps being observed, don't want to notify it. change to 0
            var pl = prevObserving.Count;
            while (pl-- > 0)
            {
                var pdep = prevObserving[pl];
                if (pdep.DiffValue == 0)
                {
                    pdep.RemoveObserver(derivation);
                }
                pdep.DiffValue = 0;
            }

            // Go through all new observables and check diffValue: (now it should be unique)
            //   0: it was set to 0 in last loop. don't need to do anything.
            //   1: it wasn't observed, let's observe it. set back to 0
            while (i0-- > 0)
            {
                var xdep = observings[i0];
                if (xdep.DiffValue == 1)
                {
                    xdep.DiffValue = 0;
                    xdep.AddObserver(derivation);
                }
            }

            // Some new observed derivations may become stale during this derivation computation
            // so they have had no chance to propagate staleness (#916)
            if (lowestNewObservingDerivationState != DerivationState.UP_TO_DATE)
            {
                derivation.DependenciesState = lowestNewObservingDerivationState;
                derivation.OnBecomeStale();
            }
        }

        /**
         * Executes the provided function `f` and tracks which observables are being accessed.
         * The tracking information is stored on the `derivation` object and the derivation is registered
         * as observer of any of the accessed observables.
         */
        public static T TrackedDerivedFunction<T>(this IDerivation derivation, Func<T> func, object context)
        {
            // pre allocate array allocation + room for variation in deps
            // array will be trimmed by bindDependencies
            derivation.ChangeDependenciesStateTo0();

            derivation.NewObservings = new List<IObservable>(derivation.Observings.Count + 100);
            derivation.UnboundDepsCount = 0;
            derivation.RunId = States.NextRunId;

            var previous = States.UntrackedStart(derivation);

            T result = default(T);

            if (States.State.DisableErrorBoundaries)
            {
                result = func();
            }
            else
            {
                try
                {
                    result = func();
                }
                catch (Exception ex)
                {
                    throw new CaughtException(ex);
                }
            }

            States.UntrackedEnd(previous);
            derivation.BindDependencies();
            return result;
        }
    }
}
