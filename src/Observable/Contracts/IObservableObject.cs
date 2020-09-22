using System;

namespace Skclusive.Mobx.Observable
{
    public interface IObservableObject : IObservableMeta, IInterceptable<IObjectWillChange>, IListenable<IObjectDidChange>
    {
        string Name { get; }

        bool Remove(string key);

        object Get(string key);

        object Read(string key);

        object Write(string key, object value);

        bool TryRead(string key, out object value);

        bool TryWrite(string key, object value, out object newValue);

        void AddAction(string method, Func<object[], object> action);

        bool TryInvokeAction(string action, object[] args, out object result);
    }

    public interface IObservableObject<T, W> : IObservableObject
    {
        P Read<P>(string key);

        object Write<P>(string key, P value);

        T Proxy { get; }
    }

    public interface IObservableObject<T> : IObservableObject<T, object>
    {
    }
}
