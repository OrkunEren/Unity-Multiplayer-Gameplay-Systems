using InvadersOverboard.Ship.Physics;
using InvadersOverboard.Ship.Buoyancy;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace InvadersOverboard.Networking.Ship
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(ShipPhysicsBody))]
    public sealed class NetworkShipBridge : NetworkBehaviour
    {
        [SerializeField] private ShipPhysicsBody physicsBody;
        [SerializeField] private ShipBuoyancy shipBuoyancy;

        private void Reset()
        {
            physicsBody = GetComponent<ShipPhysicsBody>();
            shipBuoyancy = GetComponent<ShipBuoyancy>();
        }

        private void Awake()
        {
            if (physicsBody == null)
                physicsBody = GetComponent<ShipPhysicsBody>();

            if (shipBuoyancy == null)
                shipBuoyancy = GetComponent<ShipBuoyancy>();
        }

        public override void OnNetworkSpawn()
        {
            SetServerAuthority(IsServer);
        }

        public override void OnNetworkDespawn()
        {
            SetServerAuthority(false);
        }

        private void SetServerAuthority(bool hasAuthority)
        {
            if (shipBuoyancy != null)
                shipBuoyancy.enabled = hasAuthority;

            physicsBody.SetSimulationEnabled(hasAuthority);
        }
    }
}