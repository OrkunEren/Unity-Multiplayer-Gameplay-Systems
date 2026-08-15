namespace InvadersOverboard.Interaction
{
    public interface IInteractable
    {
        InteractionOffer GetInteractionOffer(
            in InteractionContext context);

        bool TryInteract(
            in InteractionContext context);
    }
}