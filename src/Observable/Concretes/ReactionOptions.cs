using System.Collections.Generic;

namespace Skclusive.Mobx.Observable
{
    public class ReactionOptions<T> : AutorunOptions, IReactionOptions<T>
    {
        public bool FireImmediately { get; set; }

        public IEqualityComparer<T> Comparer { get; set; }
    }
}
