using System;

namespace Skclusive.Mobx.Observable
{
    public interface IAutorunOptions
    {
        int Delay { get; }

        string Name { get; }

        Action<Action> Scheduler { get; }

        Action<object, IDerivation> OnError { get; }
    }
}
