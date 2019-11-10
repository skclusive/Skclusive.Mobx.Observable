namespace Skclusive.Mobx.Observable
{
    public class ValueDidChange<T> : ValueWillChange<T>, IValueDidChange<T>
    {
        public ValueDidChange(T value, T oldvalue, object objectx) : base(value, objectx)
        {
            OldValue = oldvalue;
        }

        T IValueDidChange<T>.OldValue { get => (T)OldValue; }

        public object OldValue { private set; get; }
    }
}
