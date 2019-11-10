namespace Skclusive.Mobx.Observable
{
    public interface IActionRunInfo
    {
        IDerivation Derivation { get; }

        bool AllowStateChanges { get; }

        long Started { get; }

        bool NotifySpy { get; }
    }
}
