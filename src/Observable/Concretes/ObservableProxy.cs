using System;
using System.Collections.Generic;

namespace Skclusive.Mobx.Observable
{
    public abstract class ObservableProxy<T, W> : IObservableObject<T, W>
    {
        protected ObservableProxy(IObservableObject<T, W> target)
        {
            Target = target;
        }

        protected IObservableObject<T, W> Target { private set; get; }

        public string Name => Target.Name;

        public object Meta => Target.Meta;

        public IList<Func<IObjectWillChange, IObjectWillChange>> Interceptors => Target.Interceptors;

        public IList<Action<IObjectDidChange>> Listeners => Target.Listeners;

        public abstract T Proxy { get; }

        public object Get(string key)
        {
            return Target.Get(key);
        }

        public IDisposable Intercept(Func<IObjectWillChange, IObjectWillChange> interceptor)
        {
            return Target.Intercept(interceptor);
        }

        public IDisposable Observe(Action<IObjectDidChange> listener, bool force = false)
        {
            return Target.Observe(listener, force);
        }

        public P Read<P>(string key)
        {
            return Target.Read<P>(key);
        }

        public object Read(string key)
        {
            return Target.Read(key);
        }

        public bool Remove(string key)
        {
            return Target.Remove(key);
        }

        public bool TryRead(string key, out object value)
        {
            return Target.TryRead(key, out value);
        }

        public bool TryWrite(string key, object value, out object newValue)
        {
            return Target.TryWrite(key, value, out newValue);
        }

        public object Write<P>(string key, P value)
        {
            return Target.Write(key, value);
        }

        public object Write(string key, object value)
        {
            return Target.Write(key, value);
        }

        public void AddAction(string method, Func<object[], object> action)
        {
            Target.AddAction(method, action);
        }

        public bool TryInvokeAction(string action, object[] args, out object result)
        {
            return Target.TryInvokeAction(action, args, out result);
        }
    }

    public abstract class ObservableProxy<T> : ObservableProxy<T, object>, IObservableObject<T>
    {
        protected ObservableProxy(IObservableObject<T> target) : base(target)
        {
        }
    }
}
