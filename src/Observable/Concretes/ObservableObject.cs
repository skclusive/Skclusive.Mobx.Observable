using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Skclusive.Mobx.Observable
{
    public class ObservableObject<T, W> : DynamicObject, IDepTreeNodeClassifier, IDepTreeNodeFinder, IObservableObject<T, W> where W : class
    {
        private IAtom KeysAtom { set; get; }

        IDepTreeNode IDepTreeNodeClassifier.Node => KeysAtom;

        DepTreeNodeType IDepTreeNodeClassifier.AtomType => DepTreeNodeType.Object;

        public object Target { private set; get; }

        private IDictionary<string, IObservableValue<bool>> PendingKeys { set; get; }

        private IDictionary<string, IObservable> Values { set; get; }

        public string Name { private set; get; }

        private IManipulator<W, object> Manipulator { set; get; }

        public T Proxy { private set; get; }

        public IList<Func<IObjectWillChange, IObjectWillChange>> Interceptors { private set; get; } = new List<Func<IObjectWillChange, IObjectWillChange>>();

        public IList<Action<IObjectDidChange>> Listeners { private set; get; } = new List<Action<IObjectDidChange>>();

        private ObservableObject(object target, IDictionary<string, IObservable> values,
            Func<IObservableObject<T, W>, T> proxify, string name = null,
            IManipulator<W, object> manipulator = null, params Type[] otherTypes)
        {
            if (!typeof(T).IsInterface)
            {
                throw new ArgumentException($"{typeof(T).Name} should be interface");
            }

            if (proxify == null)
            {
                throw new ArgumentException($"{nameof(proxify)} should not be Null");
            }

            Target = target;

            Values = values ?? new Dictionary<string, IObservable>();

            Name = name ?? $"ObservableObject@{States.NextId}";

            Manipulator = manipulator ?? Manipulator<W, object>.For();

            KeysAtom = new Atom($"{Name}.keys");

            Proxy = proxify(this);
        }

        public ObservableObject(ObservableTypeDef typeDef, IDictionary<string, IObservable> values,
            Func<IObservableObject<T, W>, T> proxify, string name,
            IManipulator<W, object> manipulator = null, params Type[] otherTypes)
            : this((object)null, values, proxify, name, manipulator, otherTypes)
        {
            var addObservable = ExpressionUtils.GetMethod<ObservableObject<T, W>>(x => x.AddObservableProperty<object>("", null, null));

            var isObject = typeof(W) == typeof(object);

            foreach (var observable in typeDef.Observables)
            {
                var add = addObservable.MakeGenericMethod(observable.Type);

                var value = isObject ? observable.Default : default(W);

                add.Invoke(this, new object[] { observable.Name, value, manipulator });
            }

            var addComputed = ExpressionUtils.GetMethod<ObservableObject<T, W>>(x => x.AddComputedPropertyRaw<object>("", null));

            foreach (var computed in typeDef.Computeds)
            {
                var add = addComputed.MakeGenericMethod(computed.Type);

                add.Invoke(this, new object[] { computed.Name, computed.Compute });
            }

            foreach (var action in typeDef.Actions)
            {
                AddActionMethod(action.Name, action.Action);
            }
        }

        //public ObservableObject(T target, IDictionary<string, IObservable> values, Func<IObservableObject<T, W>, T> proxify, string name,
        //    IManipulator<W, object> manipulator = null, params Type[] otherTypes)
        //    : this((object)target, values, proxify, name, manipulator, otherTypes)
        //{
        //    var addObservable = ExpressionUtils.GetMethod<ObservableObject<T, W>>(x => x.AddObservableProperty<object>("", (W)null, null));

        //    var properties = typeof(T).GetProperties();

        //    var observables = properties.Where(p => p.GetGetMethod() != null);

        //    foreach (var observable in observables)
        //    {
        //        var add = addObservable.MakeGenericMethod(observable.PropertyType);

        //        add.Invoke(this, new object[] { observable.Name, observable.GetValue(target), null });
        //    }

        //    var addComputed = ExpressionUtils.GetMethod<ObservableObject<T, W>>(x => x.AddComputedProperty<object>("", null));

        //    var makeGet = ExpressionUtils.GetMethod<ObservableObject<T, W>>(x => ExpressionUtils.MakeGetDelegate<object, object>(null));

        //    var computeds = properties.Where(p => p.GetSetMethod() == null);

        //    foreach (var computed in computeds)
        //    {
        //        var getterGeneric = makeGet.MakeGenericMethod(typeof(T), computed.PropertyType);

        //        var getter = getterGeneric.Invoke(null, new object[] { computed.GetGetMethod() });

        //        var add = addComputed.MakeGenericMethod(computed.PropertyType);

        //        add.Invoke(this, new object[] { computed.Name, getter });
        //    }
        //}

        //public static IObservableObject<T, W> From(T target, Func<IObservableObject<T, W>, T> proxify, string name, params Type[] otherTypes)
        //{
        //    return new ObservableObject<T, W>(target, null, proxify, name, null, otherTypes);
        //}

        //public static T FromAs(T target, Func<IObservableObject<T, W>, T> proxify, string name, params Type[] otherTypes)
        //{
        //    return From(target, proxify, name, otherTypes).Proxy;
        //}

        public static IObservableObject<T, W> From(ObservableTypeDef typeDef, Func<IObservableObject<T, W>, T> proxify, string name, IManipulator<W, object> manipulator = null, params Type[] otherTypes)
        {
            return new ObservableObject<T, W>(typeDef, null, proxify, name, manipulator, otherTypes);
        }

        public static T FromAs(ObservableTypeDef typeDef, Func<IObservableObject<T, W>, T> proxify, string name, IManipulator<W, object> manipulator = null, params Type[] otherTypes)
        {
            return From(typeDef, proxify, name, manipulator, otherTypes).Proxy;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return TryWrite(binder.Name, value, out object newValue);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return TryRead(binder.Name, out result);
        }

        public override bool TryDeleteMember(DeleteMemberBinder binder)
        {
            return Remove(binder.Name);
        }

        //public override bool TryConvert(ConvertBinder binder, out object result)
        //{
        //    if (typeof(T) == binder.Type)
        //    {
        //        result = this;
        //    }
        //    else
        //    {
        //        result = null;
        //    }

        //    return result != null;
        //}

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            return base.TryInvoke(binder, args, out result);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (TryInvokeAction(binder.Name, args, out result))
            {
                return true;
            }

            return base.TryInvokeMember(binder, args, out result);
        }

        public bool TryInvokeAction(string name, object[] args, out object result)
        {
            if (Values.TryGetValue(name, out IObservable observable))
            {
                if (observable is IObservableAction action)
                {
                    result = action.Execute(args);

                    return true;
                }
            }
            result = null;

            return false;
        }

        public bool TryRead(string key, out object value)
        {
            value = null;

            if (!Values.ContainsKey(key))
            {
                return false;
            }

            var observable = Values[key];

            if (observable is IValueReader reader)
            {
                value = reader.Value;

                return true;
            }

            return false;
        }

        public object Get(string key)
        {
            if (!Values.ContainsKey(key))
            {
                return null;
            }

            var observable = Values[key];

            if (observable is IComputedValue computed)
            {
                return computed.Value;
            }

            if (observable is IObservableValue value)
            {
                return value.Value;
            }

            return null;
        }

        public bool TryWrite(string key, object value, out object newValue)
        {
            newValue = value;

            if (!Values.ContainsKey(key))
            {
                return false;
            }

            var observable = Values[key];

            if (observable is IComputedValue computed)
            {
                throw new InvalidOperationException($"Property {key} is computed value {computed.ToString()} and can not be updated");
            }

            if (this.HasInterceptors())
            {
                var change = this.NotifyInterceptors(new ObjectWillChange<object>(key, ChangeType.UPDATE, value, Proxy));
                if (change == null)
                {
                    // TODO: investigate with tests
                    return true;
                }

                value = change.NewValue;
            }

            if (observable is IObservableValue obsvalue)
            {
                var oldValue = obsvalue.Value;
                if (obsvalue.PrepareNewValue(oldValue, value, out object changedValue))
                {
                    (obsvalue as IValueWriter).Value = changedValue;

                    newValue = changedValue;

                    if (this.HasListeners())
                    {
                        this.NotifyListeners(new ObjectDidChange<object>(key, ChangeType.UPDATE, newValue, oldValue, Proxy));
                    }
                }

                return true;
            }

            return false;
        }

        public bool Has(string key)
        {
            if (Values.TryGetValue(key, out IObservable value))
            {
                return true;
            }

            WaitForKey(key);

            return false;
        }

        private void WaitForKey(string key)
        {
            var keys = PendingKeys ?? (PendingKeys = new Dictionary<string, IObservableValue<bool>>());
            var entry = keys.ContainsKey(key) ? keys[key] : null;
            if (entry == null)
            {
                entry = ObservableValue<bool>.From(false, $"{Name}.{key}?");
                keys[key] = entry;
            }
            // read to subscribe
            var x = (entry as IValueReader<bool>).Value;
        }

        public bool Remove(string key)
        {
            if (!Values.ContainsKey(key))
            {
                return false;
            }

            if (this.HasInterceptors())
            {
                var change = this.NotifyInterceptors(new ObjectWillChange<object>(key, ChangeType.REMOVE, null, this));
                if (change == null)
                {
                    return false;
                }
            }

            Reactions.Transaction(() =>
            {
                var oobservable = Values[key];
                var value = (oobservable as IValueReader).Value;
                (oobservable as IValueWriter).Value = null;

                KeysAtom.ReportChanged();

                Values.Remove(key);

                if (this.HasListeners())
                {
                    this.NotifyListeners(new ObjectDidChange<object>(key, ChangeType.REMOVE, null, value, Proxy));
                }
            });

            return false;
        }

        protected void AddActionMethod(string method, Func<object[], object> action)
        {
            Values[method] = new ObservableAction<T>(method, action, Proxy);
        }

        public void AddAction(string method, Func<object[], object> action)
        {
            Values[method] = new ObservableAction<T>(method, action);
        }

        public void AddComputedProperty<P>(string property, Func<T, P> compute)
        {
            Func<P> derivation = () => compute(Proxy);

            AddComputedProperty(Target, property, new ComputedValueOptions<P> { Derivation = derivation });
        }

        public void AddComputedPropertyRaw<P>(string property, Func<object, object> compute)
        {
            Func<P> derivation = () => (P)compute(Proxy);

            AddComputedProperty(Target, property, new ComputedValueOptions<P> { Derivation = derivation });
        }

        public void AddComputedProperty<P>(object owner, string property, IComputedValueOptions<P> options)
        {
            var opts = new ComputedValueOptions<P>
            {
                Derivation = options.Derivation,

                Comparer = options.Comparer,

                KeepAlive = options.KeepAlive,

                RequiresReaction = options.RequiresReaction,

                Context = options.Context,

                Name = options.Name ?? $"{Name}.{property}"
            };

            var computed = new ComputedValue<P>(opts);

            Values[property] = computed;
        }

        public void AddObservableProperty<P>(string property, W value, IManipulator manipulator)
        {
            // assertPropertyConfigurable(target, propName)

            if (this.HasInterceptors())
            {
                var change = this.NotifyInterceptors(new ObjectWillChange<object>(property, ChangeType.ADD, value, this));
                if (change == null)
                {
                    return;
                }

                value = (W)change.NewValue;
            }

            var observable = new ObservableValue<W, P>(value, $"{Name}.{property}", manipulator);

            Values[property] = observable;


            // observableValue might have changed it
            value = observable.Value;

            // Object.defineProperty(target, propName, generateObservablePropConfig(propName))
            NotifyPropertyAddition(property, value);
        }

        private void NotifyPropertyAddition(string property, W value)
        {
            if (this.HasListeners())
            {
                this.NotifyListeners(new ObjectDidChange<object>(property, ChangeType.ADD, value, null, Proxy));
            }

            if (PendingKeys != null && PendingKeys.TryGetValue(property, out IObservableValue<bool> entry))
            {
                (entry as IValueWriter<bool>).Value = true;
            }

            KeysAtom.ReportChanged();
        }

        public IDisposable Intercept(Func<IObjectWillChange, IObjectWillChange> interceptor)
        {
            return this.ResigerInterceptor(interceptor);
        }

        public IDisposable Observe(Action<IObjectDidChange> listener, bool force = false)
        {
            if (force)
            {
                throw new InvalidOperationException("`observe` doesn't support the fire immediately property for observable objects.");
            }
            return this.ResigerListener(listener);
        }

        public P Read<P>(string key)
        {
            return (P)Read(key);
        }

        public object Read(string key)
        {
            if (TryRead(key, out object value))
            {
                return value;
            }

            throw new InvalidOperationException($"Not able to read property {key}");
        }

        public object Write<P>(string key, P value)
        {
            return Write(key, (object)value);
        }

        public object Write(string key, object value)
        {
            if (TryWrite(key, value, out object newValue))
            {
                return newValue;
            }

            throw new InvalidOperationException($"Not able to write property {key} with value {value}");
        }

        IDepTreeNode IDepTreeNodeFinder.FindNode(string property)
        {
            var observable = Values[property];

            if (observable is IDepTreeNodeClassifier atom)
            {
                return atom.Node;
            }

            throw new Exception($"Not able to find Atom for property {property}");
        }

        public IList<string> Keys
        {
            get => Values.Keys.Where(key => (Values[key] is IObservableValue)).ToList();
        }
    }

    public class ObservableObject<T> : ObservableObject<T, object>, IObservableObject<T> where T : class
    {
        public ObservableObject(ObservableTypeDef typeDef, IDictionary<string, IObservable> values, Func<IObservableObject<T>, T> proxify, string name, IManipulator manipulator = null, params Type[] otherTypes) : base(typeDef, values, (x) => proxify(x as IObservableObject<T>), name, null, otherTypes)
        {
        }

        public static IObservableObject<T> From(ObservableTypeDef typeDef, Func<IObservableObject<T>, T> proxify, string name, IManipulator manipulator = null, params Type[] otherTypes)
        {
            return new ObservableObject<T>(typeDef, null, proxify, name, manipulator, otherTypes);
        }

        public static T FromAs(ObservableTypeDef typeDef, Func<IObservableObject<T>, T> proxify, string name, IManipulator manipulator = null, params Type[] otherTypes)
        {
            return From(typeDef, proxify, name, manipulator, otherTypes).Proxy;
        }
    }
}
