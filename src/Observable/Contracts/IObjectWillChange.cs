namespace Skclusive.Mobx.Observable
{
    public interface IObjectWillChange
    {
        string Name { get; }

        object Object { get; }

        object NewValue { get; set; }

        ChangeType Type { get; }
    }

    public interface IObjectWillChange<T> : IObjectWillChange
    {
        new T NewValue { get; set; }
    }
}
