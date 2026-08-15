using UnityEngine;

namespace InvadersOverboard.Player.Movement
{
    [CreateAssetMenu(
        fileName = "SO_PlayerMovement",
        menuName =
            "Invaders Overboard/Player/Movement Settings")]
    public sealed class CharacterMovementSettings :
        ScriptableObject
    {
        [Header("Planar Movement")]

        [SerializeField, Min(0f)]
        private float moveSpeed = 5f;
        
        [SerializeField, Min(0f)]
        private float sprintSpeed = 7.5f;

        [SerializeField, Min(0f)]
        private float acceleration = 20f;

        [SerializeField, Min(0f)]
        private float deceleration = 25f;
        

        [SerializeField, Range(0f, 1f)]
        private float sprintForwardThreshold = 0.5f;


        [Header("Vertical Movement")]

        [SerializeField, Min(0f)]
        private float gravity = 25f;

        [SerializeField, Min(0f)]
        private float maxFallSpeed = 35f;

        [SerializeField, Min(0f)]
        private float groundedStickSpeed = 2f;

        [SerializeField, Min(0f)]
        private float jumpHeight = 1.2f;


        [Header("External Motion")]

        [SerializeField, Min(0f)]
        private float externalVelocityDecay = 12f;
        
        [Header("Ground Detection")]
        [SerializeField, Min(0.01f)]
        private float groundProbeRadius = 0.25f;

        [SerializeField, Min(0.01f)]
        private float groundProbeStartHeight = 0.25f;

        [SerializeField, Min(0.01f)]
        private float groundProbeDistance = 0.18f;

        [SerializeField]
        private LayerMask groundLayers = ~0;


        public float MoveSpeed =>
            moveSpeed;
        
        public float SprintSpeed =>
            sprintSpeed;

        public float Acceleration =>
            acceleration;

        public float Deceleration =>
            deceleration;

        public float Gravity =>
            gravity;

        public float MaxFallSpeed =>
            maxFallSpeed;

        public float GroundedStickSpeed =>
            groundedStickSpeed;

        public float JumpHeight =>
            jumpHeight;

        public float ExternalVelocityDecay =>
            externalVelocityDecay;
        

        public float SprintForwardThreshold =>
            sprintForwardThreshold;
        
        public float GroundProbeRadius => groundProbeRadius;
        public float GroundProbeStartHeight => groundProbeStartHeight;
        public float GroundProbeDistance => groundProbeDistance;
        public LayerMask GroundLayers => groundLayers;
    }
}