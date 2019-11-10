using ImpromptuInterface;
using System;

namespace Skclusive.Mobx.Observable.Tests
{
    public static class ObservableObjectExtension
    {
        public static TInterface ActAs<TInterface>(this IObservableObject observable, params Type[] otherInterfaces) where TInterface : class
        {
            return observable.ActLike<TInterface>(otherInterfaces);
        }
    }
}
