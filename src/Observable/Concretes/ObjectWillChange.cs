namespace Skclusive.Mobx.Observable
{
    public class ObjectWillChange<T> : IObjectWillChange<T>
    {
        public ObjectWillChange(string name, ChangeType type, T value, object objectx)
        {
            Name = name;

            NewValue = value;

            Object = objectx;

            Type = type;
        }

        public T NewValue { set; get; }

        public object Object { private set; get; }

        public string Name { private set; get; }

        public ChangeType Type { private set; get; }

        object IObjectWillChange.NewValue { get => NewValue; set => NewValue = (T)value; }
    }
}
