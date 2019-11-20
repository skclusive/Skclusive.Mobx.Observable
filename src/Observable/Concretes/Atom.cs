using System.Collections.Generic;

namespace Skclusive.Mobx.Observable
{
    public class Atom : IAtom, IDepTreeNodeClassifier
    {
        public Atom(string name)
        {
            Name = name ?? $"Atom@{States.NextId}";
        }

        public int DiffValue { set; get; }

        public int LastAccessedBy { set; get; }

        public bool IsBeingObserved { set; get; }

        public DerivationState LowestObserverState { set; get; } = DerivationState.NOT_TRACKING;

        public IList<IDerivation> Observers { set; get; } = new List<IDerivation>();

        public bool IsPendingUnobservation { set; get; }

        public string Name { private set; get; }

        public IList<IObservable> Observings { set; get; } = new List<IObservable>();

        DepTreeNodeType IDepTreeNodeClassifier.AtomType => DepTreeNodeType.Atom;

        IDepTreeNode IDepTreeNodeClassifier.Node => this;

        public event AtomHandler OnBecomeObservedEvent;

        public event AtomHandler OnBecomeUnObservedEvent;

        public void OnBecomeObserved()
        {
            if (OnBecomeObservedEvent != null)
            {
                OnBecomeObservedEvent.Invoke(this);
            }
        }

        public void OnBecomeUnobserved()
        {
            if (OnBecomeUnObservedEvent != null)
            {
                OnBecomeUnObservedEvent.Invoke(this);
            }
        }

    /**
     * Invoke this method _after_ this method has changed to signal mobx that all its observers should invalidate.
     */
        public void ReportChanged()
        {
            Reactions.Transaction(() => this.PropagateChanged());
        }

    /**
    * Invoke this method to notify mobx that your atom has been used somehow.
    * Returns true if there is currently a reactive context.
    */
        public bool ReportObserved()
        {
            return ObservableExtension.ReportObserved(this);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
