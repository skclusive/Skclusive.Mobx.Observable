using System;
using System.Collections.Generic;

namespace Skclusive.Mobx.Observable
{
    public class ComputedValueOptions<T> : IComputedValueOptions<T>
    {
        public string Name { set; get; }

        public IEqualityComparer<T> Comparer { set; get; }

        public object Context { set; get; }

        public bool RequiresReaction { set; get; }

        public bool KeepAlive { set; get; }

        public Func<T> Derivation { set; get; }
    }
}
