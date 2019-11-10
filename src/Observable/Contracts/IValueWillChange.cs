namespace Skclusive.Mobx.Observable
{
    public interface IValueWillChange
    {
        object Object { get; }

        object NewValue { get; }
    }

    public interface IValueWillChange<T> : IValueWillChange
    {
        new T NewValue { get; }
    }
}
