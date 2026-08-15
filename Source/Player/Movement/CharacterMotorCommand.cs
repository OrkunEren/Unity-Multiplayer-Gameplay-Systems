using UnityEngine;

namespace InvadersOverboard.Player.Movement
{
    public readonly struct CharacterMotorCommand
    {
        public static CharacterMotorCommand Idle =>
            new(
                Vector3.zero,
                false,
                false);


        public Vector3 DesiredVelocity
        {
            get;
        }


        public Vector3 DesiredPlanarVelocity
        {
            get
            {
                Vector3 planarVelocity =
                    DesiredVelocity;

                planarVelocity.y = 0f;

                return planarVelocity;
            }
        }


        public float DesiredVerticalVelocity =>
            DesiredVelocity.y;


        public bool JumpRequested
        {
            get;
        }


        public bool FollowWaterSurface
        {
            get;
        }


        public CharacterMotorCommand(
            Vector3 desiredVelocity,
            bool jumpRequested,
            bool followWaterSurface)
        {
            DesiredVelocity =
                desiredVelocity;

            JumpRequested =
                jumpRequested;

            FollowWaterSurface =
                followWaterSurface;
        }
    }
}