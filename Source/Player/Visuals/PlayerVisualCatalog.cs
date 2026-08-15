using System;
using UnityEngine;

namespace InvadersOverboard.Player.Visuals
{
    [Serializable]
    public struct PlayerVisualEntry
    {
        [SerializeField]
        private PlayerVisualId id;

        [SerializeField]
        private GameObject prefab;


        public PlayerVisualId Id =>
            id;

        public GameObject Prefab =>
            prefab;
    }


    [CreateAssetMenu(
        fileName = "SO_PlayerVisualCatalog",
        menuName =
            "Invaders Overboard/Player/Visual Catalog")]
    public sealed class PlayerVisualCatalog :
        ScriptableObject
    {
        [SerializeField]
        private PlayerVisualEntry[] entries;


        public bool TryGetPrefab(
            PlayerVisualId visualId,
            out GameObject prefab)
        {
            if (entries != null)
            {
                for (int i = 0;
                     i < entries.Length;
                     i++)
                {
                    PlayerVisualEntry entry =
                        entries[i];

                    if (entry.Id != visualId)
                        continue;

                    prefab = entry.Prefab;

                    return prefab != null;
                }
            }

            prefab = null;

            return false;
        }


        public bool Contains(
            PlayerVisualId visualId)
        {
            return TryGetPrefab(
                visualId,
                out _);
        }
    }
}