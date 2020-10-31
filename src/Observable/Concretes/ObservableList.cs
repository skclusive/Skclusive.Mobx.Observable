using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Skclusive.Mobx.Observable
{
    public class ObservableList<TIn, TOut> : IObservableList<TIn, TOut>, IDepTreeNodeClassifier
    {
        private IAtom KeysAtom { set; get; }

        IDepTreeNode IDepTreeNodeClassifier.Node => KeysAtom;

        DepTreeNodeType IDepTreeNodeClassifier.AtomType => DepTreeNodeType.List;

        public string Name { private set; get; }

        private List<TIn> Values { set; get; }

        private IManipulator<TIn, TOut> Manipulator { set; get; }

        public int LastKnownLength { private set; get; }

        public IList<Func<IListWillChange<TIn>, IListWillChange<TIn>>> Interceptors { private set; get; } = new List<Func<IListWillChange<TIn>, IListWillChange<TIn>>>();

        public IList<Action<IListDidChange<TIn>>> Listeners { private set; get; } = new List<Action<IListDidChange<TIn>>>();

        public object Meta { get; }

        protected ObservableList(string name, IManipulator<TIn, TOut> manipulator = null, object meta = null)
        {
            Values = new List<TIn>();

            Name = name ?? $"ObservableArray@{States.NextId}";

            Manipulator = manipulator ?? Manipulator<TIn, TOut>.For();

            KeysAtom = new Atom(Name);

            Meta = meta;
        }

        public static IObservableList<TIn, TOut> From(IEnumerable<TOut> values = null, string name = null, IManipulator<TIn, TOut> manipulator = null, object meta = null)
        {
            var list = new ObservableList<TIn, TOut>(name, manipulator, meta);

            if (values != null)
            {
                var previous = Actions.AllowStateChangesStart(true);

                list.SpliceWith(0, 0, values.ToArray());

                Actions.AllowStateChangesEnd(previous);
            }

            return list;
        }

        public static IObservableList<TIn, TOut> FromIn(IEnumerable<TIn> values = null, string name = null, IManipulator<TIn, TOut> manipulator = null, object meta = null)
        {
            var list = new ObservableList<TIn, TOut>(name, manipulator, meta);

            if (values != null)
            {
                var previous = Actions.AllowStateChangesStart(true);

                list.SpliceWithIn(0, 0, values.ToArray());

                Actions.AllowStateChangesEnd(previous);
            }

            return list;
        }

        public TOut this[int key]
        {
            get
            {
                KeysAtom.ReportObserved();

                return Dehance(Values).ToList()[key];
            }

            set => Set(key, value);
        }

        private TOut Dehance(TIn value)
        {
            return Manipulator.Dehance(value);
        }

        private IEnumerable<TOut> Dehance(IEnumerable<TIn> values)
        {
            foreach (var value in values)
            {
                yield return Dehance(value);
            }
        }

        public IDisposable Intercept(Func<IListWillChange<TIn>, IListWillChange<TIn>> interceptor)
        {
            return this.ResigerInterceptor(interceptor);
        }

        public IDisposable Observe(Action<IListDidChange<TIn>> listener, bool force = false)
        {
            if (force)
            {
                var change = ListDidChange<TIn>.Splice
                (
                    this, 0,
                    Values.ToArray(), Values.Count,
                    new TIn[] { }, 0
                );

                listener(change);
            }
            return this.ResigerListener(listener);
        }

        public bool IsReadOnly => false;

        public int Count { get => GetLength(); }

        public int Length { set => SetLength(value); get => GetLength(); }

        public int IndexOf(TOut item)
        {
            KeysAtom.ReportObserved();

            return Dehance(Values).ToList().IndexOf(item);
        }

        public void Insert(int index, TOut value)
        {
            Splice(index, 0, value);
        }

        public int FindIndex(Predicate<TOut> match)
        {
            KeysAtom.ReportObserved();

            return Dehance(Values).ToList().FindIndex(match);
        }

        public void Unshift(TOut value)
        {
            Insert(0, value);
        }

        public TOut Shift()
        {
            return Splice(0, 1)[0];
        }

        public TOut[] Push(params TOut[] values)
        {
            return Splice(Length, 0, values);
        }

        public TOut Pop()
        {
            if (Length < 1)
            {
                throw new InvalidOperationException("No Elements to pop");
            }

            return Splice(Math.Max(Length - 1, 0), 1)[0];
        }

        public void Set(int index, TOut value)
        {
            if (index < Values.Count)
            {
                // update at index in range
                KeysAtom.CheckIfStateModificationsAreAllowed();

                var oldValue = Values[index];

                TIn newValue = Manipulator.Enhance(value);

                if (this.HasInterceptors())
                {
                    var change = this.NotifyInterceptors<IListWillChange<TIn>>(ListWillChange<TIn>.Update(this, index, newValue));
                    if (change == null)
                    {
                        return;
                    }
                    newValue = change.NewValue;
                }

                newValue = Manipulator.Enhance(newValue, oldValue, "[...]");

                if (!EqualityComparer<TIn>.Default.Equals(newValue, oldValue))
                {
                    Values[index] = newValue;

                    NotifyUpdate(index, newValue, oldValue);
                }
            }
            else if (index == Values.Count)
            {
                SpliceWith(index, 0, new TOut[] { value });
            }
            else
            {
                throw new IndexOutOfRangeException($"Index out of bounds, {index} is larger than {Values.Count}");
            }
        }

        private void NotifyUpdate(int index, TIn newValue, TIn oldValue)
        {
            KeysAtom.ReportChanged();

            if (this.HasListeners())
            {
                this.NotifyListeners<IListDidChange<TIn>>(ListDidChange<TIn>.Update(this, index, newValue, oldValue));
            }
        }

        private void NotifySplice(int index, TIn[] added, TIn[] removed)
        {
            KeysAtom.ReportChanged();

            if (this.HasListeners())
            {
                this.NotifyListeners<IListDidChange<TIn>>(ListDidChange<TIn>.Splice(this, index, added, added.Length, removed, removed.Length));
            }
        }

        public void RemoveAt(int index)
        {
            SpliceWith(index, 1);
        }

        public void Add(TOut item)
        {
            SpliceWith(Values.Count, 0, new TOut[] { item });
        }

        public bool Contains(TOut item)
        {
            KeysAtom.ReportObserved();

            return Dehance(Values).Contains(item);
        }

        public void CopyTo(TOut[] array, int index)
        {
            Dehance(Values).ToList().CopyTo(array, index);
        }

        public bool Remove(TOut item)
        {
            var index = IndexOf(item);

            return SpliceWith(index, 1).Length == 1;
        }

        public void Clear()
        {
            (this as IObservableList<TIn, TOut>).Clear();
        }

        TOut[] IObservableList<TIn, TOut>.Clear()
        {
            return SpliceWith(0, null);
        }

        private void UpdateLength(int oldLength, int delta)
        {
            if (oldLength != LastKnownLength)
            {
                throw new InvalidOperationException("Modification exception: the internal structure of an observable array was changed.");
            }
            LastKnownLength += delta;
        }

        protected void SetLength(int newLength)
        {
            if (newLength < 0)
            {
                throw new InvalidOperationException("List Out of range: ");
            }

            var currentLength = Values.Count;

            if (currentLength == newLength)
            {
                return;
            }
            else if (newLength > currentLength)
            {
                var newItems = Enumerable.Range(0, newLength - currentLength).Select(i => default(TOut)).ToArray();

                SpliceWith(currentLength, 0, newItems);
            }
            else
            {
                SpliceWith(newLength, currentLength - newLength);
            }
        }

        protected int GetLength()
        {
            KeysAtom.ReportObserved();

            return Values.Count;
        }

        protected TOut[] SpliceWith(int? argIndex, int? argDeleteCount, params TOut[] newItems)
        {
            return SpliceWithIn(argIndex, argDeleteCount, newItems.Select(newItem => Manipulator.Enhance(newItem)).ToArray());
        }

        protected TOut[] SpliceWithIn(int? argIndex, int? argDeleteCount, params TIn[] newItems)
        {
            KeysAtom.CheckIfStateModificationsAreAllowed();

            var count = Values.Count;

            var index = argIndex.HasValue ? argIndex.Value : 0;

            if (index > count)
            {
                index = count;
            }
            else if (index < 0)
            {
                index = Math.Max(0, count + index);
            }

            var deleteCount = argDeleteCount.HasValue ? argDeleteCount.Value : count - index;

            deleteCount = Math.Max(0, Math.Min(deleteCount, count - index));

            if (this.HasInterceptors())
            {
                var change = this.NotifyInterceptors<IListWillChange<TIn>>(ListWillChange<TIn>.Splice(this, index, newItems, deleteCount));
                if (change == null)
                {
                    return Array.Empty<TOut>();
                }
                deleteCount = change.RemovedCount;
                newItems = change.Added;
            }

            newItems = newItems.Select(newitem => Manipulator.Enhance(newitem, default(TIn), "")).ToArray();

            var lengthDelta = newItems.Length - deleteCount;
            UpdateLength(count, lengthDelta); // checks if internal array wasn't modified

            var spliced = Values.Splice(index, deleteCount, newItems);

            if (deleteCount > 0 || newItems.Length > 0)
            {
                NotifySplice(index, newItems, spliced.ToArray());
            }

            return Dehance(spliced).ToArray();
        }

        public IEnumerator<TOut> GetEnumerator()
        {
            KeysAtom.ReportObserved();

            return Dehance(Values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public TOut[] Splice(int index)
        {
            return SpliceWith(index, null);
        }

        public TOut[] Splice(int index, int deleteCount, params TOut[] newItems)
        {
            return SpliceWith(index, deleteCount, newItems);
        }

        public TOut[] Replace(TOut[] newItems)
        {
            return SpliceWith(0, Count, newItems);
        }

        public override string ToString()
        {
            KeysAtom.ReportObserved();

            return Values.ToString();
        }

        public IEnumerable<TIn> GetValues()
        {
            return Values;
        }

        public TIn Get(int index)
        {
            return Values[index];
        }
    }

    public class ObservableList<T> : ObservableList<T, T>, IObservableList<T>
    {
        protected ObservableList(string name, IManipulator<T, T> manipulator = null) : base(name, manipulator)
        {
        }

        public static IObservableList<T> From(IEnumerable<T> values = null, string name = null, IManipulator<T> manipulator = null)
        {
            var list = new ObservableList<T>(name, manipulator);

            if (values != null)
            {
                var previous = Actions.AllowStateChangesStart(true);

                list.SpliceWith(0, 0, values.ToArray());

                Actions.AllowStateChangesEnd(previous);
            }

            return list;
        }
    }
}
