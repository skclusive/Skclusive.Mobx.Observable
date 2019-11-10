using System.Collections.Generic;

namespace Skclusive.Mobx.Observable
{

    /// <summary>
    /// A dictionary that remembers the order that keys were first inserted. If a new entry overwrites an existing entry, the original insertion position is left unchanged. Deleting an entry and reinserting it will move it to the end.
    /// </summary>
    /// <typeparam name="TKey">The type of keys</typeparam>
    /// <typeparam name="TValue">The type of values</typeparam>
    public interface IOrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        /// <summary>
        /// The value of the element at the given index.
        /// </summary>
        TValue this[int index] { get; set; }

        /// <summary>
        /// Find the position of an element by key. Returns -1 if the dictionary does not contain an element with the given key.
        /// </summary>
        int IndexOf(TKey key);

        /// <summary>
        /// Insert an element at the given index.
        /// </summary>
        void Insert(int index, TKey key, TValue value);

        /// <summary>
        /// Remove the element at the given index.
        /// </summary>
        void RemoveAt(int index);
    }
}
