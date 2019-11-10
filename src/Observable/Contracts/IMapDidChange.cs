namespace Skclusive.Mobx.Observable
{
    public interface IMapDidChange : IMapWillChange
    {
        object OldValue { get; }
    }

    public interface IMapDidChange<TKey, TValue> : IMapDidChange, IMapWillChange<TKey, TValue>
    {
        new TValue OldValue { get; }
    }
}
