using System;
using System.Collections.Generic;

namespace Skclusive.Mobx.Observable
{
    public partial class Reactions
    {
        private readonly static object Null = new object();

        public static IDisposable SetTimeout(Action action, int delay)
        {
            return ExecutionPlan.Delay(delay, action);
        }

        private static Action<Action> CreateScheduler(IAutorunOptions options)
        {
            if (options?.Scheduler != null)
            {
                return options.Scheduler;
            }

            if (options?.Delay > 0)
            {
                return (action) =>
                {
                    SetTimeout(action, options.Delay);
                };
            }

            return (action) => action();
        }

        public static IReactionDisposable Autorun(Action view, IAutorunOptions options = null)
        {
            return Autorun(r => view(), options);
        }

        /**
         * Creates a named reactive view and keeps it alive, so that the view is always
         * updated if one of the dependencies changes, even when the view is not further used by something else.
         * @param view The reactive view
         * @returns disposer function, which can be used to stop the view from being updated in the future.
         */
        public static IReactionDisposable Autorun(Action<IReactionPublic> view, IAutorunOptions options = null)
        {
            var name = options?.Name ?? $"Autorun@{States.NextId}";
            var runSync = options == null || options.Scheduler != null && options.Delay == 0;

            Reaction _reaction = null;

            if (runSync)
            {
                _reaction = new Reaction(name, (reaction) =>
                {
                    reaction.Track(() => ReactionRunner(reaction));
                }, options?.OnError);
            }
            else
            {
                var scheduler = CreateScheduler(options);

                // debounced autorun
                var isScheduled = false;
                _reaction = new Reaction(name, (reaction) =>
                {
                    if (!isScheduled)
                    {
                        isScheduled = true;
                        scheduler(() =>
                        {
                            isScheduled = false;
                            if (!reaction.IsDisposed)
                            {
                                reaction.Track(() => ReactionRunner(reaction));
                            }
                        });
                    }
                }, options?.OnError);
            }

            object ReactionRunner(Reaction reaction)
            {
                view(reaction);
                return null;
            }

            _reaction.Schedule();

            return _reaction.GetDisposable();
        }

        public static IReactionDisposable Reaction<T>(Func<IReactionPublic, T> expression, Action<T, IReactionPublic> effect, IReactionOptions<T> options = null)
        {
            var name = options?.Name ?? $"Reaction@{States.NextId}";
            var runSync = options == null || options.Scheduler != null && options.Delay == 0;
            var scheduler = CreateScheduler(options);

            var firstTime = true;
            var isScheduled = false;
            T value = default(T);

            var comparer = options?.Comparer ?? EqualityComparer<T>.Default;

            var _reaction = new Reaction(
                 name,
                 (reaction) =>
                 {
                     if (firstTime || runSync)
                     {
                         reactionRunner(reaction);
                     }
                     else if (!isScheduled)
                     {
                         isScheduled = true;
                         scheduler(() => reactionRunner(reaction));
                     }
                 },
                 options?.OnError
             );

            void reactionRunner(Reaction reaction)
            {
                isScheduled = false; // Q: move into reaction runner?
                if (reaction.IsDisposed)
                {
                    return;
                }
                var changed = false;
                reaction.Track(() =>
                {
                    var nextValue = expression(reaction);
                    changed = firstTime || !comparer.Equals(value, nextValue) || Null == (object)nextValue;
                    value = nextValue;
                    return value;
                });
                var firImmediately = options?.FireImmediately;
                if (firstTime && firImmediately.HasValue && firImmediately.Value)
                {
                    effect(value, reaction);
                }
                if (!firstTime && changed)
                {
                    effect(value, reaction);
                }
                if (firstTime)
                {
                    firstTime = false;
                }
            }

            _reaction.Schedule();

            return _reaction.GetDisposable();
        }

        public static IReactionDisposable Reaction(Action expression, Action effect, IReactionOptions<object> options = null)
        {
            return Reaction((reaction) =>
            {
                expression();
                return Null;
            }, (value, reaction) =>
            {
                effect();
            }, new ReactionOptions<object>()
            {
                Comparer = new LambdaComparer<object>((a, b) => false)
            });
        }
        public static T Transaction<T>(Func<T> action)
        {
            try
            {
                States.StartBatch();
                return action();
            }
            finally
            {
                States.EndBatch();
            }
        }

        public static void Transaction(Action action)
        {
            Transaction(() =>
            {
                action();
                return Null;
            });
        }
    }
}
