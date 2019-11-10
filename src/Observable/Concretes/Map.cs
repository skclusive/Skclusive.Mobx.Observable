using System.Collections.Generic;
using System.Linq;

namespace Skclusive.Mobx.Observable
{
    public class Map<TKey, TValue> : OrderedDictionary<TKey, TValue>, IMap<TKey, TValue>
    {
        public Map()
        {
        }

        public Map(IMap<TKey, TValue> map) : base(map)
        {
        }

        public Map(IDictionary<TKey, TValue> dictionary) : base(dictionary)
        {
        }

        public Map(IEqualityComparer<TKey> comparer) : base(comparer)
        {
        }

        public Map(int capacity) : base(capacity)
        {
        }

        public Map(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer)
        {
        }

        public Map(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer)
        {
        }

        public IMap<TKey, TValue> Replace(IDictionary<TKey, TValue> dictionary)
        {
            var newKeys = dictionary.Keys;
            var oldKeys = Keys;

            var missingKeys = oldKeys.Where(key => !newKeys.Contains(key));

            foreach (var key in missingKeys)
            {
                Remove(key);
            }

            foreach (var entry in dictionary)
            {
                this[entry.Key] = entry.Value;
            }

            return this;
        }
    }
}
