namespace Skclusive.Mobx.Observable
{
    internal interface IDepTreeNodeClassifier
    {
        DepTreeNodeType AtomType { get; }

        IDepTreeNode Node { get; }
    }
}
