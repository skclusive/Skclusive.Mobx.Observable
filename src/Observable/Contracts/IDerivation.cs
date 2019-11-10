using System;
using System.Collections.Generic;
using System.Text;

namespace Skclusive.Mobx.Observable
{
    public interface IDerivation : IDepTreeNode
    {
        IList<IObservable> NewObservings { get; set; }

        DerivationState DependenciesState { get; set; }

        /**
        * Id of the current run of a derivation. Each time the derivation is tracked
        * this number is increased by one. This number is globally unique
        */
        int RunId { get; set; }

        /**
        * amount of dependencies used by the derivation in this run, which has not been bound yet.
        */
        int UnboundDepsCount { get; set; }

        string MapId { get; set;  }

        TraceMode Mode { get; set; }

        void OnBecomeStale();
    }
}
