using System;
using System.Collections.Generic;
using System.Text;

namespace Skclusive.Mobx.Observable
{
    public interface IReactionPublic : IDisposable
    {
        void Trace(bool enterBreakPoint = false);
    }
}
