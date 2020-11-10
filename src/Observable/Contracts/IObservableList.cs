using System;
using System.Collections.Generic;

namespace Skclusive.Mobx.Observable
{
    public interface IObservableList<TIn, TOut> : IList<TOut>, IObservableMeta, IInterceptable<IListWillChange<TIn>>, IListenable<IListDidChange<TIn>>
    {
        new TOut[] Clear();

        int Length { get; set; }

        void Set(int index, TOut value);

        TOut Shift();

        void Unshift(TOut value);

        TOut[] Push(params TOut[] values);

        TOut Pop();

        TOut[] Replace(TOut[] newItems);

        TOut[] Splice(int index);

        TOut[] Splice(int index, int deleteCount, params TOut[] newItems);

        IEnumerable<TIn> GetValues();

        TIn Get(int index);

        int FindIndex(Predicate<TOut> match);
    }

    public interface IObservableList<T> : IObservableList<T, T>
    {
    }
}
