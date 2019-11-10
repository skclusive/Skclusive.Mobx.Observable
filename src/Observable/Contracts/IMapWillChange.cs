namespace Skclusive.Mobx.Observable
{
    public interface IMapWillChange
    {
        object Name { get; }

        object Object { get; }

        object NewValue { get; }

        ChangeType Type { get; }
    }

    public interface IMapWillChange<TKey, TValue> : IMapWillChange
    {
        new TKey Name { get; }

        new TValue NewValue { get; set; }
    }
}
