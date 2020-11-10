using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Skclusive.Mobx.Observable
{
    public class VolatileProperty
    {
        public string Name { set; get; }

        public Type Type { set; get; }

        public object Default { set; get; }
    }

    public class ObservableProperty
    {
        public string Name { set; get; }

        public Type Type { set; get; }

        public object Default { set; get; }
    }

    public class ComputedProperty
    {
        public string Name { set; get; }

        public Type Type { set; get; }

        public Func<object, object> Compute { set; get; }
    }

    public class ActionMethod
    {
        public string Name { set; get; }

        public Func<object[], object> Action { set; get; }
    }

    public class ObservableTypeDef
    {
        public ObservableTypeDef(
            IEnumerable<ObservableProperty> observables,
            IEnumerable<VolatileProperty> volatiles,
            IEnumerable<ComputedProperty> computeds,
            IEnumerable<ActionMethod> actions = null)
        {
            Observables = observables;

            Volatiles = volatiles;

            Computeds = computeds;

            Actions = actions ?? Enumerable.Empty<ActionMethod>();
        }

        public IEnumerable<ObservableProperty> Observables { private set; get; }

        public IEnumerable<VolatileProperty> Volatiles { private set; get; }

        public IEnumerable<ComputedProperty> Computeds { private set; get; }

        public IEnumerable<ActionMethod> Actions { private set; get; }
    }

    public class ObservableTypeDefBuilder<T>
    {
        private readonly ISet<ObservableProperty> ObservableProperties = new HashSet<ObservableProperty>();

        private readonly ISet<VolatileProperty> VolatileProperties = new HashSet<VolatileProperty>();

        private readonly ISet<ComputedProperty> ComputedProperties = new HashSet<ComputedProperty>();

        private readonly ISet<ActionMethod> ActionMethods = new HashSet<ActionMethod>();

        public ObservableTypeDefBuilder<T> Observable<I>(Expression<Func<T, I>> expresion, I defaultValue = default(I))
        {
            var property = ExpressionUtils.GetPropertySymbol(expresion);

            ObservableProperties.Add(new ObservableProperty
            {
                Name = property,

                Default = defaultValue,

                Type = typeof(I)
            });

            return this;
        }

        public ObservableTypeDefBuilder<T> Volatile<I>(Expression<Func<T, I>> expresion, I defaultValue = default(I))
        {
            var property = ExpressionUtils.GetPropertySymbol(expresion);

            VolatileProperties.Add(new VolatileProperty
            {
                Name = property,

                Default = defaultValue,

                Type = typeof(I)
            });

            return this;
        }

        public ObservableTypeDefBuilder<T> Computed<I>(Expression<Func<T, I>> expresion, Func<T, I> compute)
        {
            var property = ExpressionUtils.GetPropertySymbol<T, I>(expresion);

            ComputedProperties.Add(new ComputedProperty
            {
                Name = property,

                Compute = (obj) =>
                {
                    return compute((T)obj);
                },

                Type = typeof(I)
            });

            return this;
        }

        private ObservableTypeDefBuilder<T> Action(string name, Func<object[], object> action)
        {
            ActionMethods.Add(new ActionMethod
            {
                Name = name,

                Action = action
            });

            return this;
        }

        private ObservableTypeDefBuilder<T> Action(string name, Action<object[]> action)
        {
            return Action(name, (arguments) =>
            {
                action(arguments);
                return null;
            });
        }

        public ObservableTypeDefBuilder<T> Action<A1, R>(Expression<Action<T>> expression, Func<T, A1, R> func)
        {
            var method = ExpressionUtils.GetMethodSymbol(expression);
            return Action(method, Actions.WrapAction(method, func));
        }

        public ObservableTypeDefBuilder<T> Action<A1, A2, R>(Expression<Action<T>> expression, Func<T, A1, A2, R> func)
        {
            var method = ExpressionUtils.GetMethodSymbol(expression);
            return Action(method, Actions.WrapAction(method, func));
        }

        public ObservableTypeDefBuilder<T> Action<A1, A2, A3, R>(Expression<Action<T>> expression, Func<T, A1, A2, A3, R> func)
        {
            var method = ExpressionUtils.GetMethodSymbol(expression);
            return Action(method, Actions.WrapAction(method, func));
        }

        public ObservableTypeDefBuilder<T> Action<A1, A2, A3, A4, R>(Expression<Action<T>> expression, Func<T, A1, A2, A3, A4, R> func)
        {
            var method = ExpressionUtils.GetMethodSymbol(expression);
            return Action(method, Actions.WrapAction(method, func));
        }

        public ObservableTypeDefBuilder<T> Action<A1>(Expression<Action<T>> expression, Action<T, A1> func)
        {
            var method = ExpressionUtils.GetMethodSymbol(expression);
            return Action(method, Actions.WrapAction(method, func));
        }

        public ObservableTypeDefBuilder<T> Action<A1, A2>(Expression<Action<T>> expression, Action<T, A1, A2> func)
        {
            var method = ExpressionUtils.GetMethodSymbol(expression);
            return Action(method, Actions.WrapAction(method, func));
        }

        public ObservableTypeDefBuilder<T> Action<A1, A2, A3>(Expression<Action<T>> expression, Action<T, A1, A2, A3> func)
        {
            var method = ExpressionUtils.GetMethodSymbol(expression);
            return Action(method, Actions.WrapAction(method, func));
        }

        public ObservableTypeDefBuilder<T> Action<A1, A2, A3, A4>(Expression<Action<T>> expression, Action<T, A1, A2, A3, A4> func)
        {
            var method = ExpressionUtils.GetMethodSymbol(expression);
            return Action(method, Actions.WrapAction(method, func));
        }

        public ObservableTypeDef Build()
        {
            return new ObservableTypeDef(ObservableProperties, VolatileProperties, ComputedProperties, ActionMethods);
        }
    }
}
