namespace InvadersOverboard.Interaction
{
    public interface IInteractionFocusReceiver
    {
        void OnInteractionFocusEntered(
            in InteractionContext context);

        void OnInteractionFocusExited(
            in InteractionContext context);
    }
}