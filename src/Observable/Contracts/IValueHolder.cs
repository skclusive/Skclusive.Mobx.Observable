namespace Skclusive.Mobx.Observable
{
    public interface IValueReader
    {
        object Value { get; }
    }

    public interface IValueWriter
    {
        object Value { set; }
    }

    public interface IValueReader<T>
    {
        T Value { get; }
    }

    public interface IValueWriter<T>
    {
        T Value { set; }
    }
}
