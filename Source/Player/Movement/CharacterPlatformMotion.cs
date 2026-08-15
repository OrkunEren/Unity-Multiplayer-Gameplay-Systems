using UnityEngine;

namespace InvadersOverboard.Player.Movement
{
    public readonly struct CharacterPlatformMotion
    {
        public static CharacterPlatformMotion None =>
            new(Vector3.zero, 0f);

        public Vector3 Displacement { get; }

        public float YawDelta { get; }

        public CharacterPlatformMotion(
            Vector3 displacement,
            float yawDelta)
        {
            Displacement = displacement;
            YawDelta = yawDelta;
        }
    }
}
