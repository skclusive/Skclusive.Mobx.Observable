using System;

namespace Skclusive.Mobx.Observable
{
    public interface IReactionDisposable : IDisposable
    {
        Reaction Reaction { get; }
    }
}
