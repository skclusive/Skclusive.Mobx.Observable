namespace Skclusive.Mobx.Observable
{
    public class MapWillChange<TKey, TValue> : IMapWillChange<TKey, TValue>
    {
        public MapWillChange(TKey name, ChangeType type, TValue value, object map)
        {
            Name = name;

            NewValue = value;

            Object = map;

            Type = type;
        }

        public TValue NewValue { set; get; }

        public object Object { private set; get; }

        public TKey Name { private set; get; }

        public ChangeType Type { private set; get; }

        object IMapWillChange.NewValue { get => NewValue; }

        object IMapWillChange.Name { get => Name; }

        object IMapWillChange.Object { get => Object; }
    }
}
