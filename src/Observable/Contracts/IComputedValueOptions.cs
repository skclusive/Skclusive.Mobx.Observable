using System;
using System.Collections.Generic;
using System.Text;

namespace Skclusive.Mobx.Observable
{
    public interface IComputedValueOptions<T>
    {
        string Name { get; }

        IEqualityComparer<T> Comparer { get; }

        object Context { get; }

        bool RequiresReaction { get; }

        bool KeepAlive { get; }

        Func<T> Derivation { get; }
    }
}
