using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Skclusive.Core.Collection;

namespace Skclusive.Mobx.Observable
{
    public class ObservableMap<TKey, TIn, TOut> : IObservableMap<TKey, TIn, TOut>, IDepTreeNodeClassifier, IDepTreeNodeFinder
    {
        private IAtom KeysAtom { set; get; }

        IDepTreeNode IDepTreeNodeClassifier.Node => KeysAtom;

        DepTreeNodeType IDepTreeNodeClassifier.AtomType => DepTreeNodeType.Map;

        private IMap<TKey, IObservableValue<TIn>> Data { set; get; }

        private IMap<TKey, IObservableValue<bool>> HasMap { set; get; }

        public string Name { private set; get; }

        private IManipulator<TIn, TOut, TKey> Manipulator { set; get; }

        public IList<Func<IMapWillChange<TKey, TIn>, IMapWillChange<TKey, TIn>>> Interceptors { private set; get; } = new List<Func<IMapWillChange<TKey, TIn>, IMapWillChange<TKey, TIn>>>();

        public IList<Action<IMapDidChange<TKey, TIn>>> Listeners { private set; get; } = new List<Action<IMapDidChange<TKey, TIn>>>();

        public object Meta { get; }

        protected ObservableMap(string name, IManipulator<TIn, TOut, TKey> manipulator = null, object meta = null)
        {
            Data = new Map<TKey, IObservableValue<TIn>>();

            HasMap = new Map<TKey, IObservableValue<bool>>();

            Name = name ?? $"ObservableMap@{States.NextId}";

            Manipulator = manipulator ?? Manipulator<TIn, TOut, TKey>.For();

            KeysAtom = new Atom($"{Name}.keys()");

            Meta = meta;
        }

        public static IObservableMap<TKey, TIn, TOut> From(IMap<TKey, TOut> values = null, string name = null, object meta = null)
        {
            return From(values, name, null, meta);
        }

        public static IObservableMap<TKey, TIn, TOut> From(IMap<TKey, TOut> values = null, string name = null, IManipulator<TIn, TOut, TKey> manipulator = null, object meta = null)
        {
            var observableMap = new ObservableMap<TKey, TIn, TOut>(name, manipulator, meta);

            if (values != null)
            {
                observableMap.Merge(values);
            }

            return observableMap;
        }

        public static IObservableMap<TKey, TIn, TOut> FromIn(IMap<TKey, TIn> values = null, string name = null, IManipulator<TIn, TOut, TKey> manipulator = null, object meta = null)
        {
            var observableMap = new ObservableMap<TKey, TIn, TOut>(name, manipulator, meta);

            if (values != null)
            {
                observableMap.MergeIn(values);
            }

            return observableMap;
        }

        IDepTreeNode IDepTreeNodeFinder.FindNode(string property)
        {
            TKey key = (TKey)(property as object);

            object observable = Data.ContainsKey(key) ?
                Data[key] : null;

            if ( observable is null)
            {
                observable = HasMap.ContainsKey(key) ? HasMap[key] : null;
            }

            if (observable is IDepTreeNodeClassifier atom)
            {
                return atom.Node;
            }

            throw new Exception($"Not able to find Atom for property {property}");
        }

        public TOut Get(TKey key)
        {
            return Manipulator.Dehance(Has(key) ? (Data[key] as IValueReader<TIn>).Value : default(TIn));
        }

        public ICollection<TKey> Keys
        {
            get
            {
                KeysAtom.ReportObserved();

                return Data.Keys;
            }
        }

        public ICollection<TOut> Values
        {
            get
            {
                return Keys.Select(key => Get(key)).ToList();
            }
        }

        public bool IsReadOnly => false;

        public TOut this[TKey key]
        {
            get => Get(key);

            set => Set(key, value);
        }

        private bool UpdateValue(TKey key, TIn value, out TIn newValue)
        {
            var observable = Data[key];

            var oldValue = (observable as IValueReader<TIn>).Value;

            if (observable.PrepareNewValue(oldValue, value, out TIn changedValue))
            {
                observable.SetNewValue(changedValue);

                newValue = changedValue;

                if (this.HasListeners())
                {
                    this.NotifyListeners(new MapDidChange<TKey, TIn>(key, ChangeType.UPDATE, newValue, oldValue, this));
                }

                return true;
            }

            newValue = value;

            return false;
        }

        private bool AddValue(TKey key, TIn value, out TIn newValue)
        {
            KeysAtom.CheckIfStateModificationsAreAllowed();

            TIn changed = default(TIn);

            Reactions.Transaction(() =>
            {
                var observable = new ObservableValue<TIn>(value, $"${Name}.{key}");

                Data[key] = observable;

                changed = observable.Value;

                UpdateHasMapEntry(key, true);

                KeysAtom.ReportChanged();
            });

            newValue = changed;

            if (this.HasListeners())
            {
                this.NotifyListeners(new MapDidChange<TKey, TIn>(key, ChangeType.ADD, newValue, default(TIn), this));
            }

            return true;
        }

        private bool _Has(TKey key) => Data.ContainsKey(key);

        public bool Has(TKey key)
        {
            if (HasMap.ContainsKey(key))
            {
                return (HasMap[key] as IValueReader<bool>).Value;
            }

            return (UpdateHasMapEntry(key, false) as IValueReader<bool>).Value;
        }

        public ObservableMap<TKey, TIn, TOut> Set(TKey key, TOut value)
        {
            return Set(key, Manipulator.Enhance(value));
        }

        public ObservableMap<TKey, TIn, TOut> Set(TKey key, TIn value)
        {
            var hasKey = _Has(key);

            if (this.HasInterceptors())
            {
                var change = this.NotifyInterceptors(new MapWillChange<TKey, TIn>(key, hasKey ? ChangeType.UPDATE : ChangeType.ADD, value, this));
                if (change == null)
                {
                    return this;
                }
                value = change.NewValue;
            }

            if (hasKey)
            {
                UpdateValue(key, value, out TIn outValue);
            }
            else
            {
                AddValue(key, value, out TIn outValue);
            }

            return this;
        }

        private IObservableValue<bool> UpdateHasMapEntry(TKey key, bool value)
        {
            // optimization; don't fill the hasMap if we are not observing, or remove entry if there are no observers anymore
            var entry = HasMap.ContainsKey(key) ? HasMap[key] : null;
            if (entry != null)
            {
                entry.SetNewValue(value);
            }
            else
            {
                entry = new ObservableValue<bool>(value, $"{Name}.{key}?");
                HasMap[key] = entry;
            }

            return entry;
        }

        public bool Remove(TKey key)
        {
            if (this.HasInterceptors())
            {
                var change = this.NotifyInterceptors<IMapWillChange<TKey, TIn>>(new MapWillChange<TKey, TIn>(key, ChangeType.REMOVE, default(TIn), this));
                if (change == null)
                {
                    return false;
                }
            }

            if (_Has(key))
            {
                var observable = Data[key];
                var oldValue = (observable as IValueReader<TIn>).Value;

                Reactions.Transaction(() =>
                {
                    KeysAtom.ReportChanged();

                    UpdateHasMapEntry(key, false);

                    observable.SetNewValue(default(TIn));

                    Data.Remove(key);
                });

                if (this.HasListeners())
                {
                    this.NotifyListeners(new MapDidChange<TKey, TIn>(key, ChangeType.REMOVE, default(TIn), oldValue, this));
                }

                return true;
            }

            return false;
        }

        protected IObservableMap<TKey, TIn, TOut> MergeIn(IDictionary<TKey, TIn> values)
        {
            Reactions.Transaction(() =>
            {
                foreach (var item in values)
                {
                    Set(item.Key, item.Value);
                }
            });

            return this;
        }

        public IObservableMap<TKey, TIn, TOut> Merge(IDictionary<TKey, TOut> values)
        {
            Reactions.Transaction(() =>
            {
                foreach (var item in values)
                {
                    Set(item.Key, item.Value);
                }
            });

            return this;
        }

        public IDisposable Intercept(Func<IMapWillChange<TKey, TIn>, IMapWillChange<TKey, TIn>> interceptor)
        {
            return this.ResigerInterceptor(interceptor);
        }

        public IDisposable Observe(Action<IMapDidChange<TKey, TIn>> listener, bool force = false)
        {
            if (force)
            {
                throw new InvalidOperationException("`observe` doesn't support fireImmediately=true in combination with maps.");
            }
            return this.ResigerListener(listener);
        }

        public void Add(TKey key, TOut value)
        {
            Set(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return _Has(key);
        }

        public bool TryGetValue(TKey key, out TOut value)
        {
            if (Data.TryGetValue(key, out IObservableValue<TIn> observable))
            {
                value = Manipulator.Dehance((observable as IValueReader<TIn>).Value);

                return true;
            }

            value = default(TOut);

            return false;
        }

        public IEnumerable<TIn> GetValues()
        {
            KeysAtom.ReportObserved();

            return Data.Values.Select(value => (value as IValueReader<TIn>).Value);
        }

        public TIn GetValue(TKey key)
        {
            KeysAtom.ReportObserved();

            var value = Data[key];

            return (value as IValueReader<TIn>).Value;
        }

        public IEnumerable<KeyValuePair<TKey, TIn>> GetPairs()
        {
            KeysAtom.ReportObserved();

            foreach (var entry in Data)
            {
                yield return new KeyValuePair<TKey, TIn>(entry.Key, (entry.Value as IValueReader<TIn>).Value);
            }
        }

        public void Add(KeyValuePair<TKey, TOut> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            Reactions.Transaction(() =>
            {
                States.Untracked<object>(() =>
                {
                    foreach (var key in Keys.ToList())
                    {
                        Remove(key);
                    }
                    return null;
                });
            });
        }

        public int Count
        {
            get => Keys.Count;
        }

        public bool Contains(KeyValuePair<TKey, TOut> item)
        {
            if (_Has(item.Key))
            {
                var value = Data[item.Key];

                return EqualityComparer<TOut>.Default.Equals(Manipulator.Dehance((value as IValueReader<TIn>).Value), item.Value);
            }

            return false;
        }

        public void CopyTo(KeyValuePair<TKey, TOut>[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", "Must be greater than or equal to zero.");
            }

            if (index + Data.Count > array.Length)
            {
                throw new ArgumentException("array", "Array is too small");
            }

            foreach (var pair in this)
            {
                array[index] = pair;
                index++;
            }
        }

        public bool Remove(KeyValuePair<TKey, TOut> item)
        {
            if (Contains(item))
            {
                Data.Remove(item.Key);

                return false;
            }

            return false;
        }

        public IEnumerator<KeyValuePair<TKey, TOut>> GetEnumerator()
        {
            foreach (var entry in GetPairs())
            {
                yield return new KeyValuePair<TKey, TOut>(entry.Key, Manipulator.Dehance(entry.Value));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IObservableMap<TKey, TIn, TOut> Replace(IDictionary<TKey, TOut> dictionary)
        {
            Reactions.Transaction(() =>
            {
                var newKeys = dictionary.Keys;

                var oldKeys = Keys.ToList();

                var missingKeys = oldKeys.Where(key => !newKeys.Contains(key));

                foreach (var key in missingKeys)
                {
                    Remove(key);
                }

                Merge(dictionary);
            });

            return this;
        }

        public override string ToString()
        {
            var pairs = string.Join(", ", this.Select(pair => $"{pair.Key}: {pair.Value}"));

            return $"{Name}[{{{pairs}}}]";
        }

        IMap<TKey, TOut> IMap<TKey, TOut>.Replace(IDictionary<TKey, TOut> dictionary)
        {
            return Replace(dictionary);
        }
    }

    public class ObservableMap<TKey, TIn> : ObservableMap<TKey, TIn, TIn>, IObservableMap<TKey, TIn>
    {
        protected ObservableMap(string name, IManipulator<TIn, TIn, TKey> manipulator = null, object meta = null) : base(name, manipulator, meta)
        {
        }

        public static new IObservableMap<TKey, TIn> From(IMap<TKey, TIn> values = null, string name = null, object meta = null)
        {
            var observableMap = new ObservableMap<TKey, TIn>(name, null, meta);

            if (values != null)
            {
                observableMap.Merge(values);
            }

            return observableMap;
        }
    }
}
