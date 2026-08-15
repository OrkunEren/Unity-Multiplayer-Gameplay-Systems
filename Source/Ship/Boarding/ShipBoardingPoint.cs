using InvadersOverboard.Interaction;
using UnityEngine;

namespace InvadersOverboard.Ship.Boarding
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class ShipBoardingPoint :
        MonoBehaviour,
        IInteractable
    {
        [Header("Identity")]

        [SerializeField]
        private byte pointId;


        [Header("Anchors")]

        [SerializeField]
        private Transform waterAnchor;

        [SerializeField]
        private Transform ledgeAnchor;

        [SerializeField]
        private Transform deckAnchor;


        [Header("Interaction")]

        [SerializeField]
        private string climbPrompt =
            "Climb";

        [SerializeField]
        private int interactionPriority = 10;


        private ShipBoardingPointCollection
            collection;


        public byte PointId =>
            pointId;

        public Transform WaterAnchor =>
            waterAnchor;

        public Transform LedgeAnchor =>
            ledgeAnchor;

        public Transform DeckAnchor =>
            deckAnchor;

        public ShipBoardingPointCollection
            Collection =>
                collection;

        public bool IsConfigured =>
            waterAnchor != null &&
            ledgeAnchor != null &&
            deckAnchor != null &&
            collection != null;


        private void Awake()
        {
            collection =
                GetComponentInParent<
                    ShipBoardingPointCollection>();

            if (!IsConfigured)
            {
                Debug.LogError(
                    $"{name}: Boarding point " +
                    "configuration is incomplete.",
                    this);
            }
        }


        private void Reset()
        {
            BoxCollider boxCollider =
                GetComponent<BoxCollider>();

            boxCollider.isTrigger = true;

            collection =
                GetComponentInParent<
                    ShipBoardingPointCollection>();
        }


        public InteractionOffer
            GetInteractionOffer(
                in InteractionContext context)
        {
            if (!IsConfigured ||
                context.InteractorObject == null)
            {
                return InteractionOffer.Unavailable(
                    string.Empty);
            }

            IShipBoardingRequester requester =
                context.InteractorObject
                    .GetComponent<
                        IShipBoardingRequester>();

            if (requester == null ||
                !requester.CanRequestBoarding(
                    this))
            {
                return InteractionOffer.Unavailable(
                    string.Empty,
                    interactionPriority);
            }

            return InteractionOffer.Instant(
                climbPrompt,
                interactionPriority);
        }


        public bool TryInteract(
            in InteractionContext context)
        {
            if (!IsConfigured ||
                context.InteractorObject == null)
            {
                return false;
            }

            IShipBoardingRequester requester =
                context.InteractorObject
                    .GetComponent<
                        IShipBoardingRequester>();

            if (requester == null ||
                !requester.CanRequestBoarding(
                    this))
            {
                return false;
            }

            return requester.RequestBoarding(
                this);
        }


#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (waterAnchor == null ||
                ledgeAnchor == null ||
                deckAnchor == null)
            {
                return;
            }

            Gizmos.color =
                new Color(
                    0.1f,
                    0.65f,
                    1f,
                    1f);

            Gizmos.DrawLine(
                waterAnchor.position,
                ledgeAnchor.position);

            Gizmos.DrawLine(
                ledgeAnchor.position,
                deckAnchor.position);

            Gizmos.DrawWireSphere(
                waterAnchor.position,
                0.08f);

            Gizmos.DrawWireSphere(
                ledgeAnchor.position,
                0.08f);

            Gizmos.DrawWireSphere(
                deckAnchor.position,
                0.08f);
        }
#endif
    }
}
