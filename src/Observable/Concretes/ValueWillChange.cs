namespace Skclusive.Mobx.Observable
{
    public class ValueWillChange<T> : IValueWillChange<T>
    {
        public ValueWillChange(T value, object objectx)
        {
            NewValue = value;

            Object = objectx;
        }

        public T NewValue { private set; get; }

        public object Object { private set; get; }

        object IValueWillChange.NewValue { get => NewValue; }
    }
}
