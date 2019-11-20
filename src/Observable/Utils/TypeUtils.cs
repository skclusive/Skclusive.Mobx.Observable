using System;

namespace Skclusive.Mobx.Observable
{
    public static class TypeUtils
    {
        public static IDepTreeNode GetAtom(object thing, string property = null, bool nothrow = false)
        {
            if (thing is IDepTreeNodeFinder finder && !string.IsNullOrEmpty(property))
            {
                return finder.FindNode(property);
            }

            if (thing is IDepTreeNodeClassifier classifier)
            {
                return classifier.Node;
            }

            if (!nothrow)
            {
                throw new Exception("Not an Atom type classified object");
            }

            return null;
        }
    }
}
