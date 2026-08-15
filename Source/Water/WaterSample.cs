using UnityEngine;

namespace InvadersOverboard.Water
{
    public readonly struct WaterSample
    {
        public float SurfaceHeight { get; }

        public WaterSample(float surfaceHeight)
        {
            SurfaceHeight = surfaceHeight;
        }

        public float GetSubmersionDepth(
            Vector3 worldPosition)
        {
            return Mathf.Max(
                0f,
                SurfaceHeight - worldPosition.y);
        }

        public bool IsSubmerged(
            Vector3 worldPosition)
        {
            return worldPosition.y < SurfaceHeight;
        }
    }
}