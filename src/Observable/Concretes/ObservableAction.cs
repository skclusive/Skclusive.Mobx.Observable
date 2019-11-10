using System;
using System.Collections.Generic;
using System.Linq;

namespace Skclusive.Mobx.Observable
{
    public class ObservableAction<T> : IObservable, IObservableAction
    {
        public ObservableAction(string name, Func<object[], object> action, params object[] prefixes)
        {
            Name = name;

            Prefixes = prefixes;

            Action = action;
        }

        private object[] Prefixes { set; get; }

        private Func<object[], object> Action { set; get; }

        public int DiffValue { set; get; }

        public int LastAccessedBy { set; get; }

        public bool IsBeingObserved { set; get; }

        public DerivationState LowestObserverState { set; get; }

        public IList<IDerivation> Observers { private set; get; }

        public bool IsPendingUnobservation { set; get; }

        public IList<IObservable> Observings { set; get; }

        public string Name { private set; get; }

        public object Execute(object[] arguments)
        {
            return Action.Invoke(Prefixes.Concat(arguments).ToArray());
        }

        public void OnBecomeObserved()
        {
        }

        public void OnBecomeUnobserved()
        {
        }
    }
}
