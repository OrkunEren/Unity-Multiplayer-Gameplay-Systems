using InvadersOverboard.Water;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InvadersOverboard.Player.Swimming
{
    [DisallowMultipleComponent]
    public sealed class CharacterWaterDetector :
        MonoBehaviour
    {
        [Header("Optional Scene Reference")]

        [Tooltip(
            "Can be left unassigned on the prefab. " +
            "The provider in the active scene is resolved automatically.")]
        [SerializeField]
        private WaterSurfaceProvider waterProvider;


        public bool IsSwimming
        {
            get;
            private set;
        }

        public float SurfaceHeight
        {
            get;
            private set;
        }

        public float SubmersionDepth
        {
            get;
            private set;
        }


        private void OnEnable()
        {
            SceneManager.sceneLoaded +=
                HandleSceneLoaded;

            ResolveWaterProvider();
        }


        private void OnDisable()
        {
            SceneManager.sceneLoaded -=
                HandleSceneLoaded;

            ResetState();
        }


        public CharacterWaterInfo Sample(
            CharacterSwimmingSettings settings)
        {
            if (settings == null)
            {
                return CharacterWaterInfo.None;
            }

            if (waterProvider == null)
            {
                ResolveWaterProvider();
            }

            if (waterProvider == null ||
                !waterProvider.TrySample(
                    transform.position,
                    out WaterSample waterSample))
            {
                ResetState();

                return CharacterWaterInfo.None;
            }

            bool wasSwimming =
                IsSwimming;

            SurfaceHeight =
                waterSample.SurfaceHeight;

            SubmersionDepth =
                waterSample.GetSubmersionDepth(
                    transform.position);

            if (wasSwimming)
            {
                IsSwimming =
                    SubmersionDepth >
                    settings.ExitDepth;
            }
            else
            {
                IsSwimming =
                    SubmersionDepth >=
                    settings.EnterDepth;
            }

            bool enteredSwimming =
                !wasSwimming
                && IsSwimming;

            bool exitedSwimming =
                wasSwimming
                && !IsSwimming;

            return new CharacterWaterInfo(
                true,
                IsSwimming,
                enteredSwimming,
                exitedSwimming,
                SurfaceHeight,
                SubmersionDepth);
        }


        public void SetWaterProvider(
            WaterSurfaceProvider provider)
        {
            waterProvider =
                provider;

            ResetState();
        }


        private void ResolveWaterProvider()
        {
            if (waterProvider != null)
                return;

            waterProvider =
                FindFirstObjectByType<
                    WaterSurfaceProvider>();
        }


        private void HandleSceneLoaded(
            Scene scene,
            LoadSceneMode loadMode)
        {
            waterProvider = null;

            ResetState();

            ResolveWaterProvider();
        }


        private void ResetState()
        {
            IsSwimming = false;

            SurfaceHeight = 0f;

            SubmersionDepth = 0f;
        }
    }
}