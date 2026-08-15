using InvadersOverboard.Player.Visuals;
using Unity.Netcode;
using UnityEngine;
using System;

namespace InvadersOverboard.Networking.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    public sealed class NetworkPlayerVisualSelector :
        NetworkBehaviour
    {
        [Header("Dependencies")]

        [SerializeField]
        private PlayerVisualCatalog visualCatalog;

        [SerializeField]
        private Transform visualsRoot;


        [Header("Default")]

        [SerializeField]
        private PlayerVisualId defaultVisualId =
            PlayerVisualId.Lizard;


        private readonly NetworkVariable<
            PlayerVisualId> selectedVisualId =
            new(
                PlayerVisualId.Lizard,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);


        private PlayerVisualInstance spawnedVisual;

        public PlayerVisualId SelectedVisualId => selectedVisualId.Value;
        public PlayerVisualInstance ActiveVisual => spawnedVisual;

        public event Action<PlayerVisualInstance> VisualSpawned;
        public event Action VisualDespawned;


        private void Awake()
        {
            if (visualCatalog == null)
            {
                Debug.LogError(
                    $"{nameof(NetworkPlayerVisualSelector)} " +
                    "requires a visual catalog.",
                    this);
            }

            if (visualsRoot == null)
            {
                Debug.LogError(
                    $"{nameof(NetworkPlayerVisualSelector)} " +
                    "requires a VisualsRoot.",
                    this);
            }
        }


        public override void OnNetworkSpawn()
        {
            selectedVisualId.OnValueChanged +=
                HandleVisualChanged;

            if (IsServer &&
                visualCatalog != null &&
                visualCatalog.Contains(
                    defaultVisualId))
            {
                selectedVisualId.Value =
                    defaultVisualId;
            }

            SpawnSelectedVisual(
                selectedVisualId.Value);
        }


        public override void OnNetworkDespawn()
        {
            selectedVisualId.OnValueChanged -=
                HandleVisualChanged;

            DestroySpawnedVisual();
        }


        // A future character-selection screen can call this method.
        public void RequestVisual(
            PlayerVisualId requestedVisualId)
        {
            if (!IsSpawned ||
                !IsOwner ||
                visualCatalog == null ||
                !visualCatalog.Contains(
                    requestedVisualId))
            {
                return;
            }

            if (IsServer)
            {
                SetVisualOnServer(
                    requestedVisualId);

                return;
            }

            RequestVisualRpc(
                requestedVisualId);
        }


        [Rpc(
            SendTo.Server,
            InvokePermission = RpcInvokePermission.Owner)]
        private void RequestVisualRpc(
            PlayerVisualId requestedVisualId,
            RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId)
                return;

            SetVisualOnServer(requestedVisualId);
        }


        private void SetVisualOnServer(
            PlayerVisualId requestedVisualId)
        {
            if (!IsServer ||
                visualCatalog == null ||
                !visualCatalog.Contains(
                    requestedVisualId))
            {
                return;
            }

            selectedVisualId.Value =
                requestedVisualId;
        }


        private void HandleVisualChanged(
            PlayerVisualId previousVisualId,
            PlayerVisualId currentVisualId)
        {
            SpawnSelectedVisual(
                currentVisualId);
        }


        private void SpawnSelectedVisual(PlayerVisualId visualId)
        {
            DestroySpawnedVisual();

            if (visualCatalog == null || visualsRoot == null)
                return;

            if (!visualCatalog.TryGetPrefab(
                    visualId,
                    out GameObject visualPrefab) ||
                visualPrefab == null)
            {
                Debug.LogError(
                    $"{name}: No visual prefab found for {visualId}.",
                    this);

                return;
            }

            GameObject visualObject =
                Instantiate(visualPrefab, visualsRoot);

            visualObject.transform.SetLocalPositionAndRotation(
                Vector3.zero,
                Quaternion.identity);

            visualObject.transform.localScale = Vector3.one;

            if (!visualObject.TryGetComponent(
                    out PlayerVisualInstance visualInstance))
            {
                Debug.LogError(
                    $"On {visualPrefab.name}, " +
                    $"{nameof(PlayerVisualInstance)} was not found.",
                    visualObject);

                Destroy(visualObject);
                return;
            }

            spawnedVisual = visualInstance;

            VisualSpawned?.Invoke(spawnedVisual);
        }


        private void DestroySpawnedVisual()
        {
            if (spawnedVisual == null)
                return;

            VisualDespawned?.Invoke();

            GameObject visualObject = spawnedVisual.gameObject;
            spawnedVisual = null;

            Destroy(visualObject);
        }
    }
}