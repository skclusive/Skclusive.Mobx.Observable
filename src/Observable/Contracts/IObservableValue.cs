namespace Skclusive.Mobx.Observable
{
    public interface IObservableValue : IValueReader, IValueWriter, IObservableMeta
    {
        string Name { get; }

        new object Value { set; get; }

        void SetNewValue(object newValue);

        bool PrepareNewValue(object oldValue, object changeValue, out object newValue);
    }

    public interface IObservableValue<TIn, TOut> : IValueReader<TOut>, IValueWriter<TIn>, IObservableValue, IInterceptable<IValueWillChange<TIn>>, IListenable<IValueDidChange<TIn>>
    {
        new TIn Value { set; get; }

        void SetNewValue(TIn newValue);

        bool PrepareNewValue(TIn oldValue, TIn changeValue, out TIn newValue);
    }

    public interface IObservableValue<T> : IObservableValue<T, T>
    {
    }
}
