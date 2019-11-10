namespace Skclusive.Mobx.Observable
{
    public interface IValueDidChange : IValueWillChange
    {
        object OldValue { get; }
    }

    public interface IValueDidChange<T> : IValueDidChange, IValueWillChange<T>
    {
        new T OldValue { get; }
    }
}
