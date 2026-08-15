using InvadersOverboard.Player;
using InvadersOverboard.Player.Movement;
using InvadersOverboard.Player.States;
using InvadersOverboard.Player.Visuals;
using Unity.Netcode;
using UnityEngine;

namespace InvadersOverboard.Networking.Player
{
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterMotor))]
    [RequireComponent(typeof(PlayerBrain))]
    [RequireComponent(
        typeof(NetworkPlayerVisualSelector))]
    public sealed class NetworkPlayerAnimationPresenter :
        NetworkBehaviour
    {
        [Header("Smoothing")]

        [SerializeField, Min(0f)]
        private float locomotionDampTime = 0.05f;

        [SerializeField, Min(0f)]
        private float verticalDampTime = 0.05f;


        private readonly NetworkVariable<
            NetworkPlayerAnimationState> animationState =
            new(
                default,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner);


        private static readonly int MoveXHash =
            Animator.StringToHash("MoveX");

        private static readonly int MoveYHash =
            Animator.StringToHash("MoveY");

        private static readonly int MoveAmountHash =
            Animator.StringToHash("MoveAmount");

        private static readonly int VerticalSpeedHash =
            Animator.StringToHash("VerticalSpeed");

        private static readonly int IsGroundedHash =
            Animator.StringToHash("IsGrounded");

        private static readonly int IsSwimmingHash =
            Animator.StringToHash("IsSwimming");


        private CharacterMotor characterMotor;

        private PlayerBrain playerBrain;

        private NetworkPlayerVisualSelector
            visualSelector;

        private Animator activeAnimator;

        private NetworkPlayerAnimationState
            targetState;
        
        public NetworkPlayerAnimationState CurrentState =>
            targetState;


        private void Awake()
        {
            characterMotor =
                GetComponent<CharacterMotor>();

            playerBrain =
                GetComponent<PlayerBrain>();

            visualSelector =
                GetComponent<
                    NetworkPlayerVisualSelector>();
        }


        public override void OnNetworkSpawn()
        {
            animationState.OnValueChanged +=
                HandleAnimationStateChanged;

            visualSelector.VisualSpawned +=
                HandleVisualSpawned;

            visualSelector.VisualDespawned +=
                HandleVisualDespawned;


            targetState =
                animationState.Value;

            BindVisual(
                visualSelector.ActiveVisual);


            if (IsOwner)
            {
                CaptureOwnerState();
            }

            ApplyStateImmediately();
        }


        public override void OnNetworkDespawn()
        {
            animationState.OnValueChanged -=
                HandleAnimationStateChanged;

            visualSelector.VisualSpawned -=
                HandleVisualSpawned;

            visualSelector.VisualDespawned -=
                HandleVisualDespawned;

            activeAnimator = null;
        }


        private void Update()
        {
            if (!IsSpawned)
                return;

            if (IsOwner)
            {
                CaptureOwnerState();
            }

            ApplyTargetState(
                Time.deltaTime);
        }


        private void CaptureOwnerState()
        {
            NetworkPlayerAnimationState nextState =
                BuildAnimationState();

            targetState =
                nextState;

            if (!nextState.Equals(
                    animationState.Value))
            {
                animationState.Value =
                    nextState;
            }
        }
        
        private void GetPresentedMovement(
            out float moveX,
            out float moveY,
            out float moveAmount)
        {
            moveX = targetState.MoveX;
            moveY = targetState.MoveY;
            moveAmount = targetState.MoveAmount;

            if (!IsOwner &&
                targetState.IsSwimming)
            {
                moveX = 0f;
                moveY = moveAmount;
            }
        }


        private NetworkPlayerAnimationState
            BuildAnimationState()
        {
            Vector3 localVelocity =
                transform.InverseTransformDirection(
                    characterMotor.PlanarVelocity);

            Vector2 planarVelocity =
                new(
                    localVelocity.x,
                    localVelocity.z);


            PlayerLocomotionState locomotionState =
                playerBrain.CurrentState;

            bool isGrounded =
                locomotionState ==
                PlayerLocomotionState.Grounded;

            bool isSwimming =
                locomotionState ==
                PlayerLocomotionState.Swimming;


            Vector2 blendMovement =
                isSwimming
                    ? CalculateSwimmingBlendMovement(
                        planarVelocity)
                    : CalculateGroundBlendMovement(
                        planarVelocity);


            return new NetworkPlayerAnimationState(
                blendMovement.x,
                blendMovement.y,
                characterMotor.VerticalVelocity,
                isGrounded,
                isSwimming);
        }


        private Vector2 CalculateGroundBlendMovement(
            Vector2 planarVelocity)
        {
            float sprintSpeed =
                characterMotor.Settings != null
                    ? characterMotor.Settings
                        .SprintSpeed
                    : 0f;

            if (sprintSpeed <= 0.001f)
                return Vector2.zero;


            // Normalize actual velocity directly against the maximum
            // ground speed.
            return Vector2.ClampMagnitude(
                planarVelocity / sprintSpeed,
                1f);
        }


        private Vector2 CalculateSwimmingBlendMovement(
            Vector2 planarVelocity)
        {
            float swimSpeed =
                characterMotor.SwimmingSettings != null
                    ? characterMotor
                        .SwimmingSettings
                        .SwimSpeed
                    : 0f;

            if (swimSpeed <= 0.001f)
                return Vector2.zero;


            // The swim Blend Tree uses its own 0-1 range.
            return Vector2.ClampMagnitude(
                planarVelocity / swimSpeed,
                1f);
        }


        private void HandleAnimationStateChanged(
            NetworkPlayerAnimationState previousState,
            NetworkPlayerAnimationState currentState)
        {
            if (IsOwner)
                return;

            targetState =
                currentState;
        }


        private void HandleVisualSpawned(
            PlayerVisualInstance visual)
        {
            BindVisual(
                visual);

            ApplyStateImmediately();
        }


        private void HandleVisualDespawned()
        {
            activeAnimator = null;
        }


        private void BindVisual(
            PlayerVisualInstance visual)
        {
            activeAnimator =
                visual != null
                    ? visual.Animator
                    : null;
        }


        private void ApplyTargetState(
            float deltaTime)
        {
            GetPresentedMovement(
                out float presentedMoveX,
                out float presentedMoveY,
                out float presentedMoveAmount);
            
            if (activeAnimator == null ||
                !activeAnimator.isActiveAndEnabled)
            {
                return;
            }


            activeAnimator.SetFloat(
                MoveXHash,
                presentedMoveX,
                locomotionDampTime,
                deltaTime);

            activeAnimator.SetFloat(
                MoveYHash,
                presentedMoveY,
                locomotionDampTime,
                deltaTime);

            activeAnimator.SetFloat(
                MoveAmountHash,
                presentedMoveAmount,
                locomotionDampTime,
                deltaTime);

            activeAnimator.SetFloat(
                VerticalSpeedHash,
                targetState.VerticalSpeed,
                verticalDampTime,
                deltaTime);

            activeAnimator.SetBool(
                IsGroundedHash,
                targetState.IsGrounded);

            activeAnimator.SetBool(
                IsSwimmingHash,
                targetState.IsSwimming);
        }


        private void ApplyStateImmediately()
        {
            if (activeAnimator == null ||
                !activeAnimator.isActiveAndEnabled)
            {
                return;
            }

            GetPresentedMovement(
                out float presentedMoveX,
                out float presentedMoveY,
                out float presentedMoveAmount);

            activeAnimator.SetFloat(
                MoveXHash,
                presentedMoveX);

            activeAnimator.SetFloat(
                MoveYHash,
                presentedMoveY);

            activeAnimator.SetFloat(
                MoveAmountHash,
                presentedMoveAmount);

            activeAnimator.SetFloat(
                VerticalSpeedHash,
                targetState.VerticalSpeed);

            activeAnimator.SetBool(
                IsGroundedHash,
                targetState.IsGrounded);

            activeAnimator.SetBool(
                IsSwimmingHash,
                targetState.IsSwimming);
        }
    }
}