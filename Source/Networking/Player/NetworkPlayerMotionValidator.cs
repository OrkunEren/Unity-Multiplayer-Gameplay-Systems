using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace InvadersOverboard.Networking.Player
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    public sealed class NetworkPlayerMotionValidator : NetworkBehaviour
    {
        [Header("Position Validation")]

        [SerializeField, Min(0.1f)]
        private float maximumHorizontalSpeed = 18f;

        [SerializeField, Min(0.1f)]
        private float maximumVerticalSpeed = 30f;

        [SerializeField, Min(0f)]
        private float positionTolerance = 0.5f;

        [Header("Rotation Validation")]

        [SerializeField, Min(1f)]
        private float maximumYawSpeed = 1440f;

        [SerializeField, Min(0f)]
        private float rotationTolerance = 15f;

        [Header("Timing")]

        [SerializeField, Min(0.05f)]
        private float maximumValidationInterval = 0.5f;

        private NetworkTransform networkTransform;

        private Vector3 lastAcceptedPosition;
        private Quaternion lastAcceptedRotation;
        private Vector3 acceptedScale;

        private double lastServerTime;

        private void Awake()
        {
            networkTransform = GetComponent<NetworkTransform>();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
                return;

            ResetValidationState();

            networkTransform.OnClientRequestChange =
                ValidateClientTransform;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer || networkTransform == null)
                return;

            networkTransform.OnClientRequestChange = null;
        }

        public void ResetValidationState()
        {
            lastAcceptedPosition = transform.position;
            lastAcceptedRotation = transform.rotation;
            acceptedScale = transform.localScale;

            if (NetworkManager != null)
                lastServerTime = NetworkManager.ServerTime.Time;
        }

        private (
            Vector3 pos,
            Quaternion rotOut,
            Vector3 scale
            ) ValidateClientTransform(
                Vector3 requestedPosition,
                Quaternion requestedRotation,
                Vector3 requestedScale)
        {
            float elapsedTime = GetValidationDeltaTime();

            Vector3 validatedPosition = ValidatePosition(
                requestedPosition,
                elapsedTime);

            Quaternion validatedRotation = ValidateRotation(
                requestedRotation,
                elapsedTime);

            lastAcceptedPosition = validatedPosition;
            lastAcceptedRotation = validatedRotation;

            // The client is not allowed to change the player scale.
            return (
                validatedPosition,
                validatedRotation,
                acceptedScale);
        }

        private Vector3 ValidatePosition(
            Vector3 requestedPosition,
            float elapsedTime)
        {
            Vector3 delta =
                requestedPosition - lastAcceptedPosition;

            Vector3 horizontalDelta =
                new Vector3(delta.x, 0f, delta.z);

            float maximumHorizontalDistance =
                maximumHorizontalSpeed * elapsedTime +
                positionTolerance;

            if (horizontalDelta.sqrMagnitude >
                maximumHorizontalDistance *
                maximumHorizontalDistance)
            {
                horizontalDelta = horizontalDelta.normalized *
                                  maximumHorizontalDistance;
            }

            float maximumVerticalDistance =
                maximumVerticalSpeed * elapsedTime +
                positionTolerance;

            float verticalDelta = Mathf.Clamp(
                delta.y,
                -maximumVerticalDistance,
                maximumVerticalDistance);

            return lastAcceptedPosition +
                   horizontalDelta +
                   Vector3.up * verticalDelta;
        }

        private Quaternion ValidateRotation(
            Quaternion requestedRotation,
            float elapsedTime)
        {
            float maximumRotation =
                maximumYawSpeed * elapsedTime +
                rotationTolerance;

            return Quaternion.RotateTowards(
                lastAcceptedRotation,
                requestedRotation,
                maximumRotation);
        }

        private float GetValidationDeltaTime()
        {
            double currentServerTime =
                NetworkManager.ServerTime.Time;

            float minimumInterval =
                1f / Mathf.Max(
                    1,
                    NetworkManager.NetworkConfig.TickRate);

            float elapsedTime = Mathf.Clamp(
                (float)(currentServerTime - lastServerTime),
                minimumInterval,
                maximumValidationInterval);

            lastServerTime = currentServerTime;

            return elapsedTime;
        }
    }
}