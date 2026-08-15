using UnityEngine;

namespace InvadersOverboard.Player.Look
{
    [CreateAssetMenu(
        fileName = "SO_FirstPersonLook",
        menuName =
            "Invaders Overboard/Player/First Person Look Settings")]
    public sealed class FirstPersonLookSettings :
        ScriptableObject
    {
        [Header("Sensitivity")]

        [SerializeField, Min(0f)]
        private float mouseSensitivity = 0.08f;

        [SerializeField, Min(0f)]
        private float gamepadSensitivity = 160f;


        [Header("Pitch")]

        [SerializeField, Range(-89f, 0f)]
        private float minimumPitch = -85f;

        [SerializeField, Range(0f, 89f)]
        private float maximumPitch = 85f;

        [SerializeField]
        private bool invertY;


        public float MouseSensitivity =>
            mouseSensitivity;

        public float GamepadSensitivity =>
            gamepadSensitivity;

        public float MinimumPitch =>
            minimumPitch;

        public float MaximumPitch =>
            maximumPitch;

        public bool InvertY =>
            invertY;
    }
}