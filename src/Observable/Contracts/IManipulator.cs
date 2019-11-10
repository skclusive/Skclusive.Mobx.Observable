namespace Skclusive.Mobx.Observable
{
    public interface IManipulator
    {
        object Enhance(object value);

        object Enhance(object newv, object oldV, object name);

        object Dehance(object value);
    }

    public interface IManipulator<TIn, TOut> : IManipulator
    {
        TIn Enhance(TOut value);

        TIn Enhance(TIn newv, TIn oldV, object name);

        TOut Dehance(TIn value);
    }

    public interface IManipulator<T> : IManipulator<T, T>
    {
    }

    public interface IManipulator<TIn, TOut, K> : IManipulator<TIn, TOut>
    {
        TIn Enhance(TIn newv, TIn oldV, K name);
    }
}
