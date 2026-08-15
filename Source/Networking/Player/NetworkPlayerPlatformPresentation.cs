using InvadersOverboard.Player.Movement;
using Unity.Netcode;
using UnityEngine;

namespace InvadersOverboard.Networking.Player
{
    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(CharacterPlatformTracker))]
    public sealed class NetworkPlayerPlatformPresentation :
        NetworkBehaviour
    {
        [Header("Presentation")]

        [SerializeField]
        private Transform visualRoot;

        [SerializeField, Min(1f)]
        private float localPositionSharpness = 20f;

        [SerializeField, Min(1f)]
        private float localRotationSharpness = 20f;


        [Header("Network Thresholds")]

        [SerializeField, Min(0.001f)]
        private float positionPublishThreshold = 0.02f;

        [SerializeField, Min(0.01f)]
        private float rotationPublishThreshold = 0.5f;


        private readonly NetworkVariable<
            NetworkPlayerPlatformState> platformState =
            new(
                NetworkPlayerPlatformState.None,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner);


        private CharacterPlatformTracker platformTracker;

        private Transform originalVisualParent;

        private Vector3 defaultVisualLocalPosition;

        private Quaternion defaultVisualLocalRotation;

        private Vector3 defaultVisualLocalScale;


        private Transform cachedOwnerPlatform;

        private NetworkObject cachedOwnerPlatformObject;


        private NetworkObject renderedPlatform;

        private Vector3 smoothedLocalPosition;

        private float smoothedLocalYaw;


        private void Awake()
        {
            platformTracker =
                GetComponent<CharacterPlatformTracker>();

            if (visualRoot == null)
            {
                Debug.LogError(
                    $"{nameof(NetworkPlayerPlatformPresentation)} " +
                    "requires a VisualRoot.",
                    this);

                return;
            }

            originalVisualParent =
                visualRoot.parent;

            defaultVisualLocalPosition =
                visualRoot.localPosition;

            defaultVisualLocalRotation =
                visualRoot.localRotation;

            defaultVisualLocalScale =
                visualRoot.localScale;
        }


        public override void OnNetworkSpawn()
        {
            renderedPlatform = null;

            cachedOwnerPlatform = null;
            cachedOwnerPlatformObject = null;

            ResetVisualRoot();
        }


        public override void OnGainedOwnership()
        {
            renderedPlatform = null;

            ResetVisualRoot();
        }


        public override void OnLostOwnership()
        {
            renderedPlatform = null;
        }


        public override void OnNetworkDespawn()
        {
            renderedPlatform = null;

            cachedOwnerPlatform = null;
            cachedOwnerPlatformObject = null;

            ResetVisualRoot();
        }


        private void LateUpdate()
        {
            if (!IsSpawned ||
                visualRoot == null)
            {
                return;
            }

            if (IsOwner)
            {
                PublishOwnerPlatformState();
                return;
            }

            RenderRemoteVisual();
        }


        private void PublishOwnerPlatformState()
        {
            Transform platform =
                platformTracker.TrackedPlatform;

            if (platform == null)
            {
                PublishNoPlatform();
                return;
            }

            NetworkObject platformObject =
                ResolvePlatformNetworkObject(
                    platform);

            if (platformObject == null ||
                !platformObject.IsSpawned)
            {
                PublishNoPlatform();
                return;
            }

            Vector3 localPosition =
                platform.InverseTransformPoint(
                    transform.position);

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

            NetworkPlayerPlatformState nextState =
                new(
                    platformObject,
                    localPosition,
                    localYaw);

            if (ShouldPublish(nextState))
            {
                platformState.Value =
                    nextState;
            }
        }


        private void PublishNoPlatform()
        {
            cachedOwnerPlatform = null;
            cachedOwnerPlatformObject = null;

            if (!platformState.Value.HasPlatform)
                return;

            platformState.Value =
                NetworkPlayerPlatformState.None;
        }


        private NetworkObject ResolvePlatformNetworkObject(
            Transform platform)
        {
            if (cachedOwnerPlatform == platform)
            {
                return cachedOwnerPlatformObject;
            }

            cachedOwnerPlatform = platform;

            cachedOwnerPlatformObject =
                platform.GetComponentInParent<
                    NetworkObject>();

            return cachedOwnerPlatformObject;
        }


        private bool ShouldPublish(
            in NetworkPlayerPlatformState nextState)
        {
            NetworkPlayerPlatformState currentState =
                platformState.Value;

            if (!currentState.HasPlatform)
                return true;

            if (!currentState.Platform.Equals(
                    nextState.Platform))
            {
                return true;
            }

            float positionThresholdSquared =
                positionPublishThreshold
                * positionPublishThreshold;

            Vector3 positionDelta =
                currentState.LocalPosition
                - nextState.LocalPosition;

            if (positionDelta.sqrMagnitude >
                positionThresholdSquared)
            {
                return true;
            }

            float rotationDelta =
                Mathf.Abs(
                    Mathf.DeltaAngle(
                        currentState.LocalYaw,
                        nextState.LocalYaw));

            return rotationDelta >
                   rotationPublishThreshold;
        }


        private void RenderRemoteVisual()
        {
            NetworkPlayerPlatformState state =
                platformState.Value;

            if (!state.HasPlatform ||
                !state.Platform.TryGet(
                    out NetworkObject platformObject) ||
                platformObject == null ||
                !platformObject.IsSpawned)
            {
                ReturnVisualToPlayerRoot();
                return;
            }

            bool platformChanged =
                renderedPlatform != platformObject;

            renderedPlatform = platformObject;

            if (platformChanged)
            {
                smoothedLocalPosition =
                    state.LocalPosition;

                smoothedLocalYaw =
                    state.LocalYaw;
            }
            else
            {
                float positionBlend =
                    CalculateBlend(
                        localPositionSharpness);

                float rotationBlend =
                    CalculateBlend(
                        localRotationSharpness);

                smoothedLocalPosition =
                    Vector3.Lerp(
                        smoothedLocalPosition,
                        state.LocalPosition,
                        positionBlend);

                smoothedLocalYaw =
                    Mathf.LerpAngle(
                        smoothedLocalYaw,
                        state.LocalYaw,
                        rotationBlend);
            }

            AttachVisualToPlatform(
                platformObject.transform);
        }


        private void AttachVisualToPlatform(
            Transform platform)
        {
            // VisualRoot stays under its normal parent in the player prefab.
            if (originalVisualParent != null &&
                visualRoot.parent != originalVisualParent)
            {
                visualRoot.SetParent(
                    originalVisualParent,
                    true);
            }

            // The player's foot position follows the platform's full transform.
            Vector3 playerWorldPosition =
                platform.TransformPoint(
                    smoothedLocalPosition);

            // The character should not inherit the platform's pitch or roll.
            // Only the platform's world-space yaw contributes to presentation.
            float platformYaw =
                ExtractYaw(
                    platform.rotation);

            float playerWorldYaw =
                platformYaw +
                smoothedLocalYaw;

            Quaternion playerWorldRotation =
                Quaternion.Euler(
                    0f,
                    playerWorldYaw,
                    0f);

            Vector3 targetWorldPosition =
                playerWorldPosition
                + playerWorldRotation
                * defaultVisualLocalPosition;

            Quaternion targetWorldRotation =
                playerWorldRotation
                * defaultVisualLocalRotation;

            visualRoot.SetPositionAndRotation(
                targetWorldPosition,
                targetWorldRotation);

            visualRoot.localScale =
                defaultVisualLocalScale;
        }


        private void ReturnVisualToPlayerRoot()
        {
            renderedPlatform = null;

            if (originalVisualParent == null)
                return;

            if (visualRoot.parent !=
                originalVisualParent)
            {
                visualRoot.SetParent(
                    originalVisualParent,
                    true);
            }

            float positionBlend =
                CalculateBlend(
                    localPositionSharpness);

            float rotationBlend =
                CalculateBlend(
                    localRotationSharpness);

            visualRoot.localPosition =
                Vector3.Lerp(
                    visualRoot.localPosition,
                    defaultVisualLocalPosition,
                    positionBlend);

            visualRoot.localRotation =
                Quaternion.Slerp(
                    visualRoot.localRotation,
                    defaultVisualLocalRotation,
                    rotationBlend);

            visualRoot.localScale =
                Vector3.Lerp(
                    visualRoot.localScale,
                    defaultVisualLocalScale,
                    positionBlend);
        }


        private void ResetVisualRoot()
        {
            if (visualRoot == null)
                return;

            if (originalVisualParent != null &&
                visualRoot.parent != originalVisualParent)
            {
                visualRoot.SetParent(
                    originalVisualParent,
                    false);
            }

            visualRoot.localPosition =
                defaultVisualLocalPosition;

            visualRoot.localRotation =
                defaultVisualLocalRotation;

            visualRoot.localScale =
                defaultVisualLocalScale;
        }


        private float CalculateBlend(
            float sharpness)
        {
            return 1f -
                   Mathf.Exp(
                       -sharpness
                       * Time.deltaTime);
        }


        private static float ExtractYaw(
            Quaternion rotation)
        {
            Vector3 planarForward =
                rotation
                * Vector3.forward;

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