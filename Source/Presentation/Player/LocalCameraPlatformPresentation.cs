using InvadersOverboard.Player.Movement;
using UnityEngine;

namespace InvadersOverboard.Presentation.Player
{
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterPlatformTracker))]
    [RequireComponent(typeof(CharacterMotor))]
    public sealed class LocalCameraPlatformPresentation :
        MonoBehaviour
    {
        [Header("References")]

        [SerializeField]
        private Transform cameraPlatformRoot;


        [Header("Platform Transition")]

        [Tooltip(
            "Transition duration when attaching to or detaching from the ship.")]
        [SerializeField, Min(0.01f)]
        private float attachBlendDuration = 0.1f;


        [Header("Idle Stabilization")]

        [Tooltip(
            "When the player is idle, local corrections smaller than this " +
            "are not applied to the camera.")]
        [SerializeField, Min(0f)]
        private float idlePositionDeadZone = 0.015f;

        [Tooltip(
            "Smoothing speed for corrections outside the dead zone.")]
        [SerializeField, Min(1f)]
        private float correctionSharpness = 12f;

        [Tooltip(
            "Player movement is followed immediately above this speed.")]
        [SerializeField, Min(0f)]
        private float movementSpeedThreshold = 0.05f;


        private CharacterPlatformTracker platformTracker;

        private CharacterMotor characterMotor;


        private Transform originalParent;

        private Vector3 originalLocalPosition;

        private Quaternion originalLocalRotation;

        private Vector3 originalLocalScale;


        private Transform presentedPlatform;

        private Vector3 presentedLocalPosition;


        // Transition while attaching to the ship
        private bool isBlendingAttachment;

        private float attachBlendElapsed;

        private Vector3 attachStartLocalPosition;

        private Quaternion attachStartLocalRotation;


        // Transition while detaching from the ship
        private bool isBlendingReturn;

        private float returnBlendElapsed;

        private Vector3 returnStartLocalPosition;

        private Quaternion returnStartLocalRotation;


        private void Awake()
        {
            platformTracker =
                GetComponent<CharacterPlatformTracker>();

            characterMotor =
                GetComponent<CharacterMotor>();

            if (cameraPlatformRoot == null)
            {
                Debug.LogError(
                    $"{nameof(LocalCameraPlatformPresentation)} " +
                    "requires a CameraPlatformRoot.",
                    this);

                enabled = false;
                return;
            }

            originalParent =
                cameraPlatformRoot.parent;

            originalLocalPosition =
                cameraPlatformRoot.localPosition;

            originalLocalRotation =
                cameraPlatformRoot.localRotation;

            originalLocalScale =
                cameraPlatformRoot.localScale;
        }


        private void LateUpdate()
        {
            Transform platform =
                platformTracker.TrackedPlatform;

            if (platform == null)
            {
                UpdateReturnToPlayer();
                return;
            }

            Vector3 measuredLocalPosition =
                platform.InverseTransformPoint(
                    transform.position);

            bool platformChanged =
                presentedPlatform != platform
                || cameraPlatformRoot.parent != platform;

            if (platformChanged)
            {
                AttachToPlatform(
                    platform);
            }

            float platformYaw =
                ExtractYaw(
                    platform.rotation);

            float playerYaw =
                ExtractYaw(
                    transform.rotation);

            float localYaw =
                Mathf.DeltaAngle(
                    platformYaw,
                    playerYaw);

            Quaternion targetLocalRotation =
                Quaternion.Euler(
                    0f,
                    localYaw,
                    0f);

            if (isBlendingAttachment)
            {
                UpdateAttachmentBlend(
                    measuredLocalPosition,
                    targetLocalRotation);
            }
            else
            {
                UpdatePresentedLocalPosition(
                    measuredLocalPosition);

                cameraPlatformRoot.localRotation =
                    targetLocalRotation;
            }

            cameraPlatformRoot.localPosition =
                presentedLocalPosition;

            cameraPlatformRoot.localScale =
                originalLocalScale;
        }


        // =====================================================
        // PLATFORM ATTACHMENT
        // =====================================================

        private void AttachToPlatform(
            Transform platform)
        {
            presentedPlatform =
                platform;

            isBlendingReturn = false;

            // Preserve the camera world pose while changing parent.
            cameraPlatformRoot.SetParent(
                platform,
                true);

            attachStartLocalPosition =
                cameraPlatformRoot.localPosition;

            attachStartLocalRotation =
                cameraPlatformRoot.localRotation;

            presentedLocalPosition =
                attachStartLocalPosition;

            attachBlendElapsed = 0f;

            isBlendingAttachment = true;
        }


        private void UpdateAttachmentBlend(
            Vector3 targetLocalPosition,
            Quaternion targetLocalRotation)
        {
            attachBlendElapsed +=
                Time.deltaTime;

            float normalizedTime =
                Mathf.Clamp01(
                    attachBlendElapsed
                    / attachBlendDuration);

            float blend =
                SmoothStep(
                    normalizedTime);

            presentedLocalPosition =
                Vector3.Lerp(
                    attachStartLocalPosition,
                    targetLocalPosition,
                    blend);

            cameraPlatformRoot.localRotation =
                Quaternion.Slerp(
                    attachStartLocalRotation,
                    targetLocalRotation,
                    blend);

            if (normalizedTime >= 1f)
            {
                isBlendingAttachment = false;

                presentedLocalPosition =
                    targetLocalPosition;

                cameraPlatformRoot.localRotation =
                    targetLocalRotation;
            }
        }


        // =====================================================
        // PLATFORM PRESENTATION
        // =====================================================

        private void UpdatePresentedLocalPosition(
            Vector3 measuredLocalPosition)
        {
            Vector3 planarVelocity =
                characterMotor.PlanarVelocity;

            planarVelocity.y = 0f;

            float movementThresholdSquared =
                movementSpeedThreshold
                * movementSpeedThreshold;

            bool characterIsMoving =
                planarVelocity.sqrMagnitude >
                movementThresholdSquared;

            if (characterIsMoving)
            {
                // Do not add camera lag to the player's own movement.
                presentedLocalPosition =
                    measuredLocalPosition;

                return;
            }

            Vector3 correction =
                measuredLocalPosition
                - presentedLocalPosition;

            float deadZoneSquared =
                idlePositionDeadZone
                * idlePositionDeadZone;

            if (correction.sqrMagnitude <=
                deadZoneSquared)
            {
                // Ignore tiny CharacterController collision
                // corrections so they are not propagated to the camera.
                return;
            }

            float blend =
                1f -
                Mathf.Exp(
                    -correctionSharpness
                    * Time.deltaTime);

            presentedLocalPosition =
                Vector3.Lerp(
                    presentedLocalPosition,
                    measuredLocalPosition,
                    blend);
        }


        // =====================================================
        // RETURN TO PLAYER
        // =====================================================

        private void UpdateReturnToPlayer()
        {
            bool requiresReturn =
                presentedPlatform != null
                || cameraPlatformRoot.parent != originalParent;

            if (requiresReturn)
            {
                BeginReturnToPlayer();
            }

            if (!isBlendingReturn)
                return;

            returnBlendElapsed +=
                Time.deltaTime;

            float normalizedTime =
                Mathf.Clamp01(
                    returnBlendElapsed
                    / attachBlendDuration);

            float blend =
                SmoothStep(
                    normalizedTime);

            cameraPlatformRoot.localPosition =
                Vector3.Lerp(
                    returnStartLocalPosition,
                    originalLocalPosition,
                    blend);

            cameraPlatformRoot.localRotation =
                Quaternion.Slerp(
                    returnStartLocalRotation,
                    originalLocalRotation,
                    blend);

            cameraPlatformRoot.localScale =
                originalLocalScale;

            if (normalizedTime >= 1f)
            {
                isBlendingReturn = false;

                cameraPlatformRoot.localPosition =
                    originalLocalPosition;

                cameraPlatformRoot.localRotation =
                    originalLocalRotation;

                cameraPlatformRoot.localScale =
                    originalLocalScale;
            }
        }


        private void BeginReturnToPlayer()
        {
            presentedPlatform = null;

            isBlendingAttachment = false;
            isBlendingReturn = true;

            // Preserve the current world pose while changing parent.
            cameraPlatformRoot.SetParent(
                originalParent,
                true);

            returnStartLocalPosition =
                cameraPlatformRoot.localPosition;

            returnStartLocalRotation =
                cameraPlatformRoot.localRotation;

            returnBlendElapsed = 0f;
        }


        // =====================================================
        // LIFECYCLE
        // =====================================================

        private void OnDisable()
        {
            RestoreImmediatelyToPlayer();
        }


        private void RestoreImmediatelyToPlayer()
        {
            if (cameraPlatformRoot == null)
                return;

            presentedPlatform = null;

            isBlendingAttachment = false;
            isBlendingReturn = false;

            if (cameraPlatformRoot.parent !=
                originalParent)
            {
                cameraPlatformRoot.SetParent(
                    originalParent,
                    false);
            }

            cameraPlatformRoot.localPosition =
                originalLocalPosition;

            cameraPlatformRoot.localRotation =
                originalLocalRotation;

            cameraPlatformRoot.localScale =
                originalLocalScale;
        }


        // =====================================================
        // HELPERS
        // =====================================================

        private static float SmoothStep(
            float normalizedTime)
        {
            return normalizedTime
                   * normalizedTime
                   * (3f - 2f * normalizedTime);
        }


        private static float ExtractYaw(
            Quaternion rotation)
        {
            Vector3 planarForward =
                rotation * Vector3.forward;

            planarForward.y = 0f;

            if (planarForward.sqrMagnitude <
                0.0001f)
            {
                return 0f;
            }

            planarForward.Normalize();

            return Mathf.Atan2(
                       planarForward.x,
                       planarForward.z)
                   * Mathf.Rad2Deg;
        }
    }
}