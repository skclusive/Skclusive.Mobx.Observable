namespace Skclusive.Mobx.Observable
{
    public class MapDidChange<TKey, TValue> : MapWillChange<TKey, TValue>, IMapDidChange<TKey, TValue>
    {
        public MapDidChange(TKey name, ChangeType type, TValue value, TValue oldvalue, object map) :
            base(name, type, value, map)
        {
            OldValue = oldvalue;
        }

        public TValue OldValue { private set; get; }

        object IMapDidChange.OldValue { get => OldValue; }
    }
}
