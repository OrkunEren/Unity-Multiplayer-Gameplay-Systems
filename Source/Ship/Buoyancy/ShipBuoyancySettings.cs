using UnityEngine;

namespace InvadersOverboard.Ship.Buoyancy
{
    [CreateAssetMenu(
        fileName = "SO_ShipBuoyancy_",
        menuName = "Invaders Overboard/Ship/Buoyancy Settings")]
    public sealed class ShipBuoyancySettings :
        ScriptableObject
    {
        [Header("Lift")]

        [Tooltip(
            "Maximum mass the ship can support when all buoyancy points are fully submerged.")]
        [SerializeField, Min(1f)]
        private float maximumSupportedMass = 1000f;

        [Tooltip(
            "Submersion depth at which a point reaches maximum buoyant force.")]
        [SerializeField, Min(0.01f)]
        private float maximumSubmersionDepth = 0.8f;


        [Header("Water Resistance")]

        [Tooltip(
            "Reduces vertical oscillation of the ship on the water.")]
        [SerializeField, Min(0f)]
        private float verticalDamping = 3f;

        [Tooltip(
            "Reduces uncontrolled lateral sliding of the ship on the water.")]
        [SerializeField, Min(0f)]
        private float horizontalDrag = 0.6f;


        public float MaximumSupportedMass =>
            maximumSupportedMass;

        public float MaximumSubmersionDepth =>
            maximumSubmersionDepth;

        public float VerticalDamping =>
            verticalDamping;

        public float HorizontalDrag =>
            horizontalDrag;
    }
}