using System.Collections.Generic;
using Skclusive.Core.Collection;

namespace Skclusive.Mobx.Observable
{
    public interface IObservableMap<TKey, TIn, TOut> : IMap<TKey, TOut>, IObservableMeta, IInterceptable<IMapWillChange<TKey, TIn>>, IListenable<IMapDidChange<TKey, TIn>>
    {
        string Name { get; }

        bool Has(TKey key);

        IObservableMap<TKey, TIn, TOut> Merge(IDictionary<TKey, TOut> values);

        new IObservableMap<TKey, TIn, TOut> Replace(IDictionary<TKey, TOut> dictionary);

        IEnumerable<TIn> GetValues();

        TIn GetValue(TKey key);

        IEnumerable<KeyValuePair<TKey, TIn>> GetPairs();
    }

    public interface IObservableMap<TKey, TIn> : IObservableMap<TKey, TIn, TIn>
    {
    }
}
