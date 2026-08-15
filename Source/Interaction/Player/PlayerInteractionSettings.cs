using UnityEngine;

namespace InvadersOverboard.Player.Interaction
{
    [CreateAssetMenu(
        fileName = "SO_PlayerInteraction_Default",
        menuName =
            "Invaders Overboard/Player/Interaction Settings")]
    public sealed class PlayerInteractionSettings :
        ScriptableObject
    {
        [Header("Detection")]

        [SerializeField, Min(0.1f)]
        private float maximumDistance = 3f;

        [SerializeField, Min(0f)]
        private float sphereRadius = 0.08f;

        [SerializeField]
        private LayerMask castMask = ~0;

        [SerializeField]
        private LayerMask interactableMask;

        [SerializeField]
        private QueryTriggerInteraction
            triggerInteraction =
                QueryTriggerInteraction.Collide;


        public float MaximumDistance =>
            maximumDistance;

        public float SphereRadius =>
            sphereRadius;

        public LayerMask CastMask =>
            castMask;

        public QueryTriggerInteraction
            TriggerInteraction =>
            triggerInteraction;


        public bool IsInteractableLayer(
            int layer)
        {
            return
                (interactableMask.value &
                 (1 << layer)) != 0;
        }


        private void OnValidate()
        {
            maximumDistance =
                Mathf.Max(
                    0.1f,
                    maximumDistance);

            sphereRadius =
                Mathf.Max(
                    0f,
                    sphereRadius);
        }
    }
}
