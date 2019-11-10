using System;

namespace Skclusive.Mobx.Observable
{
    public class Actions
    {
        #region Core Actions

        internal static object ExecuteAction(string name, Func<object[], object> action, object[] arguments)
        {
            IActionRunInfo actionInfo = StartAction(name, action, arguments);
            var shouldSupressReactionError = true;
            try
            {
                return action(arguments);
            }
            finally
            {

                if (shouldSupressReactionError)
                {
                    States.State.SuppressReactionErrors = shouldSupressReactionError;

                    EndAction(actionInfo);

                    States.State.SuppressReactionErrors = false;
                }
                else
                {
                    EndAction(actionInfo);
                }
            }
        }

        private static IActionRunInfo StartAction(string name, Func<object[], object> action, object[] arguments)
        {
            var derivation = States.UntrackedStart();

            States.StartBatch();

            var allowStateChanges = AllowStateChangesStart(true);

            var time = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            return new ActionRunInfo
            {
                Derivation = derivation,

                AllowStateChanges = allowStateChanges,

                NotifySpy = false,

                Started = time
            };
        }

        private static void EndAction(IActionRunInfo actionInfo)
        {
            AllowStateChangesEnd(actionInfo.AllowStateChanges);

            States.EndBatch();

            States.UntrackedEnd(actionInfo.Derivation);
        }

        public static T AllowStateChanges<T>(bool allowStateChanges, Func<T> action)
        {
            var previousAllowStateChanges = AllowStateChangesStart(allowStateChanges);

            try
            {
                return action();
            }
            finally
            {
                AllowStateChangesEnd(previousAllowStateChanges);
            }
        }

        public static bool AllowStateChangesStart(bool allowStateChanges)
        {
            var previous = States.State.AllowStateChanges;

            States.State.AllowStateChanges = allowStateChanges;

            return previous;
        }

        public static bool AllowStateChangesEnd(bool allowStateChangesPrevious)
        {
            States.State.AllowStateChanges = allowStateChangesPrevious;

            return allowStateChangesPrevious;
        }

        public static T AllowStateChangesInsideComputed<T>(Func<T> action)
        {
            var prev = States.State.ComputationDepth;

            States.State.ComputationDepth = 0;

            try
            {
                return action();
            }
            finally
            {
                States.State.ComputationDepth = prev;
            }
        }

        public static void AllowStateChangesInsideComputed(Action action)
        {
            AllowStateChangesInsideComputed<object>(() =>
            {
                action();
                return null;
            });
        }

        #endregion

        #region Func WrapActions

        public static Func<object[], object> WrapAction<R>(string name, Func<R> action)
        {
            return CreateAction(name, action.Pack());
        }

        public static Func<object[], object> WrapAction<A1, R>(string name, Func<A1, R> action)
        {
            return CreateAction(name, action.Pack());
        }

        public static Func<object[], object> WrapAction<A1, A2, R>(string name, Func<A1, A2, R> action)
        {
            return CreateAction(name, action.Pack());
        }

        public static Func<object[], object> WrapAction<A1, A2, A3, R>(string name, Func<A1, A2, A3, R> action)
        {
            return CreateAction(name, action.Pack());
        }

        public static Func<object[], object> WrapAction<A1, A2, A3, A4, R>(string name, Func<A1, A2, A3, A4, R> action)
        {
            return CreateAction(name, action.Pack());
        }

        public static Func<object[], object> WrapAction<A1, A2, A3, A4, A5, R>(string name, Func<A1, A2, A3, A4, A5, R> action)
        {
            return CreateAction(name, action.Pack());
        }

        #endregion

        #region Func CreateActions

        public static Func<object[], object> CreateAction(string name, Func<object[], object> action)
        {
            return (object[] arguments) =>
            {
                return ExecuteAction(name, action, arguments);
            };
        }

        public static Func<R> CreateAction<R>(string name, Func<R> action)
        {
            return WrapAction(name, action).Unpack<R>();
        }

        public static Func<A1, R> CreateAction<A1, R>(string name, Func<A1, R> action)
        {
            return WrapAction(name, action).Unpack<A1, R>();
        }

        public static Func<A1, A2, R> CreateAction<A1, A2, R>(string name, Func<A1, A2, R> action)
        {
            return WrapAction(name, action).Unpack<A1, A2, R>();
        }

        public static Func<A1, A2, A3, R> CreateAction<A1, A2, A3, R>(string name, Func<A1, A2, A3, R> action)
        {
            return WrapAction(name, action).Unpack<A1, A2, A3, R>();
        }

        public static Func<A1, A2, A3, A4, R> CreateAction<A1, A2, A3, A4, R>(string name, Func<A1, A2, A3, A4, R> action)
        {
            return WrapAction(name, action).Unpack<A1, A2, A3, A4, R>();
        }

        public static Func<A1, A2, A3, A4, A5, R> CreateAction<A1, A2, A3, A4, A5, R>(string name, Func<A1, A2, A3, A4, A5, R> action)
        {
            return WrapAction(name, action).Unpack<A1, A2, A3, A4, A5, R>();
        }

        #endregion

        #region Action WrapActions

        public static Action<object[]> WrapAction(string name, Action action)
        {
            return CreateAction(name, action.Pack());
        }

        public static Action<object[]> WrapAction<A1>(string name, Action<A1> action)
        {
            return CreateAction(name, action.Pack());
        }

        public static Action<object[]> WrapAction<A1, A2>(string name, Action<A1, A2> action)
        {
            return CreateAction(name, action.Pack());
        }

        public static Action<object[]> WrapAction<A1, A2, A3, A4>(string name, Action<A1, A2, A3, A4> action)
        {
            return CreateAction(name, action.Pack());
        }

        public static Action<object[]> WrapAction<A1, A2, A3>(string name, Action<A1, A2, A3> action)
        {
            return CreateAction(name, action.Pack());
        }

        public static Action<object[]> WrapAction<A1, A2, A3, A4, A5>(string name, Action<A1, A2, A3, A4, A5> action)
        {
            return CreateAction(name, action.Pack());
        }

        #endregion

        #region Action CreateActions

        public static Action<object[]> CreateAction(string name, Action<object[]> action)
        {
            var execute = CreateAction(name, (a) =>
            {
                action(a);
                return null;
            });

            return (object[] arguments) =>
            {
                execute(arguments);
            };
        }

        public static Action CreateAction(string name, Action action)
        {
            return WrapAction(name, action).Unpack();
        }

        public static Action<A1> CreateAction<A1>(string name, Action<A1> action)
        {
            return WrapAction(name, action).Unpack<A1>();
        }

        public static Action<A1, A2> CreateAction<A1, A2>(string name, Action<A1, A2> action)
        {
            return WrapAction(name, action).Unpack<A1, A2>();
        }

        public static Action<A1, A2, A3> CreateAction<A1, A2, A3>(string name, Action<A1, A2, A3> action)
        {
            return WrapAction(name, action).Unpack<A1, A2, A3>();
        }

        public static Action<A1, A2, A3, A4> CreateAction<A1, A2, A3, A4>(string name, Action<A1, A2, A3, A4> action)
        {
            return WrapAction(name, action).Unpack<A1, A2, A3, A4>();
        }

        public static Action<A1, A2, A3, A4, A5> CreateAction<A1, A2, A3, A4, A5>(string name, Action<A1, A2, A3, A4, A5> action)
        {
            return WrapAction(name, action).Unpack<A1, A2, A3, A4, A5>();
        }

        #endregion

        #region Func RunInActions

        public static object RunInAction(Func<object[], object> action, object[] arguments)
        {
            return RunInAction("<unnamed action>", action, arguments);
        }

        public static object RunInAction(string name, Func<object[], object> action, object[] arguments)
        {
            return CreateAction(name, action)(arguments);
        }

        public static R RunInAction<R>(string name, Func<R> action)
        {
            return CreateAction(name, action)();
        }

        public static R RunInAction<A1, R>(string name, Func<A1, R> action, A1 arg1)
        {
            return CreateAction(name, action)(arg1);
        }

        public static R RunInAction<A1, A2, R>(string name, Func<A1, A2, R> action, A1 arg1, A2 arg2)
        {
            return CreateAction(name, action)(arg1, arg2);
        }

        public static R RunInAction<A1, A2, A3, R>(string name, Func<A1, A2, A3, R> action, A1 arg1, A2 arg2, A3 arg3)
        {
            return CreateAction(name, action)(arg1, arg2, arg3);
        }

        public static R RunInAction<A1, A2, A3, A4, R>(string name, Func<A1, A2, A3, A4, R> action, A1 arg1, A2 arg2, A3 arg3, A4 arg4)
        {
            return CreateAction(name, action)(arg1, arg2, arg3, arg4);
        }

        public static R RunInAction<A1, A2, A3, A4, A5, R>(string name, Func<A1, A2, A3, A4, A5, R> action, A1 arg1, A2 arg2, A3 arg3, A4 arg4, A5 arg5)
        {
            return CreateAction(name, action)(arg1, arg2, arg3, arg4, arg5);
        }

        #endregion
    }
}
