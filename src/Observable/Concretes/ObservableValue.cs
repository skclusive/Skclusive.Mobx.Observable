using System;
using System.Collections.Generic;

namespace Skclusive.Mobx.Observable
{
    public class ObservableValue<TIn, TOut> : Atom, IObservableValue<TIn, TOut>
    {
        public ObservableValue(TIn value, string name = null, IManipulator manipulator = null, object meta = null) : base(name ?? $"ObservableValue@{States.NextId}")
        {
            Meta = meta;

            Manipulator = manipulator ?? Manipulator<TIn, TOut>.For();

            _Value = Manipulator.Enhance(value, default(TIn), Name);
        }

        public static IObservableValue<TIn, TOut> From()
        {
            return From(default(TIn));
        }

        public static IObservableValue<TIn, TOut> From(TIn value)
        {
            return From(value, null);
        }

        public static IObservableValue<TIn, TOut> From(TIn value, string name, IManipulator<TIn, TOut> manipulator = null, object meta = null)
        {
            return new ObservableValue<TIn, TOut>(value, name, manipulator, meta);
        }

        private IManipulator Manipulator { set; get; }

        public object Meta { get; }

        public IList<Func<IValueWillChange<TIn>, IValueWillChange<TIn>>> Interceptors { private set; get; } = new List<Func<IValueWillChange<TIn>, IValueWillChange<TIn>>>();

        public IList<Action<IValueDidChange<TIn>>> Listeners { private set; get; } = new List<Action<IValueDidChange<TIn>>>();

        private object _Value { get; set; }

        object IValueReader.Value
        {
            get
            {
                ReportObserved();

                return Manipulator.Dehance((TIn)_Value);
            }
        }

        object IValueWriter.Value
        {
            set
            {
                var oldValue = (TIn)_Value;

                if (PrepareNewValue(oldValue, value, out object newValue))
                {
                    SetNewValue(newValue);
                }
            }
        }

        TOut IValueReader<TOut>.Value
        {
            get
            {
                return (TOut)(this as IValueReader).Value;
            }
        }

        TIn IValueWriter<TIn>.Value
        {
            set
            {
                (this as IValueWriter).Value = value;
            }
        }

        public TIn Value
        {
            get
            {
                ReportObserved();
                return (TIn)_Value;
            }
            set
            {
                (this as IValueWriter).Value = value;
            }
        }

        object IObservableValue.Value
        {
            get
            {
                ReportObserved();
                return _Value;
            }
            set
            {
                (this as IValueWriter).Value = value;
            }
        }

        public void SetNewValue(object newValue)
        {
            SetNewValue((TIn)newValue);
        }

        public void SetNewValue(TIn newValue)
        {
            var oldValue = _Value;

            _Value = newValue;

            ReportChanged();

            if (this.HasListeners())
            {
                this.NotifyListeners<IValueDidChange<TIn>>(new ValueDidChange<TIn>(newValue, (TIn)oldValue, this));
            }
        }

        public bool PrepareNewValue(object oldValue, object changeValue, out object newValue)
        {
            if (PrepareNewValue((TIn)oldValue, (TIn)changeValue, out TIn newTValue))
            {
                newValue = newTValue;
                return true;
            }

            newValue = null;
            return false;
        }

        public bool PrepareNewValue(TIn oldValue, TIn changeValue, out TIn newValue)
        {
            this.CheckIfStateModificationsAreAllowed();

            newValue = changeValue;

            if (this.HasInterceptors<IValueWillChange<TIn>>())
            {
                var change = this.NotifyInterceptors(new ValueWillChange<TIn>(newValue, this));
                if (change == null)
                {
                    return false;
                }
                newValue = change.NewValue;
            }

            newValue = (TIn)Manipulator.Enhance(newValue, oldValue, Name);

            return !EqualityComparer<TIn>.Default.Equals(newValue, oldValue);
        }

        public IDisposable Intercept(Func<IValueWillChange<TIn>, IValueWillChange<TIn>> interceptor)
        {
            return this.ResigerInterceptor<IValueWillChange<TIn>>(interceptor);
        }

        public IDisposable Observe(Action<IValueDidChange<TIn>> listener, bool force = false)
        {
            if (force)
            {
                listener(new ValueDidChange<TIn>((TIn)_Value, default(TIn), this));
            }
            return this.ResigerListener<IValueDidChange<TIn>>(listener);
        }

        public override string ToString()
        {
            return $"{Name}[${Value}]";
        }
    }


    public class ObservableValue<T> : ObservableValue<T, T>, IObservableValue<T>
    {
        public ObservableValue(T value, string name = null, IManipulator<T, T> manipulator = null, object meta = null) : base(value, name, manipulator, meta)
        {
        }

        public new static IObservableValue<T> From()
        {
            return From(default(T));
        }

        public new static IObservableValue<T> From(T value)
        {
            return From(value, null);
        }

        public new static IObservableValue<T> From(T value, string name, IManipulator<T, T> manipulator = null, object meta = null)
        {
            return new ObservableValue<T>(value, name, manipulator, meta);
        }
    }
}
