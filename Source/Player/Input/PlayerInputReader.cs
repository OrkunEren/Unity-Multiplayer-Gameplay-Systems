using UnityEngine;
using UnityEngine.InputSystem;

namespace InvadersOverboard.Player.Input
{
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [Header("Startup")]
        [SerializeField]
        private bool enableInputOnStart = true;


        private PlayerInputActions inputActions;

        private bool inputEnabled;
        private bool jumpRequested;
        private bool interactRequested;


        public Vector2 MoveInput =>
            inputEnabled
                ? inputActions.Gameplay.Move.ReadValue<Vector2>()
                : Vector2.zero;

        public bool IsLookInputFromMouse =>
            inputEnabled
            && inputActions.Gameplay.Look
                .activeControl?.device is Mouse;

        public Vector2 LookInput =>
            inputEnabled
                ? inputActions.Gameplay.Look.ReadValue<Vector2>()
                : Vector2.zero;

        public bool IsSprintHeld =>
            inputEnabled
            && inputActions.Gameplay.Sprint.IsPressed();

        public bool IsJumpHeld =>
            inputEnabled
            && inputActions.Gameplay.Jump.IsPressed();

        public bool IsInteractHeld =>
            inputEnabled &&
            inputActions.Gameplay.Interact.IsPressed();

        public bool IsInputEnabled =>
            inputEnabled;


        private void Awake()
        {
            inputActions = new PlayerInputActions();
        }


        private void OnEnable()
        {
            if (enableInputOnStart)
                SetInputEnabled(true);
        }


        private void OnDisable()
        {
            SetInputEnabled(false);
        }


        private void OnDestroy()
        {
            inputActions?.Dispose();
        }


        public void SetInputEnabled(bool enabled)
        {
            if (inputEnabled == enabled)
                return;


            inputEnabled = enabled;

            if (enabled)
            {
                inputActions.Gameplay.Jump.performed +=
                    OnJumpPerformed;

                inputActions.Gameplay.Interact.performed +=
                    OnInteractPerformed;

                inputActions.Gameplay.Enable();
            }
            else
            {
                inputActions.Gameplay.Jump.performed -=
                    OnJumpPerformed;

                inputActions.Gameplay.Interact.performed -=
                    OnInteractPerformed;

                inputActions.Gameplay.Disable();

                jumpRequested = false;
                interactRequested = false;
            }
        }


        public bool ConsumeInteractRequest()
        {
            if (!interactRequested)
                return false;

            interactRequested = false;

            return true;
        }


        private void OnInteractPerformed(
            InputAction.CallbackContext context)
        {
            interactRequested = true;
        }


        public bool ConsumeJumpRequest()
        {
            if (!jumpRequested)
                return false;


            jumpRequested = false;

            return true;
        }


        private void OnJumpPerformed(
            InputAction.CallbackContext context)
        {
            jumpRequested = true;
        }
    }
}
