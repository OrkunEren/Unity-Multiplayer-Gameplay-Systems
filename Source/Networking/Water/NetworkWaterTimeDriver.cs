using System.Collections.Generic;
using InvadersOverboard.Water;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

namespace InvadersOverboard.Networking.Water
{
    [DefaultExecutionOrder(-10000)]
    [DisallowMultipleComponent]
    public sealed class NetworkWaterTimeDriver :
        MonoBehaviour
    {
        [Header("Dependencies")]

        [SerializeField]
        private WaterSurfaceProvider waterProvider;


        [Header("Time")]

        [Tooltip(
            "Keeps the SW3 custom time value positive by applying " +
            "an initial offset.")]
        [SerializeField, Min(1f)]
        private float timeOffset = 1f;


        private void Awake()
        {
            if (waterProvider == null)
            {
                Debug.LogError(
                    $"{nameof(NetworkWaterTimeDriver)} " +
                    "requires a water provider.",
                    this);

                enabled = false;
            }
        }


        private void OnEnable()
        {
            RenderPipelineManager.beginContextRendering +=
                HandleBeginContextRendering;

            ApplyWaterTime();
        }


        private void Update()
        {
            // Before player water queries.
            ApplyWaterTime();
        }


        private void FixedUpdate()
        {
            // Before ShipBuoyancy queries.
            ApplyWaterTime();
        }


        private void HandleBeginContextRendering(
            ScriptableRenderContext context,
            List<Camera> cameras)
        {
            // Immediately before the water shader is rendered.
            ApplyWaterTime();
        }


        private void ApplyWaterTime()
        {
            if (waterProvider == null)
                return;

            double synchronizedTime =
                GetSynchronizedTime();

            waterProvider.SetSimulationTime(
                (float)(
                    synchronizedTime
                    + timeOffset));
        }


        private static double GetSynchronizedTime()
        {
            NetworkManager networkManager =
                NetworkManager.Singleton;

            if (networkManager != null &&
                networkManager.IsListening)
            {
                return networkManager
                    .ServerTime
                    .Time;
            }

            return Time.unscaledTimeAsDouble;
        }


        private void OnDisable()
        {
            RenderPipelineManager.beginContextRendering -=
                HandleBeginContextRendering;

            if (waterProvider != null)
            {
                waterProvider.ResetSimulationTime();
            }
        }
    }
}