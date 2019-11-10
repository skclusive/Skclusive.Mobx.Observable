using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skclusive.Mobx.Observable
{
    public static class ListenableExtension
    {
        public static bool HasListeners<T>(this IListenable<T> listenable)
        {
            return listenable.Listeners.Count > 0;
        }

        public static IDisposable ResigerListener<T>(this IListenable<T> listenable, Action<T> listener)
        {
            listenable.Listeners.Add(listener);

            return new Disposable(() => listenable.Listeners.Remove(listener));
        }

        public static void NotifyListeners<T>(this IListenable<T> listenable, T change)
        {
            var tracked = States.UntrackedStart();
            var listeners = listenable.Listeners.ToList();
            foreach (var listener in listeners)
            {
                listener(change);
            }
            States.UntrackedEnd(tracked);
        }
    }
}
