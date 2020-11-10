namespace Skclusive.Mobx.Observable
{
    public interface IVolatileValue : IValueReader, IValueWriter
    {
    }

    public interface IVolatileValue<T> : IValueReader<T>, IValueWriter<T>, IVolatileValue
    {
    }
}
