using InvadersOverboard.Player.Swimming;
using UnityEngine;

namespace InvadersOverboard.Player.Movement
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class CharacterMotor :
        MonoBehaviour
    {
        [SerializeField]
        private CharacterMovementSettings settings;
        
        [SerializeField]
        private CharacterSwimmingSettings swimmingSettings;
        public CharacterSwimmingSettings SwimmingSettings =>
            swimmingSettings;


        private CharacterController controller;

        private Vector3 planarVelocity;

        private float verticalVelocity;

        private Vector3 externalVelocity;


        public CharacterMovementSettings Settings =>
            settings;

        public bool IsGrounded
        {
            get;
            private set;
        }

        public Vector3 PlanarVelocity =>
            planarVelocity;

        public float VerticalVelocity =>
            verticalVelocity;

        public Vector3 ExternalVelocity =>
            externalVelocity;

        public Vector3 CurrentVelocity =>
            planarVelocity
            + Vector3.up * verticalVelocity
            + externalVelocity;


        private void Awake()
        {
            controller =
                GetComponent<CharacterController>();

            if (settings == null)
            {
                Debug.LogError(
                    $"{nameof(CharacterMotor)} " +
                    "requires movement settings.",
                    this);
            }
            
            if (swimmingSettings == null)
            {
                Debug.LogError(
                    $"{nameof(CharacterMotor)} requires " +
                    "swimming settings.",
                    this);
            }
        }


        public void Simulate(
            in CharacterMotorCommand command,
            in CharacterGroundingInfo grounding,
            Vector3 platformDisplacement,
            in CharacterWaterInfo waterInfo,
            float deltaTime)
        {
            if (settings == null ||
                deltaTime <= 0f)
            {
                return;
            }

            if (waterInfo.IsSwimming)
            {
                if (swimmingSettings == null)
                    return;

                SimulateSwimming(
                    command,
                    waterInfo,
                    swimmingSettings,
                    deltaTime);

                return;
            }

            SimulateGroundAndAir(
                command,
                grounding,
                platformDisplacement,
                deltaTime);
        }


        // =====================================================
        // GROUND AND AIR
        // =====================================================

        private void SimulateGroundAndAir(
            in CharacterMotorCommand command,
            in CharacterGroundingInfo grounding,
            Vector3 platformDisplacement,
            float deltaTime)
        {
            UpdatePlanarVelocity(
                command,
                settings.Acceleration,
                settings.Deceleration,
                deltaTime);

            bool probeCanSupportCharacter =
                grounding.IsGrounded
                && verticalVelocity <= 0f;

            bool hadGroundSupport =
                controller.isGrounded
                || probeCanSupportCharacter;

            bool jumpStarted =
                command.JumpRequested
                && hadGroundSupport;

            UpdateVerticalVelocity(
                jumpStarted,
                hadGroundSupport,
                deltaTime);

            Vector3 requestedVelocity =
                planarVelocity
                + Vector3.up * verticalVelocity
                + externalVelocity;

            Vector3 requestedDisplacement =
                requestedVelocity * deltaTime
                + platformDisplacement;

            CollisionFlags collisionFlags =
                controller.Move(
                    requestedDisplacement);

            ResolveGroundAndAirCollisions(
                collisionFlags,
                jumpStarted,
                grounding);

            DecayExternalVelocity(
                deltaTime);
        }


        private void UpdateVerticalVelocity(
            bool jumpStarted,
            bool hasGroundSupport,
            float deltaTime)
        {
            if (jumpStarted)
            {
                verticalVelocity =
                    Mathf.Sqrt(
                        2f
                        * settings.Gravity
                        * settings.JumpHeight);

                return;
            }

            if (hasGroundSupport &&
                verticalVelocity <= 0f)
            {
                verticalVelocity =
                    -settings.GroundedStickSpeed;

                return;
            }

            verticalVelocity =
                Mathf.Max(
                    verticalVelocity
                    - settings.Gravity * deltaTime,
                    -settings.MaxFallSpeed);
        }


        private void ResolveGroundAndAirCollisions(
            CollisionFlags collisionFlags,
            bool jumpStarted,
            in CharacterGroundingInfo grounding)
        {
            bool hitCeiling =
                (collisionFlags &
                 CollisionFlags.Above) != 0;

            if (hitCeiling &&
                verticalVelocity > 0f)
            {
                verticalVelocity = 0f;
            }

            bool groundedByMovement =
                (collisionFlags &
                 CollisionFlags.Below) != 0;

            bool groundedByProbe =
                grounding.IsGrounded
                && verticalVelocity <= 0f;

            IsGrounded =
                !jumpStarted
                && (groundedByMovement ||
                    groundedByProbe);

            if (IsGrounded &&
                verticalVelocity < 0f)
            {
                verticalVelocity =
                    -settings.GroundedStickSpeed;
            }
        }


        // =====================================================
        // SWIMMING
        // =====================================================

        private void SimulateSwimming(
            in CharacterMotorCommand command,
            in CharacterWaterInfo waterInfo,
            CharacterSwimmingSettings swimmingSettings,
            float deltaTime)
        {
            IsGrounded = false;

            UpdatePlanarVelocity(
                command,
                swimmingSettings.Acceleration,
                swimmingSettings.Deceleration,
                deltaTime);

            if (waterInfo.EnteredSwimming)
            {
                // Prevent the character from continuing to sink for too long
                // after entering the water at high speed.
                verticalVelocity =
                    Mathf.Clamp(
                        verticalVelocity,
                        -swimmingSettings
                            .MaximumVerticalSpeed,
                        swimmingSettings
                            .MaximumVerticalSpeed);
            }

            if (command.FollowWaterSurface)
            {
                UpdateSwimmingVerticalVelocity(
                    waterInfo,
                    swimmingSettings,
                    deltaTime);
            }
            else
            {
                UpdateUnderwaterVerticalVelocity(
                    command,
                    swimmingSettings,
                    deltaTime);
            }

            Vector3 requestedVelocity =
                planarVelocity
                + Vector3.up * verticalVelocity
                + externalVelocity;

            CollisionFlags collisionFlags =
                controller.Move(
                    requestedVelocity
                    * deltaTime);

            ResolveSwimmingCollisions(
                collisionFlags);

            DecayExternalVelocity(
                deltaTime);
        }


        private void UpdateSwimmingVerticalVelocity(
            in CharacterWaterInfo waterInfo,
            CharacterSwimmingSettings swimmingSettings,
            float deltaTime)
        {
            float targetWorldHeight =
                waterInfo.SurfaceHeight
                - swimmingSettings
                    .TargetSubmersionDepth;

            float heightError =
                targetWorldHeight
                - transform.position.y;

            float targetVerticalVelocity =
                heightError
                * swimmingSettings
                    .SurfaceFollowSharpness;

            targetVerticalVelocity =
                Mathf.Clamp(
                    targetVerticalVelocity,
                    -swimmingSettings
                        .MaximumVerticalSpeed,
                    swimmingSettings
                        .MaximumVerticalSpeed);

            verticalVelocity =
                Mathf.MoveTowards(
                    verticalVelocity,
                    targetVerticalVelocity,
                    swimmingSettings
                        .VerticalAcceleration
                    * deltaTime);
        }
        
        private void UpdateUnderwaterVerticalVelocity(
            in CharacterMotorCommand command,
            CharacterSwimmingSettings swimmingSettings,
            float deltaTime)
        {
            float targetVerticalVelocity =
                command.DesiredVerticalVelocity;

            bool hasVerticalInput =
                Mathf.Abs(
                    targetVerticalVelocity) >
                0.0001f;

            float velocityChangeRate =
                hasVerticalInput
                    ? swimmingSettings.Acceleration
                    : swimmingSettings.Deceleration;

            verticalVelocity =
                Mathf.MoveTowards(
                    verticalVelocity,
                    targetVerticalVelocity,
                    velocityChangeRate
                    * deltaTime);
        }


        private void ResolveSwimmingCollisions(
            CollisionFlags collisionFlags)
        {
            bool hitCeiling =
                (collisionFlags &
                 CollisionFlags.Above) != 0;

            if (hitCeiling &&
                verticalVelocity > 0f)
            {
                verticalVelocity = 0f;
            }

            bool hitGround =
                (collisionFlags &
                 CollisionFlags.Below) != 0;

            if (hitGround &&
                verticalVelocity < 0f)
            {
                verticalVelocity = 0f;
            }
        }


        // =====================================================
        // SHARED MOVEMENT
        // =====================================================

        private void UpdatePlanarVelocity(
            in CharacterMotorCommand command,
            float acceleration,
            float deceleration,
            float deltaTime)
        {
            Vector3 targetVelocity =
                command.DesiredPlanarVelocity;

            targetVelocity.y = 0f;

            bool hasMovementInput =
                targetVelocity.sqrMagnitude >
                0.0001f;

            float velocityChangeRate =
                hasMovementInput
                    ? acceleration
                    : deceleration;

            planarVelocity =
                Vector3.MoveTowards(
                    planarVelocity,
                    targetVelocity,
                    velocityChangeRate
                    * deltaTime);
        }


        private void DecayExternalVelocity(
            float deltaTime)
        {
            externalVelocity =
                Vector3.MoveTowards(
                    externalVelocity,
                    Vector3.zero,
                    settings.ExternalVelocityDecay
                    * deltaTime);
        }


        public void AddExternalVelocity(
            Vector3 velocity)
        {
            externalVelocity +=
                velocity;
        }


        public void ResetMotion()
        {
            planarVelocity = Vector3.zero;

            verticalVelocity = 0f;

            externalVelocity = Vector3.zero;

            IsGrounded = false;
        }
    }
}