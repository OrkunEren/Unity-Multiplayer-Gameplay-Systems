using UnityEngine;

namespace InvadersOverboard.Player.Movement
{
    [DisallowMultipleComponent]
    public sealed class CharacterPlatformTracker : MonoBehaviour
    {
        private Transform trackedPlatform;

        private Vector3 localCharacterPoint;

        private Quaternion previousPlatformRotation =
            Quaternion.identity;

        public Transform TrackedPlatform =>
            trackedPlatform;

        public bool HasPlatform =>
            trackedPlatform != null;

        public CharacterPlatformMotion SampleMotion()
        {
            if (trackedPlatform == null)
                return CharacterPlatformMotion.None;

            Vector3 targetWorldPoint =
                trackedPlatform.TransformPoint(
                    localCharacterPoint);

            Vector3 displacement =
                targetWorldPoint - transform.position;

            float yawDelta =
                CalculateYawDelta(
                    previousPlatformRotation,
                    trackedPlatform.rotation);

            return new CharacterPlatformMotion(
                displacement,
                yawDelta);
        }

        public void ApplyYaw(
            in CharacterPlatformMotion platformMotion)
        {
            if (Mathf.Abs(platformMotion.YawDelta) < 0.001f)
                return;

            transform.Rotate(
                Vector3.up,
                platformMotion.YawDelta,
                Space.World);
        }

        public void Commit(
            in CharacterGroundingInfo grounding,
            bool characterIsGrounded)
        {
            Rigidbody groundRigidbody =
                grounding.GroundRigidbody;

            if (!characterIsGrounded ||
                !grounding.IsGrounded ||
                groundRigidbody == null)
            {
                Clear();
                return;
            }

            Transform nextPlatform =
                groundRigidbody.transform;

            trackedPlatform = nextPlatform;

            localCharacterPoint =
                trackedPlatform.InverseTransformPoint(
                    transform.position);

            previousPlatformRotation =
                trackedPlatform.rotation;
        }

        public void Clear()
        {
            trackedPlatform = null;
            localCharacterPoint = Vector3.zero;

            previousPlatformRotation =
                Quaternion.identity;
        }

        private void OnDisable()
        {
            Clear();
        }

        private static float CalculateYawDelta(
            Quaternion previousRotation,
            Quaternion currentRotation)
        {
            Vector3 previousForward =
                previousRotation * Vector3.forward;

            Vector3 currentForward =
                currentRotation * Vector3.forward;

            previousForward =
                Vector3.ProjectOnPlane(
                    previousForward,
                    Vector3.up);

            currentForward =
                Vector3.ProjectOnPlane(
                    currentForward,
                    Vector3.up);

            if (previousForward.sqrMagnitude < 0.0001f ||
                currentForward.sqrMagnitude < 0.0001f)
            {
                return 0f;
            }

            return Vector3.SignedAngle(
                previousForward,
                currentForward,
                Vector3.up);
        }
    }
}
