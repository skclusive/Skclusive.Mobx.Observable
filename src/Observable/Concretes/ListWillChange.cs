using System.Collections.Generic;
using System.Linq;

namespace Skclusive.Mobx.Observable
{
    public class ListWillChange<TValue> : IListWillChange<TValue>
    {
        public ListWillChange(ChangeType type, object list)
        {
            Object = list;

            Type = type;
        }

        public TValue NewValue { set; get; }

        public object Object { private set; get; }

        public ChangeType Type { private set; get; }

        public TValue[] Added { set; get; }

        public int Index { protected set; get; }

        public int RemovedCount { protected set; get; }

        object IListWillChange.NewValue { get => NewValue; set => NewValue = (TValue)value; }

        object IListWillChange.Object { get => Object; }

        object[] IListWillChange.Added
        {
            get => Added.Select(added => (object)added).ToArray();

            set => Added = value.Select(added => (TValue)added).ToArray();
        }

        public static ListWillChange<TValue> Update(object source, int index, TValue newValue)
        {
            var change = new ListWillChange<TValue>(ChangeType.UPDATE, source);

            change.Index = index;

            change.NewValue = newValue;

            return change;
        }

        public static ListWillChange<TValue> Splice(object source, int index, TValue[] added, int removedCount)
        {
            var change = new ListWillChange<TValue>(ChangeType.SPLICE, source);

            change.Index = index;

            change.Added = added;

            change.RemovedCount = removedCount;

            return change;
        }
    }
}
