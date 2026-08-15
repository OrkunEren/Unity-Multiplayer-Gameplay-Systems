using UnityEngine;

namespace InvadersOverboard.Ship.Physics
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ShipPhysicsBody : MonoBehaviour
    {
        [SerializeField]
        private ShipPhysicsSettings settings;

        [Tooltip(
            "Keep disabled until the buoyancy system is configured.")]
        [SerializeField]
        private bool simulationEnabledOnStart;

        private Rigidbody body;

        public ShipPhysicsSettings Settings =>
            settings;

        public bool IsSimulationEnabled
        {
            get;
            private set;
        }

        public Vector3 Position =>
            body.position;

        public Quaternion Rotation =>
            body.rotation;

        public Vector3 LinearVelocity =>
            body.linearVelocity;

        public Vector3 AngularVelocity =>
            body.angularVelocity;

        public float Mass =>
            body.mass;

        public Vector3 LocalCenterOfMass =>
            body.centerOfMass;

        public Vector3 WorldCenterOfMass =>
            body.worldCenterOfMass;


        private void Awake()
        {
            body = GetComponent<Rigidbody>();

            if (settings == null)
            {
                Debug.LogError(
                    $"{nameof(ShipPhysicsBody)} requires physics settings.",
                    this);

                enabled = false;
                return;
            }

            ApplyBaseSettings();

            SetSimulationEnabled(
                simulationEnabledOnStart);
        }


        private void ApplyBaseSettings()
        {
            body.mass =
                settings.BaseMass;

            body.centerOfMass =
                settings.BaseCenterOfMass;

            body.linearDamping =
                settings.LinearDamping;

            body.angularDamping =
                settings.AngularDamping;

            body.maxLinearVelocity =
                settings.MaxLinearVelocity;

            body.maxAngularVelocity =
                settings.MaxAngularVelocity;

            body.interpolation =
                settings.Interpolation;

            body.collisionDetectionMode =
                settings.CollisionDetection;

            body.useGravity =
                settings.UseGravity;
        }


        // =====================================================
        // FORCE API
        // =====================================================

        public void AddForce(
            Vector3 force,
            ForceMode forceMode = ForceMode.Force)
        {
            if (!IsSimulationEnabled)
                return;

            body.AddForce(force, forceMode);
        }

        public void AddForceAtPosition(
            Vector3 force,
            Vector3 worldPosition,
            ForceMode forceMode = ForceMode.Force)
        {
            if (!IsSimulationEnabled)
                return;

            body.AddForceAtPosition(
                force,
                worldPosition,
                forceMode);
        }

        public void AddTorque(
            Vector3 torque,
            ForceMode forceMode = ForceMode.Force)
        {
            if (!IsSimulationEnabled)
                return;

            body.AddTorque(torque, forceMode);
        }

        public Vector3 GetPointVelocity(
            Vector3 worldPosition)
        {
            return body.GetPointVelocity(
                worldPosition);
        }


        // =====================================================
        // MASS PROPERTIES
        // =====================================================

        public void SetMassProperties(
            float totalMass,
            Vector3 localCenterOfMass)
        {
            body.mass =
                Mathf.Max(1f, totalMass);

            body.centerOfMass =
                localCenterOfMass;
        }

        public void RestoreBaseMassProperties()
        {
            if (settings == null)
                return;

            SetMassProperties(
                settings.BaseMass,
                settings.BaseCenterOfMass);
        }


        // =====================================================
        // SIMULATION CONTROL
        // =====================================================

        public void SetSimulationEnabled(
            bool isEnabled)
        {
            // When Use Rigidbody For Motion is enabled, both authority and
            // client proxies are interpolated between render frames.
            body.interpolation =
                settings.Interpolation;

            if (isEnabled)
            {
                body.isKinematic = false;

                body.useGravity =
                    settings.UseGravity;

                body.WakeUp();
            }
            else
            {
                ResetMotion();

                body.useGravity = false;
                body.isKinematic = true;
            }

            IsSimulationEnabled =
                isEnabled;
        }

        public void ResetMotion()
        {
            if (body.isKinematic)
                return;

            body.linearVelocity =
                Vector3.zero;

            body.angularVelocity =
                Vector3.zero;
        }
    }
}