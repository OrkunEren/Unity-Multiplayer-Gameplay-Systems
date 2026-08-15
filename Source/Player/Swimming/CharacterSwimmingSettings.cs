using UnityEngine;

namespace InvadersOverboard.Player.Swimming
{
    [CreateAssetMenu(
        fileName = "SO_PlayerSwimming",
        menuName =
            "Invaders Overboard/Player/Swimming Settings")]
    public sealed class CharacterSwimmingSettings :
        ScriptableObject
    {
        [Header("Water Detection")]

        [SerializeField, Min(0f)]
        private float enterDepth = 1f;

        [SerializeField, Min(0f)]
        private float exitDepth = 0.65f;


        [Header("Movement")]

        [SerializeField, Min(0f)]
        private float swimSpeed = 3.5f;

        [SerializeField, Min(0f)]
        private float acceleration = 8f;

        [SerializeField, Min(0f)]
        private float deceleration = 10f;


        [Header("Surface Following")]

        [SerializeField, Min(0f)]
        private float targetSubmersionDepth = 1.3f;

        [SerializeField, Min(0f)]
        private float surfaceFollowSharpness = 3f;

        [SerializeField, Min(0f)]
        private float verticalAcceleration = 6f;

        [SerializeField, Min(0f)]
        private float maximumVerticalSpeed = 3.5f;


        [Header("Diving")]

        [Tooltip(
            "While swimming forward, looking down beyond this angle " +
            "starts a dive.")]
        [SerializeField, Range(0f, 85f)]
        private float divePitchThreshold = 20f;

        [Tooltip(
            "Minimum forward input required to start diving.")]
        [SerializeField, Range(0f, 1f)]
        private float diveForwardInputThreshold = 0.25f;

        [Tooltip(
            "When the player rises to this depth and is not swimming " +
            "downward, return to surface mode.")]
        [SerializeField, Min(0f)]
        private float surfaceReturnDepth = 1.5f;
        
        [Tooltip(
            "Target upward swim speed while Space is held.")]
        [SerializeField, Min(0f)]
        private float ascentSpeed = 3f;

        
        public float AscentSpeed =>
            ascentSpeed;

        public float EnterDepth => enterDepth;
        public float ExitDepth => exitDepth;

        public float SwimSpeed => swimSpeed;
        public float Acceleration => acceleration;
        public float Deceleration => deceleration;

        public float TargetSubmersionDepth =>
            targetSubmersionDepth;

        public float SurfaceFollowSharpness =>
            surfaceFollowSharpness;

        public float VerticalAcceleration =>
            verticalAcceleration;

        public float MaximumVerticalSpeed =>
            maximumVerticalSpeed;

        public float DivePitchThreshold =>
            divePitchThreshold;

        public float DiveForwardInputThreshold =>
            diveForwardInputThreshold;

        public float SurfaceReturnDepth =>
            surfaceReturnDepth;


        private void OnValidate()
        {
            exitDepth =
                Mathf.Min(
                    exitDepth,
                    enterDepth);

            surfaceReturnDepth =
                Mathf.Max(
                    surfaceReturnDepth,
                    targetSubmersionDepth);
            
            ascentSpeed =
                Mathf.Min(
                    ascentSpeed,
                    swimSpeed);
        }
    }
}