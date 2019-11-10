namespace Skclusive.Mobx.Observable
{
    public class ReactionDisposable : IReactionDisposable
    {
        public ReactionDisposable(Reaction reaction)
        {
            Reaction = reaction;
        }
        public Reaction Reaction { private set; get; }

        public void Dispose()
        {
            Reaction.Dispose();
        }
    }
}
