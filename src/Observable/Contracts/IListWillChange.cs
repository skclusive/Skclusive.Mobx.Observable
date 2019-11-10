using System.Collections.Generic;

namespace Skclusive.Mobx.Observable
{
    public interface IListWillChange
    {
        int Index { get; }

        object Object { get; }

        int RemovedCount { get; }

        ChangeType Type { get; }

        object NewValue { get; set; }

        object[] Added { get; set; }
    }

    public interface IListWillChange<TValue> : IListWillChange
    {
        new TValue NewValue { get; set; }

        new TValue[] Added { get; set; }
    }
}
