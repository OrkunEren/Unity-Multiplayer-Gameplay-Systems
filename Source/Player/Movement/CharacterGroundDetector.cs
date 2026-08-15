using UnityEngine;

namespace InvadersOverboard.Player.Movement
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class CharacterGroundDetector : MonoBehaviour
    {
        private static readonly Vector2[] ProbeOffsets =
        {
            Vector2.zero,
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right
        };

        [SerializeField]
        private Transform groundProbe;

        private CharacterController controller;

        public CharacterGroundingInfo CurrentGrounding
        {
            get;
            private set;
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (groundProbe == null)
            {
                Debug.LogError(
                    $"{nameof(CharacterGroundDetector)} requires a GroundProbe.",
                    this);
            }
        }

        public CharacterGroundingInfo Probe(
            CharacterMovementSettings settings)
        {
            return Probe(
                settings,
                Vector3.zero);
        }


        public CharacterGroundingInfo Probe(
            CharacterMovementSettings settings,
            Vector3 predictedDisplacement)
        {
            if (settings == null || groundProbe == null)
            {
                CurrentGrounding =
                    CharacterGroundingInfo.None;

                return CurrentGrounding;
            }

            bool foundValidGround = false;

            RaycastHit bestHit = default;
            float bestDistance =
                float.PositiveInfinity;

            float rayDistance =
                settings.GroundProbeStartHeight
                + settings.GroundProbeDistance;

            for (int i = 0;
                 i < ProbeOffsets.Length;
                 i++)
            {
                Vector2 offset =
                    ProbeOffsets[i];

                Vector3 horizontalOffset =
                    transform.right
                        * offset.x
                        * settings.GroundProbeRadius
                    + transform.forward
                        * offset.y
                        * settings.GroundProbeRadius;

                Vector3 rayOrigin =
                    groundProbe.position
                    + predictedDisplacement
                    + Vector3.up
                        * settings.GroundProbeStartHeight
                    + horizontalOffset;

                bool hasHit =
                    Physics.Raycast(
                        rayOrigin,
                        Vector3.down,
                        out RaycastHit hit,
                        rayDistance,
                        settings.GroundLayers,
                        QueryTriggerInteraction.Ignore);

                if (!hasHit)
                    continue;

                float groundAngle =
                    Vector3.Angle(
                        hit.normal,
                        Vector3.up);

                if (groundAngle >
                    controller.slopeLimit)
                {
                    continue;
                }

                if (hit.distance >= bestDistance)
                    continue;

                foundValidGround = true;
                bestDistance = hit.distance;
                bestHit = hit;
            }

            CurrentGrounding =
                foundValidGround
                    ? new CharacterGroundingInfo(
                        true,
                        bestHit)
                    : CharacterGroundingInfo.None;

            return CurrentGrounding;
        }
    }
}