namespace InvadersOverboard.Player.Swimming
{
    public readonly struct CharacterWaterInfo
    {
        public static CharacterWaterInfo None =>
            new(
                false,
                false,
                false,
                false,
                0f,
                0f);


        public bool HasValidSample
        {
            get;
        }

        public bool IsSwimming
        {
            get;
        }

        public bool EnteredSwimming
        {
            get;
        }

        public bool ExitedSwimming
        {
            get;
        }

        public float SurfaceHeight
        {
            get;
        }

        public float SubmersionDepth
        {
            get;
        }


        public CharacterWaterInfo(
            bool hasValidSample,
            bool isSwimming,
            bool enteredSwimming,
            bool exitedSwimming,
            float surfaceHeight,
            float submersionDepth)
        {
            HasValidSample =
                hasValidSample;

            IsSwimming =
                isSwimming;

            EnteredSwimming =
                enteredSwimming;

            ExitedSwimming =
                exitedSwimming;

            SurfaceHeight =
                surfaceHeight;

            SubmersionDepth =
                submersionDepth;
        }
    }
}