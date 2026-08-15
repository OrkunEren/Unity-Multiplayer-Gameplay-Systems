using InvadersOverboard.Ship.Physics;
using InvadersOverboard.Water;
using UnityEngine;

namespace InvadersOverboard.Ship.Buoyancy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ShipPhysicsBody))]
    public sealed class ShipBuoyancy :
        MonoBehaviour
    {
        [Header("Dependencies")]

        [SerializeField]
        private ShipBuoyancySettings settings;

        [SerializeField]
        private WaterSurfaceProvider waterProvider;


        [Header("Buoyancy Points")]

        [SerializeField]
        private Transform[] buoyancyPoints;


        private ShipPhysicsBody physicsBody;

        private Vector3[] samplePositions;
        private WaterSample[] waterSamples;


        public int SubmergedPointCount
        {
            get;
            private set;
        }

        public float AverageSubmersion
        {
            get;
            private set;
        }


        private void Awake()
        {
            physicsBody =
                GetComponent<ShipPhysicsBody>();

            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            CreateSampleBuffers();
        }

        private void FixedUpdate()
        {
            if (!physicsBody.IsSimulationEnabled)
                return;

            UpdateSamplePositions();

            bool sampleSucceeded =
                waterProvider.TrySample(
                    samplePositions,
                    waterSamples);

            if (!sampleSucceeded)
            {
                SubmergedPointCount = 0;
                AverageSubmersion = 0f;
                return;
            }

            ApplyBuoyancyForces();
        }

        private void UpdateSamplePositions()
        {
            for (int i = 0;
                 i < buoyancyPoints.Length;
                 i++)
            {
                samplePositions[i] =
                    buoyancyPoints[i].position;
            }
        }

        private void ApplyBuoyancyForces()
        {
            int pointCount =
                buoyancyPoints.Length;

            float gravityMagnitude =
                Mathf.Abs(
                    UnityEngine.Physics.gravity.y);

            float maximumLiftPerPoint =
                settings.MaximumSupportedMass
                * gravityMagnitude
                / pointCount;

            float dampingMassPerPoint =
                physicsBody.Mass
                / pointCount;

            float totalSubmersion = 0f;
            int submergedPointCount = 0;

            for (int i = 0;
                 i < pointCount;
                 i++)
            {
                Vector3 pointPosition =
                    samplePositions[i];

                float depth =
                    waterSamples[i]
                        .GetSubmersionDepth(
                            pointPosition);

                if (depth <= 0f)
                    continue;

                float submersion =
                    Mathf.Clamp01(
                        depth
                        / settings
                            .MaximumSubmersionDepth);

                Vector3 pointVelocity =
                    physicsBody.GetPointVelocity(
                        pointPosition);

                Vector3 buoyancyForce =
                    CalculateBuoyancyForce(
                        maximumLiftPerPoint,
                        submersion);

                Vector3 dampingForce =
                    CalculateVerticalDamping(
                        pointVelocity,
                        dampingMassPerPoint,
                        submersion);

                Vector3 dragForce =
                    CalculateHorizontalDrag(
                        pointVelocity,
                        dampingMassPerPoint,
                        submersion);

                Vector3 totalForce =
                    buoyancyForce
                    + dampingForce
                    + dragForce;

                physicsBody.AddForceAtPosition(
                    totalForce,
                    pointPosition,
                    ForceMode.Force);

                totalSubmersion += submersion;
                submergedPointCount++;
            }

            SubmergedPointCount =
                submergedPointCount;

            AverageSubmersion =
                pointCount > 0
                    ? totalSubmersion / pointCount
                    : 0f;
        }

        private static Vector3 CalculateBuoyancyForce(
            float maximumLiftPerPoint,
            float submersion)
        {
            return Vector3.up
                * maximumLiftPerPoint
                * submersion;
        }

        private Vector3 CalculateVerticalDamping(
            Vector3 pointVelocity,
            float massPerPoint,
            float submersion)
        {
            float verticalSpeed =
                Vector3.Dot(
                    pointVelocity,
                    Vector3.up);

            return -Vector3.up
                * verticalSpeed
                * massPerPoint
                * settings.VerticalDamping
                * submersion;
        }

        private Vector3 CalculateHorizontalDrag(
            Vector3 pointVelocity,
            float massPerPoint,
            float submersion)
        {
            Vector3 verticalVelocity =
                Vector3.up
                * Vector3.Dot(
                    pointVelocity,
                    Vector3.up);

            Vector3 horizontalVelocity =
                pointVelocity
                - verticalVelocity;

            return -horizontalVelocity
                * massPerPoint
                * settings.HorizontalDrag
                * submersion;
        }

        private void CreateSampleBuffers()
        {
            int pointCount =
                buoyancyPoints.Length;

            samplePositions =
                new Vector3[pointCount];

            waterSamples =
                new WaterSample[pointCount];
        }

        private bool ValidateReferences()
        {
            if (settings == null)
            {
                Debug.LogError(
                    $"{nameof(ShipBuoyancy)} requires buoyancy settings.",
                    this);

                return false;
            }

            if (waterProvider == null)
            {
                Debug.LogError(
                    $"{nameof(ShipBuoyancy)} requires a water provider.",
                    this);

                return false;
            }

            if (buoyancyPoints == null ||
                buoyancyPoints.Length == 0)
            {
                Debug.LogError(
                    $"{nameof(ShipBuoyancy)} requires buoyancy points.",
                    this);

                return false;
            }

            for (int i = 0;
                 i < buoyancyPoints.Length;
                 i++)
            {
                if (buoyancyPoints[i] != null)
                    continue;

                Debug.LogError(
                    $"Buoyancy point at index {i} is missing.",
                    this);

                return false;
            }

            return true;
        }

        public void SetWaterProvider(
            WaterSurfaceProvider provider)
        {
            waterProvider = provider;
        }
    }
}