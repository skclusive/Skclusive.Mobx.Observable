using System;
using System.Linq;

namespace Skclusive.Mobx.Observable
{
    public static class InterceptableExtension
    {
        public static bool HasInterceptors<T>(this IInterceptable<T> interceptable)
        {
            return interceptable.Interceptors.Count > 0;
        }

        public static IDisposable ResigerInterceptor<T>(this IInterceptable<T> interceptable, Func<T, T> interceptor)
        {
            interceptable.Interceptors.Add(interceptor);

            return new Disposable(() => interceptable.Interceptors.Remove(interceptor));
        }

        public static T NotifyInterceptors<T>(this IInterceptable<T> interceptable, T change)
        {
            var tracked = States.UntrackedStart();
            try
            {
                var interceptors = interceptable.Interceptors.ToList();
                foreach (var interceptor in interceptors)
                {
                    change = interceptor(change);
                    if (change == null)
                    {
                        break;
                    }
                }
                return change;
            }
            finally
            {
                States.UntrackedEnd(tracked);
            }
        }
    }
}
