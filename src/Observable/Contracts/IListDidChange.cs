namespace Skclusive.Mobx.Observable
{
    public interface IListDidChange : IListWillChange
    {
        object OldValue { get; }


        int AddedCount { get; }

        object[] Removed { get; }
    }

    public interface IListDidChange<TValue> : IListDidChange, IListWillChange<TValue>
    {
        new TValue OldValue { get; }

        new TValue[] Removed { get; }
    }
}
