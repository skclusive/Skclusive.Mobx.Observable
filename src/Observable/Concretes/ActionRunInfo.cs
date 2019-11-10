namespace Skclusive.Mobx.Observable
{
    public class ActionRunInfo : IActionRunInfo
    {
        public IDerivation Derivation { set; get; }

        public bool AllowStateChanges { set; get; }

        public long Started { set; get; }

        public bool NotifySpy { set; get; }
    }
}
