namespace Skclusive.Mobx.Observable
{
   public delegate void AtomHandler(IAtom atom);

    public interface IAtom : IObservable
    {
        event AtomHandler OnBecomeObservedEvent;

        event AtomHandler OnBecomeUnObservedEvent;

        bool ReportObserved();

        void ReportChanged();
    }
}
