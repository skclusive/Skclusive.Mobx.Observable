namespace Skclusive.Mobx.Observable
{
    public enum DerivationState
    {
        // before being run or (outside batch and not being observed)
        // at this point derivation is not holding any data about dependency tree
        NOT_TRACKING = -1,
        // no shallow dependency changed since last computation
        // won't recalculate derivation
        // this is what makes mobx fast
        UP_TO_DATE = 0,
        // some deep dependency changed, but don't know if shallow dependency changed
        // will require to check first if UP_TO_DATE or POSSIBLY_STALE
        // currently only ComputedValue will propagate POSSIBLY_STALE
        //
        // having this state is second big optimization:
        // don't have to recompute on every dependency change, but only when it's needed
        POSSIBLY_STALE = 1,
        // A shallow dependency has changed since last computation and the derivation
        // will need to recompute when it's needed next.
        STALE = 2
    }
}
