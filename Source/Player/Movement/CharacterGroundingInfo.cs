using UnityEngine;

namespace InvadersOverboard.Player.Movement
{
    public readonly struct CharacterGroundingInfo
    {
        public static CharacterGroundingInfo None => default;

        public bool IsGrounded { get; }
        public RaycastHit Hit { get; }

        public Vector3 GroundNormal =>
            IsGrounded ? Hit.normal : Vector3.up;

        public Transform GroundTransform =>
            IsGrounded ? Hit.transform : null;

        public Rigidbody GroundRigidbody =>
            IsGrounded ? Hit.rigidbody : null;

        public CharacterGroundingInfo(bool isGrounded, RaycastHit hit)
        {
            IsGrounded = isGrounded;
            Hit = hit;
        }
    }
}
