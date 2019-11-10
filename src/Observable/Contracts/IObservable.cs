using System.Collections.Generic;

namespace Skclusive.Mobx.Observable
{
    public interface IObservable : IDepTreeNode
    {
        int DiffValue { get; set; }

        int LastAccessedBy { get; set; }

        bool IsBeingObserved { get; set; }

        DerivationState LowestObserverState { get; set; }

        IList<IDerivation> Observers { get; }

        bool IsPendingUnobservation { get; set; }

        void OnBecomeUnobserved();

        void OnBecomeObserved();
    }
}
