using System.Collections.Generic;

namespace Skclusive.Mobx.Observable
{
    public interface IDepTreeNode
    {
        string Name { get; }

        IList<IObservable> Observings { get; set; }
    }
}
