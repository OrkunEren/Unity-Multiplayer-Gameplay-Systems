using System;
using InvadersOverboard.Player.Input;
using InvadersOverboard.Player.Look;
using InvadersOverboard.Player.Movement;
using InvadersOverboard.Player.States;
using InvadersOverboard.Player.Swimming;
using UnityEngine;

namespace InvadersOverboard.Player
{
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(CharacterMotor))]
    public sealed class PlayerBrain :
        MonoBehaviour
    {
        private PlayerInputReader inputReader;

        private CharacterMotor characterMotor;

        private FirstPersonLookController lookController;

        private CharacterGroundDetector groundDetector;

        private CharacterPlatformTracker platformTracker;

        private CharacterWaterDetector waterDetector;


        public PlayerLocomotionState CurrentState
        {
            get;
            private set;
        } = PlayerLocomotionState.Airborne;


        public PlayerSwimmingMode CurrentSwimmingMode
        {
            get;
            private set;
        } = PlayerSwimmingMode.None;


        public bool IsSprinting
        {
            get;
            private set;
        }


        public CharacterWaterInfo CurrentWaterInfo
        {
            get;
            private set;
        } = CharacterWaterInfo.None;


        public event Action<
            PlayerLocomotionState,
            PlayerLocomotionState> StateChanged;


        private void Awake()
        {
            inputReader =
                GetComponent<PlayerInputReader>();

            characterMotor =
                GetComponent<CharacterMotor>();

            lookController =
                GetComponent<FirstPersonLookController>();

            groundDetector =
                GetComponent<CharacterGroundDetector>();

            platformTracker =
                GetComponent<CharacterPlatformTracker>();

            waterDetector =
                GetComponentInChildren<
                    CharacterWaterDetector>(
                    true);


            if (groundDetector == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerBrain)} requires a " +
                    $"{nameof(CharacterGroundDetector)}.",
                    this);
            }

            if (platformTracker == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerBrain)} requires a " +
                    $"{nameof(CharacterPlatformTracker)}.",
                    this);
            }

            if (waterDetector == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerBrain)} requires a " +
                    $"{nameof(CharacterWaterDetector)}.",
                    this);
            }
        }


        private void Update()
        {
            if (characterMotor.Settings == null ||
                characterMotor.SwimmingSettings == null ||
                groundDetector == null ||
                platformTracker == null ||
                waterDetector == null)
            {
                return;
            }


            CurrentWaterInfo =
                waterDetector.Sample(
                    characterMotor.SwimmingSettings);

            bool isSwimming =
                CurrentWaterInfo.IsSwimming;


            CharacterPlatformMotion platformMotion =
                CharacterPlatformMotion.None;


            if (isSwimming)
            {
                platformTracker.Clear();
            }
            else
            {
                platformMotion =
                    platformTracker.SampleMotion();

                platformTracker.ApplyYaw(
                    platformMotion);
            }


            lookController?.Simulate(
                inputReader.LookInput,
                inputReader.IsLookInputFromMouse,
                Time.deltaTime);


            CharacterGroundingInfo predictedGrounding =
                isSwimming
                    ? CharacterGroundingInfo.None
                    : groundDetector.Probe(
                        characterMotor.Settings,
                        platformMotion.Displacement);


            Vector2 moveInput =
                Vector2.ClampMagnitude(
                    inputReader.MoveInput,
                    1f);


            UpdateSwimmingMode(
                isSwimming,
                moveInput);


            CharacterMotorCommand command =
                BuildMotorCommand(
                    isSwimming,
                    moveInput);


            characterMotor.Simulate(
                command,
                predictedGrounding,
                platformMotion.Displacement,
                CurrentWaterInfo,
                Time.deltaTime);


            if (isSwimming)
            {
                platformTracker.Clear();
            }
            else
            {
                CharacterGroundingInfo finalGrounding =
                    groundDetector.Probe(
                        characterMotor.Settings);

                platformTracker.Commit(
                    finalGrounding,
                    characterMotor.IsGrounded);
            }


            UpdateLocomotionState(
                CurrentWaterInfo);
        }


        private CharacterMotorCommand BuildMotorCommand(
            bool isSwimming,
            Vector2 moveInput)
        {
            bool hasMovementInput =
                moveInput.sqrMagnitude >
                0.0001f;


            float forwardDirection =
                hasMovementInput
                    ? moveInput.normalized.y
                    : 0f;

            bool canSprintForward =
                forwardDirection >=
                characterMotor.Settings
                    .SprintForwardThreshold;


            IsSprinting =
                !isSwimming
                && hasMovementInput
                && inputReader.IsSprintHeld
                && canSprintForward;


            float movementSpeed;

            if (isSwimming)
            {
                movementSpeed =
                    characterMotor
                        .SwimmingSettings
                        .SwimSpeed;
            }
            else
            {
                movementSpeed =
                    IsSprinting
                        ? characterMotor.Settings
                            .SprintSpeed
                        : characterMotor.Settings
                            .MoveSpeed;
            }


            Vector3 desiredDirection;

            if (isSwimming &&
                CurrentSwimmingMode ==
                PlayerSwimmingMode.Underwater)
            {
                desiredDirection =
                    CalculateUnderwaterDirection(
                        moveInput);
            }
            else
            {
                desiredDirection =
                    transform.right * moveInput.x
                    + transform.forward * moveInput.y;

                desiredDirection.y = 0f;
            }


            desiredDirection =
                Vector3.ClampMagnitude(
                    desiredDirection,
                    1f);


            Vector3 desiredVelocity =
                desiredDirection
                * movementSpeed;


            bool ascendRequested =
                isSwimming
                && CurrentSwimmingMode ==
                PlayerSwimmingMode.Underwater
                && inputReader.IsJumpHeld;


            if (ascendRequested)
            {
                desiredVelocity.y =
                    Mathf.Max(
                        desiredVelocity.y,
                        characterMotor
                            .SwimmingSettings
                            .AscentSpeed);

                // Prevent total swim speed from increasing when Space and
                // horizontal movement are used together.
                desiredVelocity =
                    Vector3.ClampMagnitude(
                        desiredVelocity,
                        movementSpeed);
            }


            bool jumpPressed =
                inputReader.ConsumeJumpRequest();

            bool jumpRequested =
                !isSwimming
                && CurrentState ==
                PlayerLocomotionState.Grounded
                && jumpPressed;


            bool followWaterSurface =
                isSwimming
                && CurrentSwimmingMode ==
                PlayerSwimmingMode.Surface;


            return new CharacterMotorCommand(
                desiredVelocity,
                jumpRequested,
                followWaterSurface);
        }


        private void UpdateSwimmingMode(
            bool isSwimming,
            Vector2 moveInput)
        {
            if (!isSwimming)
            {
                CurrentSwimmingMode =
                    PlayerSwimmingMode.None;

                return;
            }


            if (CurrentSwimmingMode ==
                PlayerSwimmingMode.None)
            {
                CurrentSwimmingMode =
                    PlayerSwimmingMode.Surface;
            }


            if (CurrentSwimmingMode ==
                PlayerSwimmingMode.Surface)
            {
                if (ShouldStartDive(
                        moveInput))
                {
                    CurrentSwimmingMode =
                        PlayerSwimmingMode.Underwater;
                }

                return;
            }


            if (ShouldReturnToSurface(
                    moveInput))
            {
                CurrentSwimmingMode =
                    PlayerSwimmingMode.Surface;
            }
        }


        private bool ShouldStartDive(
            Vector2 moveInput)
        {
            if (lookController == null)
                return false;


            CharacterSwimmingSettings settings =
                characterMotor.SwimmingSettings;


            return moveInput.y >=
                   settings.DiveForwardInputThreshold
                   && lookController.Pitch >=
                   settings.DivePitchThreshold;
        }


        private bool ShouldReturnToSurface(
            Vector2 moveInput)
        {
            CharacterSwimmingSettings settings =
                characterMotor.SwimmingSettings;


            if (CurrentWaterInfo.SubmersionDepth >
                settings.SurfaceReturnDepth)
            {
                return false;
            }
            
            if (inputReader.IsJumpHeld)
            {
                return true;
            }


            bool hasMovementInput =
                moveInput.sqrMagnitude >
                0.0001f;

            if (!hasMovementInput)
                return true;


            Vector3 desiredDirection =
                CalculateUnderwaterDirection(
                    moveInput);


            return desiredDirection.y >= 0f;
        }


        private Vector3 CalculateUnderwaterDirection(
            Vector2 moveInput)
        {
            Vector3 viewRight =
                lookController != null
                    ? lookController.ViewRight
                    : transform.right;

            Vector3 viewForward =
                lookController != null
                    ? lookController.ViewForward
                    : transform.forward;


            Vector3 desiredDirection =
                viewRight * moveInput.x
                + viewForward * moveInput.y;


            return Vector3.ClampMagnitude(
                desiredDirection,
                1f);
        }


        private void UpdateLocomotionState(
            in CharacterWaterInfo waterInfo)
        {
            PlayerLocomotionState nextState;


            if (waterInfo.IsSwimming)
            {
                nextState =
                    PlayerLocomotionState.Swimming;
            }
            else
            {
                nextState =
                    characterMotor.IsGrounded
                        ? PlayerLocomotionState.Grounded
                        : PlayerLocomotionState.Airborne;
            }


            ChangeState(
                nextState);
        }


        private void ChangeState(
            PlayerLocomotionState nextState)
        {
            if (CurrentState == nextState)
                return;


            PlayerLocomotionState previousState =
                CurrentState;

            CurrentState =
                nextState;


            StateChanged?.Invoke(
                previousState,
                nextState);
        }


        private void OnDisable()
        {
            IsSprinting = false;

            CurrentWaterInfo =
                CharacterWaterInfo.None;

            CurrentSwimmingMode =
                PlayerSwimmingMode.None;
        }
    }
}