using Unity.Netcode;
using UnityEngine;

namespace InvadersOverboard.Networking.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    public sealed class NetworkPlayerOwnership :
        NetworkBehaviour
    {
        [Tooltip(
            "Components that should run only for the owning player.")]
        [SerializeField]
        private Behaviour[] ownerOnlyBehaviours;

        [Tooltip(
            "Objects that should be active only on the owner.")]
        [SerializeField]
        private GameObject[] ownerOnlyObjects;

        [Tooltip(
            "Objects hidden on the owner and shown for remote players.")]
        [SerializeField]
        private GameObject[] ownerHiddenObjects;


        private void Awake()
        {
            // Before the network spawn completes, no prefab
            // should behave as if it were the local player.
            ApplyOwnershipState(false);
        }

        public override void OnNetworkSpawn()
        {
            ApplyOwnershipState(IsOwner);
        }

        public override void OnGainedOwnership()
        {
            ApplyOwnershipState(true);
        }

        public override void OnLostOwnership()
        {
            ApplyOwnershipState(false);
        }

        public override void OnNetworkDespawn()
        {
            ApplyOwnershipState(false);
        }

        private void ApplyOwnershipState(
            bool hasOwnership)
        {
            SetOwnerOnlyBehaviours(hasOwnership);
            SetGameObjects(
                ownerOnlyObjects,
                hasOwnership);

            SetGameObjects(
                ownerHiddenObjects,
                !hasOwnership);
        }

        private void SetOwnerOnlyBehaviours(
            bool isEnabled)
        {
            if (ownerOnlyBehaviours == null)
                return;

            if (isEnabled)
            {
                for (int i = 0;
                     i < ownerOnlyBehaviours.Length;
                     i++)
                {
                    Behaviour behaviour =
                        ownerOnlyBehaviours[i];

                    if (behaviour != null)
                        behaviour.enabled = true;
                }

                return;
            }

            // Disable in reverse order:
            // brain first, input last.
            for (int i = ownerOnlyBehaviours.Length - 1;
                 i >= 0;
                 i--)
            {
                Behaviour behaviour =
                    ownerOnlyBehaviours[i];

                if (behaviour != null)
                    behaviour.enabled = false;
            }
        }

        private static void SetGameObjects(
            GameObject[] objects,
            bool isActive)
        {
            if (objects == null)
                return;

            for (int i = 0;
                 i < objects.Length;
                 i++)
            {
                GameObject target =
                    objects[i];

                if (target != null &&
                    target.activeSelf != isActive)
                {
                    target.SetActive(isActive);
                }
            }
        }
    }
}