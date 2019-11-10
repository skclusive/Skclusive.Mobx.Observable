using System.Collections.Generic;

namespace Skclusive.Mobx.Observable
{
    public static class ObservableExtension
    {
        // Called by Atom when its value changes
        public static void PropagateChanged(this IObservable observable)
        {
            // invariantLOS(observable, "changed start");
            if (observable.LowestObserverState == DerivationState.STALE)
            {
                return;
            }
            observable.LowestObserverState = DerivationState.STALE;

            foreach (var observer in observable.Observers)
            {
                if (observer.DependenciesState == DerivationState.UP_TO_DATE)
                {
                    if (observer.Mode != TraceMode.NONE)
                    {
                        // logTraceInfo(d, observable);
                    }
                    observer.OnBecomeStale();
                }
                observer.DependenciesState = DerivationState.STALE;
            }
        }

        // Called by ComputedValue when it recalculate and its value changed
        public static void PropagateChangeConfirmed(this IObservable observable)
        {
            // invariantLOS(observable, "changed start");
            if (observable.LowestObserverState == DerivationState.STALE)
            {
                return;
            }
            observable.LowestObserverState = DerivationState.STALE;

            foreach (var observer in observable.Observers)
            {
                if (observer.DependenciesState == DerivationState.POSSIBLY_STALE)
                {
                    observer.DependenciesState = DerivationState.STALE;
                }
                else if (observer.DependenciesState == DerivationState.UP_TO_DATE) // this happens during computing of `d`, just keep lowestObserverState up to date.
                {
                    observable.LowestObserverState = DerivationState.UP_TO_DATE;
                }
            }
        }

        // Used by computed when its dependency changed, but we don't wan't to immediately recompute.

        public static void PropagateMaybeChanged(this IObservable observable)
        {
            // invariantLOS(observable, "changed start");
            if (observable.LowestObserverState != DerivationState.UP_TO_DATE)
            {
                return;
            }
            observable.LowestObserverState = DerivationState.POSSIBLY_STALE;

            foreach (var observer in observable.Observers)
            {
                if (observer.DependenciesState == DerivationState.UP_TO_DATE)
                {
                    observer.DependenciesState = DerivationState.POSSIBLY_STALE;
                    if (observer.Mode != TraceMode.NONE)
                    {
                        // logTraceInfo(d, observable)
                    }
                    observer.OnBecomeStale();
                }
            }
        }

        public static bool ReportObserved(this IObservable observable)
        {
            var derivation = States.State.TrackingDerivation;
            if (derivation != null)
            {
                /**
                 * Simple optimization, give each derivation run an unique id (runId)
                 * Check if last time this observable was accessed the same runId is used
                 * if this is the case, the relation is already known
                 */
                if (derivation.RunId != observable.LastAccessedBy)
                {
                    observable.LastAccessedBy = derivation.RunId;

                    // Tried storing newObserving, or observing, or both as Set, but performance didn't come close...

                    derivation.NewObservings.Insert(derivation.UnboundDepsCount++, observable);

                    if (!observable.IsBeingObserved)
                    {
                        observable.IsBeingObserved = true;
                        observable.OnBecomeObserved();
                    }
                }
                return true;
            }
            else if (observable.Observers.Count == 0 && States.State.InBatch > 0)
            {
                QueueForUnobservation(observable);
            }
            return false;
        }

        public static void QueueForUnobservation(this IObservable observable)
        {
            if (!observable.IsPendingUnobservation)
            {
                // invariant(observable._observers.length === 0, "INTERNAL ERROR, should only queue for unobservation unobserved observables");
                observable.IsPendingUnobservation = true;

                States.State.PendingUnobservations.Add(observable);
            }
        }

        public static bool HasObservers(this IObservable observable)
        {
            return observable.Observers.Count > 0;
        }

        public static ISet<IDerivation> GetObservers(this IObservable observable)
        {
            return new HashSet<IDerivation>(observable.Observers);
        }

        public static void AddObserver(this IObservable observable, IDerivation observer)
        {
            observable.Observers.Add(observer);

            if (observable.LowestObserverState > observer.DependenciesState)
            {
                observable.LowestObserverState = observer.DependenciesState;
            }
        }

        public static void RemoveObserver(this IObservable observable, IDerivation observer)
        {
            observable.Observers.Remove(observer);

            if (observable.Observers.Count == 0)
            {
                // deleting last observer
                QueueForUnobservation(observable);
            }
        }
    }
}
