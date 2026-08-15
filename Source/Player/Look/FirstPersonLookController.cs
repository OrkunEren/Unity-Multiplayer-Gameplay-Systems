using UnityEngine;

namespace InvadersOverboard.Player.Look
{
    public sealed class FirstPersonLookController :
        MonoBehaviour
    {
        [Header("Configuration")]

        [SerializeField]
        private FirstPersonLookSettings settings;


        [Header("References")]

        [SerializeField]
        private Transform cameraPivot;


        [Header("Startup")]

        [SerializeField]
        private bool enableLookOnStart = true;


        private float pitch;
        private bool lookEnabled;


        public float Pitch =>
            pitch;

        public float Yaw =>
            transform.eulerAngles.y;
        
        public Quaternion ViewRotation =>
            Quaternion.Euler(
                pitch,
                Yaw,
                0f);

        public Vector3 ViewForward =>
            ViewRotation * Vector3.forward;

        public Vector3 ViewRight =>
            ViewRotation * Vector3.right;

        public bool IsLookEnabled =>
            lookEnabled;


        private void Awake()
        {
            if (settings == null)
            {
                Debug.LogError(
                    $"{nameof(FirstPersonLookSettings)} "
                    + $"is missing on {name}.",
                    this);
            }

            if (cameraPivot == null)
            {
                Debug.LogError(
                    $"Camera Pivot is missing on {name}.",
                    this);

                return;
            }


            pitch = NormalizeAngle(
                cameraPivot.localEulerAngles.x);
        }


        private void OnEnable()
        {
            if (enableLookOnStart)
                SetLookEnabled(true);
        }


        private void OnDisable()
        {
            SetLookEnabled(false);
        }


        public void Simulate(
            Vector2 lookInput,
            bool isMouseInput,
            float deltaTime)
        {
            if (!lookEnabled
                || settings == null
                || cameraPivot == null)
            {
                return;
            }


            float sensitivity =
                isMouseInput
                    ? settings.MouseSensitivity
                    : settings.GamepadSensitivity
                      * deltaTime;


            float yawDelta =
                lookInput.x * sensitivity;

            float pitchDelta =
                lookInput.y * sensitivity;


            transform.Rotate(
                0f,
                yawDelta,
                0f,
                Space.Self);


            float verticalDirection =
                settings.InvertY
                    ? 1f
                    : -1f;

            pitch +=
                pitchDelta * verticalDirection;

            pitch =
                Mathf.Clamp(
                    pitch,
                    settings.MinimumPitch,
                    settings.MaximumPitch);


            cameraPivot.localRotation =
                Quaternion.Euler(
                    pitch,
                    0f,
                    0f);
        }


        public void SetLookEnabled(
            bool enabled)
        {
            lookEnabled = enabled;


            if (enabled)
            {
                Cursor.lockState =
                    CursorLockMode.Locked;

                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState =
                    CursorLockMode.None;

                Cursor.visible = true;
            }
        }


        private static float NormalizeAngle(
            float angle)
        {
            if (angle > 180f)
                angle -= 360f;

            return angle;
        }
    }
}