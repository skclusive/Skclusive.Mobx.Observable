using System;

namespace Skclusive.Mobx.Observable
{
    public static class FuncExtensions
    {
        #region Packs

        public static Func<object[], object> Pack<R>(this Func<R> action)
        {
            return args => action();
        }

        public static Func<object[], object> Pack<A1, R>(this Func<A1, R> action)
        {
            return args => action((A1)args[0]);
        }

        public static Func<object[], object> Pack<A1, A2, R>(this Func<A1, A2, R> action)
        {
            return args => action((A1)args[0], (A2)args[1]);
        }

        public static Func<object[], object> Pack<A1, A2, A3, R>(this Func<A1, A2, A3, R> action)
        {
            return args => action((A1)args[0], (A2)args[1], (A3)args[2]);
        }

        public static Func<object[], object> Pack<A1, A2, A3, A4, R>(this Func<A1, A2, A3, A4, R> action)
        {
            return args => action((A1)args[0], (A2)args[1], (A3)args[2], (A4)args[3]);
        }

        public static Func<object[], object> Pack<A1, A2, A3, A4, A5, R>(this Func<A1, A2, A3, A4, A5, R> action)
        {
            return args => action((A1)args[0], (A2)args[1], (A3)args[2], (A4)args[3], (A5)args[4]);
        }

        #endregion

        #region Unpack

        public static Func<R> Unpack<R>(this Func<object[], object> action)
        {
            return () => (R)action(null);
        }

        public static Func<A1, R> Unpack<A1, R>(this Func<object[], object> action)
        {
            return (A1 arg1) => (R)action(new object[] { arg1 });
        }

        public static Func<A1, A2, R> Unpack<A1, A2, R>(this Func<object[], object> action)
        {
            return (A1 arg1, A2 arg2) => (R)action(new object[] { arg1, arg2 });
        }

        public static Func<A1, A2, A3, R> Unpack<A1, A2, A3, R>(this Func<object[], object> action)
        {
            return (A1 arg1, A2 arg2, A3 arg3) => (R)action(new object[] { arg1, arg2, arg3 });
        }

        public static Func<A1, A2, A3, A4, R> Unpack<A1, A2, A3, A4, R>(this Func<object[], object> action)
        {
            return (A1 arg1, A2 arg2, A3 arg3, A4 arg4) => (R)action(new object[] { arg1, arg2, arg3, arg4 });
        }

        public static Func<A1, A2, A3, A4, A5, R> Unpack<A1, A2, A3, A4, A5, R>(this Func<object[], object> action)
        {
            return (A1 arg1, A2 arg2, A3 arg3, A4 arg4, A5 arg5) => (R)action(new object[] { arg1, arg2, arg3, arg4, arg5 });
        }

        #endregion
    }
}
