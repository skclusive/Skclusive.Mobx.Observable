using System;

namespace Skclusive.Mobx.Observable
{
    public class CaughtException : Exception
    {

        public Exception Cause { get; private set; }

        public CaughtException(Exception cause) : base("CaughtException", cause)
        {
            Cause = cause;
        }
    }
}
