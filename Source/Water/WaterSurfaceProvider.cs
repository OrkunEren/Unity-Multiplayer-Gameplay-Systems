using StylizedWater3;
using UnityEngine;

namespace InvadersOverboard.Water
{
    [DisallowMultipleComponent]
    public sealed class WaterSurfaceProvider :
        MonoBehaviour
    {
        [SerializeField]
        private HeightQuerySystem.Interface heightInterface =
            new HeightQuerySystem.Interface();


        // For multi-point queries such as ships.
        private HeightQuerySystem.Sampler batchSampler;

        private int configuredBatchSampleCount;


        // For single-point queries such as players.
        private HeightQuerySystem.Sampler singleSampler;


        public bool IsReady =>
            enabled
            && heightInterface != null
            && !heightInterface.HasMissingReferences();


        private void Reset()
        {
            EnsureHeightInterface();

            heightInterface.method =
                HeightQuerySystem.Interface.Method.CPU;

            if (heightInterface.waterObject == null)
            {
                heightInterface.waterObject =
                    GetComponent<WaterObject>();
            }
        }


        private void OnValidate()
        {
            EnsureHeightInterface();

            // Always use CPU queries for gameplay.
            heightInterface.method =
                HeightQuerySystem.Interface.Method.CPU;
        }


        private void Awake()
        {
            EnsureHeightInterface();

            heightInterface.method =
                HeightQuerySystem.Interface.Method.CPU;

            ResolveWaterObject();

            if (heightInterface.HasMissingReferences())
            {
                Debug.LogError(
                    $"{nameof(WaterSurfaceProvider)} " +
                    "requires an SW3 WaterObject.",
                    this);

                enabled = false;
            }
        }


        // =====================================================
        // SINGLE SAMPLE API
        // =====================================================

        public bool TrySample(
            Vector3 worldPosition,
            out WaterSample result)
        {
            result = default;

            if (!IsReady)
                return false;

            EnsureSingleSampler();

            singleSampler.SetSamplePosition(
                0,
                worldPosition);

            Gerstner.ComputeHeight(
                singleSampler,
                heightInterface);

            float surfaceHeight =
                singleSampler.heightValues[0];

            if (float.IsNaN(surfaceHeight))
                return false;

            result =
                new WaterSample(
                    surfaceHeight);

            return true;
        }


        // =====================================================
        // BATCH SAMPLE API
        // =====================================================

        public bool TrySample(
            Vector3[] worldPositions,
            WaterSample[] results)
        {
            if (!IsReady)
                return false;

            if (worldPositions == null ||
                results == null)
            {
                return false;
            }

            int sampleCount =
                worldPositions.Length;

            if (sampleCount == 0)
                return true;

            if (results.Length < sampleCount)
            {
                Debug.LogError(
                    "Water sample result array is too small.",
                    this);

                return false;
            }

            EnsureBatchSampler(
                sampleCount);

            for (int i = 0;
                 i < sampleCount;
                 i++)
            {
                batchSampler.SetSamplePosition(
                    i,
                    worldPositions[i]);
            }

            Gerstner.ComputeHeight(
                batchSampler,
                heightInterface);

            for (int i = 0;
                 i < sampleCount;
                 i++)
            {
                float surfaceHeight =
                    batchSampler.heightValues[i];

                if (float.IsNaN(surfaceHeight))
                    return false;

                results[i] =
                    new WaterSample(
                        surfaceHeight);
            }

            return true;
        }
         
        // =====================================================
        // NETWORK
        // =====================================================
        
        public void SetSimulationTime(
            float simulationTime)
        {
            WaterObject.CustomTime =
                Mathf.Max(
                    0.0001f,
                    simulationTime);
        }


        public void ResetSimulationTime()
        {
            WaterObject.CustomTime = -1f;
        }


        // =====================================================
        // SETUP
        // =====================================================

        private void ResolveWaterObject()
        {
            if (heightInterface.waterObject != null)
                return;

            WaterObject localWaterObject =
                GetComponent<WaterObject>();

            if (localWaterObject != null)
            {
                heightInterface.waterObject =
                    localWaterObject;

                return;
            }

            // Search only once during initialization.
            heightInterface.GetWaterObject(
                transform.position);
        }


        private void EnsureHeightInterface()
        {
            if (heightInterface == null)
            {
                heightInterface =
                    new HeightQuerySystem.Interface();
            }
        }


        private void EnsureSingleSampler()
        {
            if (singleSampler != null)
                return;

            singleSampler =
                new HeightQuerySystem.Sampler();

            singleSampler.SetSampleCount(1);
        }


        private void EnsureBatchSampler(
            int sampleCount)
        {
            if (batchSampler == null)
            {
                batchSampler =
                    new HeightQuerySystem.Sampler();
            }

            if (configuredBatchSampleCount ==
                sampleCount)
            {
                return;
            }

            batchSampler.SetSampleCount(
                sampleCount);

            configuredBatchSampleCount =
                sampleCount;
        }


        // =====================================================
        // LIFECYCLE
        // =====================================================

        private void OnDisable()
        {
            DisposeSamplers();
        }


        private void OnDestroy()
        {
            DisposeSamplers();
        }


        private void DisposeSamplers()
        {
            if (singleSampler != null)
            {
                singleSampler.Dispose();
                singleSampler = null;
            }

            if (batchSampler != null)
            {
                batchSampler.Dispose();
                batchSampler = null;
            }

            configuredBatchSampleCount = 0;
        }
    }
}