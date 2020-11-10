using System;
using System.Collections.Generic;

namespace Skclusive.Mobx.Observable
{
    public class VolatileValue<T> : IVolatileValue<T>
    {
        public VolatileValue(T value)
        {
            _Value = value;
        }

        public static IVolatileValue<T> From()
        {
            return From(default);
        }

        public static IVolatileValue<T> From(T value)
        {
            return new VolatileValue<T>(value);
        }

        private object _Value { get; set; }

        object IValueReader.Value
        {
            get
            {
                return _Value;
            }
        }

        object IValueWriter.Value
        {
            set
            {
                if (value != null && !(value is T))
                throw new Exception($"Value is not of {typeof(T).Name}");

                _Value = value;
            }
        }

        T IValueReader<T>.Value
        {
            get
            {
                return (T)_Value;
            }
        }

        T IValueWriter<T>.Value
        {
            set
            {
                _Value = value;
            }
        }

        public override string ToString()
        {
            return $"{typeof(T).Name}[${_Value}]";
        }
    }
}
