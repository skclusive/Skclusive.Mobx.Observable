namespace Skclusive.Mobx.Observable
{
    public interface IObservableAction
    {
        object Execute(object[] arguments);
    }
}
