using UnityEngine;

namespace InvadersOverboard.Ship.Physics
{
    [CreateAssetMenu(
        fileName = "SO_ShipPhysics_",
        menuName = "Invaders Overboard/Ship/Physics Settings")]
    public sealed class ShipPhysicsSettings : ScriptableObject
    {
        [Header("Mass")]

        [SerializeField, Min(1f)]
        private float baseMass = 500f;

        [SerializeField]
        private Vector3 baseCenterOfMass =
            new(0f, -0.4f, 0f);


        [Header("Resistance")]

        [SerializeField, Min(0f)]
        private float linearDamping = 0.05f;

        [SerializeField, Min(0f)]
        private float angularDamping = 0.8f;


        [Header("Limits")]

        [SerializeField, Min(0.1f)]
        private float maxLinearVelocity = 12f;

        [SerializeField, Min(0.1f)]
        private float maxAngularVelocity = 2f;


        [Header("Rigidbody")]

        [SerializeField]
        private bool useGravity = true;

        [SerializeField]
        private RigidbodyInterpolation interpolation =
            RigidbodyInterpolation.Interpolate;

        [SerializeField]
        private CollisionDetectionMode collisionDetection =
            CollisionDetectionMode.ContinuousSpeculative;


        public float BaseMass => baseMass;

        public Vector3 BaseCenterOfMass =>
            baseCenterOfMass;

        public float LinearDamping =>
            linearDamping;

        public float AngularDamping =>
            angularDamping;

        public float MaxLinearVelocity =>
            maxLinearVelocity;

        public float MaxAngularVelocity =>
            maxAngularVelocity;

        public bool UseGravity =>
            useGravity;

        public RigidbodyInterpolation Interpolation =>
            interpolation;

        public CollisionDetectionMode CollisionDetection =>
            collisionDetection;
    }
}