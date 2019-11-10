using System.Collections.Generic;
using System.Linq;

namespace Skclusive.Mobx.Observable
{
    public class ListDidChange<TValue> : ListWillChange<TValue>, IListDidChange<TValue>
    {
        public ListDidChange(ChangeType type, object list) : base(type, list)
        {
        }

        public TValue OldValue { private set; get; }

        public TValue[] Removed { private set; get; }

        public int AddedCount { private set; get; }

        object IListDidChange.OldValue { get => OldValue; }

        object[] IListDidChange.Removed { get => Removed.Select(removed => (object)removed).ToArray(); }


        public static ListDidChange<TValue> Splice(object source, int index, TValue[] added, int addedCount, TValue[] removed, int removedCount)
        {
            var change = new ListDidChange<TValue>(ChangeType.SPLICE, source);

            change.Index = index;

            change.Added = added;

            change.AddedCount = addedCount;

            change.Removed = removed;

            change.RemovedCount = removedCount;

            return change;
        }

        public static ListDidChange<TValue> Update(object source, int index, TValue newValue, TValue oldValue)
        {
            var change = new ListDidChange<TValue>(ChangeType.UPDATE, source);

            change.Index = index;

            change.NewValue = newValue;

            change.OldValue = oldValue;

            return change;
        }
    }
}
