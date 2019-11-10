using System;

namespace Skclusive.Mobx.Observable
{
    public static class ActionExtensions
    {
        #region Packs

        public static Action<object[]> Pack(this Action action)
        {
            return args => action();
        }

        public static Action<object[]> Pack<A1>(this Action<A1> action)
        {
            return args => action((A1)args[0]);
        }

        public static Action<object[]> Pack<A1, A2>(this Action<A1, A2> action)
        {
            return args => action((A1)args[0], (A2)args[1]);
        }

        public static Action<object[]> Pack<A1, A2, A3, A4>(this Action<A1, A2, A3, A4> action)
        {
            return args => action((A1)args[0], (A2)args[1], (A3)args[2], (A4)args[3]);
        }

        public static Action<object[]> Pack<A1, A2, A3>(this Action<A1, A2, A3> action)
        {
            return args => action((A1)args[0], (A2)args[1], (A3)args[2]);
        }

        public static Action<object[]> Pack<A1, A2, A3, A4, A5>(this Action<A1, A2, A3, A4, A5> action)
        {
            return args => action((A1)args[0], (A2)args[1], (A3)args[2], (A4)args[3], (A5)args[4]);
        }

        #endregion

        #region Unpack

        public static Action Unpack(this Action<object[]> action)
        {
            return () => action(null);
        }

        public static Action<A1> Unpack<A1>(this Action<object[]> action)
        {
            return (A1 arg1) => action(new object[] { arg1 });
        }

        public static Action<A1, A2> Unpack<A1, A2>(this Action<object[]> action)
        {
            return (A1 arg1, A2 arg2) => action(new object[] { arg1, arg2 });
        }

        public static Action<A1, A2, A3> Unpack<A1, A2, A3>(this Action<object[]> action)
        {
            return (A1 arg1, A2 arg2, A3 arg3) => action(new object[] { arg1, arg2, arg3 });
        }

        public static Action<A1, A2, A3, A4> Unpack<A1, A2, A3, A4>(this Action<object[]> action)
        {
            return (A1 arg1, A2 arg2, A3 arg3, A4 arg4) => action(new object[] { arg1, arg2, arg3, arg4 });
        }

        public static Action<A1, A2, A3, A4, A5> Unpack<A1, A2, A3, A4, A5>(this Action<object[]> action)
        {
            return (A1 arg1, A2 arg2, A3 arg3, A4 arg4, A5 arg5) => action(new object[] { arg1, arg2, arg3, arg4, arg5 });
        }

        #endregion
    }
}
