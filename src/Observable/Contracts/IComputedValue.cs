using System;

namespace Skclusive.Mobx.Observable
{
    public interface IComputedValue : IValueReader
    {
        void Suspend();
    }

    public interface IComputedValue<T> : IComputedValue
    {
        new T Value { get; }

        IDisposable Observe(Action<IValueDidChange<T>> listener, bool force = false);

        bool TrackAndCompute();
    }
}
