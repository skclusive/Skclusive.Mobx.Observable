using System;
using System.Collections.Generic;

namespace Skclusive.Mobx.Observable
{
    public interface IInterceptable<T>
    {
        IList<Func<T, T>> Interceptors { get; }

        IDisposable Intercept(Func<T, T> interceptor);
    }
}
