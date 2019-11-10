using System;

namespace Skclusive.Mobx.Observable
{
    public class AutorunOptions : IAutorunOptions
    {
        public int Delay { get; set; }

        public string Name { get; set; }

        public Action<Action> Scheduler { get; set; }

        public Action<object, IDerivation> OnError { get; set; }
    }
}
