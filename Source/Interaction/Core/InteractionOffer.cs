using UnityEngine;

namespace InvadersOverboard.Interaction
{
    public readonly struct InteractionOffer
    {
        public bool CanInteract
        {
            get;
        }

        public string Prompt
        {
            get;
        }

        public InteractionMode Mode
        {
            get;
        }

        public float HoldDuration
        {
            get;
        }

        public int Priority
        {
            get;
        }


        public InteractionOffer(
            bool canInteract,
            string prompt,
            InteractionMode mode =
                InteractionMode.Instant,
            float holdDuration = 0f,
            int priority = 0)
        {
            CanInteract =
                canInteract;

            Prompt =
                prompt ?? string.Empty;

            Mode =
                mode;

            HoldDuration =
                mode == InteractionMode.Hold
                    ? Mathf.Max(
                        0.05f,
                        holdDuration)
                    : 0f;

            Priority =
                priority;
        }


        public static InteractionOffer Instant(
            string prompt,
            int priority = 0)
        {
            return new InteractionOffer(
                true,
                prompt,
                InteractionMode.Instant,
                0f,
                priority);
        }


        public static InteractionOffer Hold(
            string prompt,
            float duration,
            int priority = 0)
        {
            return new InteractionOffer(
                true,
                prompt,
                InteractionMode.Hold,
                duration,
                priority);
        }


        public static InteractionOffer Unavailable(
            string prompt,
            int priority = 0)
        {
            return new InteractionOffer(
                false,
                prompt,
                InteractionMode.Instant,
                0f,
                priority);
        }
    }
}