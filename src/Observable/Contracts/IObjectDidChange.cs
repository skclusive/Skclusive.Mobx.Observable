namespace Skclusive.Mobx.Observable
{
    public interface IObjectDidChange : IObjectWillChange
    {
        object OldValue { get; }
    }

    public interface IObjectDidChange<T> : IObjectDidChange, IObjectWillChange<T>
    {
        new T OldValue { get; }
    }
}
