namespace Skclusive.Mobx.Observable
{
    public class ObjectDidChange<T> : ObjectWillChange<T>, IObjectDidChange<T>
    {
        public ObjectDidChange(string name, ChangeType type, T value, T oldvalue, object objectx) :
            base(name, type, value, objectx)
        {
            OldValue = oldvalue;
        }

        public T OldValue { private set; get; }

        object IObjectDidChange.OldValue { get => OldValue; }
    }
}
