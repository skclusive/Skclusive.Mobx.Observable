using System.Collections.Generic;

namespace Skclusive.Mobx.Observable
{
    public interface IReactionOptions<T> : IAutorunOptions
    {
        bool FireImmediately { get; }

        IEqualityComparer<T> Comparer { get; }
    }
}
