namespace Skclusive.Mobx.Observable
{
    public class Manipulator : IManipulator
    {
        public object Enhance(object newv, object oldV, object name)
        {
            return newv;
        }

        public object Dehance(object value)
        {
            return value;
        }

        public object Enhance(object value)
        {
            return value;
        }
    }

    public class Manipulator<TIn, TOut> : Manipulator, IManipulator<TIn, TOut>
    {
        public TIn Enhance(TIn newv, TIn oldV, string name)
        {
            return Enhance(newv, oldV, (object)name);
        }

        public TIn Enhance(TIn newv, TIn oldV, object name)
        {
            return (TIn)Enhance((object)newv, (object)oldV, (object)name);
        }

        public TIn Enhance(TOut value)
        {
            return (TIn)Enhance((object)value);
        }

        public TOut Dehance(TIn value)
        {
            return (TOut)Dehance((object)value);
        }

        public static IManipulator<TIn, TOut> For()
        {
            return new Manipulator<TIn, TOut>();
        }
    }

    public class Manipulator<T> : Manipulator<T, T>, IManipulator<T>
    {
        public new static IManipulator<T> For()
        {
            return new Manipulator<T>();
        }
    }

    public class Manipulator<TIn, TOut, K> : Manipulator<TIn, TOut>, IManipulator<TIn, TOut, K>
    {
        public TIn Enhance(TIn newv, TIn oldV, K name)
        {
            return Enhance(newv, oldV, (object)name);
        }

        public new static IManipulator<TIn, TOut, K> For()
        {
            return new Manipulator<TIn, TOut, K>();
        }
    }
}
