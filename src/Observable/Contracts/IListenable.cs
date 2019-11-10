using System;
using System.Collections.Generic;

namespace Skclusive.Mobx.Observable
{
    public interface IListenable<T>
    {
        IList<Action<T>> Listeners { get; }

        IDisposable Observe(Action<T> listener, bool force = false);
    }
}
