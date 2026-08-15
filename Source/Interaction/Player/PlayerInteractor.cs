using System;
using InvadersOverboard.Interaction;
using InvadersOverboard.Player.Input;
using UnityEngine;

namespace InvadersOverboard.Player.Interaction
{
    [DefaultExecutionOrder(50)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PlayerInteractor :
        MonoBehaviour
    {
        [Header("Dependencies")]

        [SerializeField]
        private PlayerInteractionSettings settings;

        [SerializeField]
        private Transform interactionOrigin;


        private PlayerInputReader inputReader;

        private MonoBehaviour focusedBehaviour;

        private IInteractable focusedTarget;

        private InteractionContext focusedContext;

        private InteractionOffer focusedOffer;


        private bool isHolding;

        private bool waitForInteractRelease;

        private float holdElapsed;

        private float holdProgress;


        public bool HasFocus =>
            focusedBehaviour != null &&
            focusedTarget != null;

        public InteractionOffer CurrentOffer =>
            focusedOffer;

        public float HoldProgress =>
            holdProgress;


        public event Action<InteractionOffer>
            OfferChanged;

        public event Action FocusCleared;

        public event Action<float>
            HoldProgressChanged;


        private void Awake()
        {
            inputReader =
                GetComponent<PlayerInputReader>();

            if (settings == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerInteractor)} " +
                    "requires interaction settings.",
                    this);
            }

            if (interactionOrigin == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerInteractor)} " +
                    "requires an interaction origin.",
                    this);
            }
        }


        private void Update()
        {
            bool interactPressed =
                inputReader.ConsumeInteractRequest();

            if (!inputReader.IsInputEnabled ||
                settings == null ||
                interactionOrigin == null)
            {
                ClearFocus();
                return;
            }

            UpdateFocus();

            ProcessInteraction(
                interactPressed,
                Time.deltaTime);
        }


        private void OnDisable()
        {
            ClearFocus();

            waitForInteractRelease = false;
        }


        private void UpdateFocus()
        {
            Ray ray =
                new(
                    interactionOrigin.position,
                    interactionOrigin.forward);

            bool hasHit;

            RaycastHit hit;


            if (settings.SphereRadius > 0f)
            {
                hasHit =
                    Physics.SphereCast(
                        ray,
                        settings.SphereRadius,
                        out hit,
                        settings.MaximumDistance,
                        settings.CastMask,
                        settings.TriggerInteraction);
            }
            else
            {
                hasHit =
                    Physics.Raycast(
                        ray,
                        out hit,
                        settings.MaximumDistance,
                        settings.CastMask,
                        settings.TriggerInteraction);
            }


            if (!hasHit ||
                hit.collider == null ||
                hit.collider.transform.IsChildOf(
                    transform) ||
                !settings.IsInteractableLayer(
                    hit.collider.gameObject.layer))
            {
                ClearFocus();
                return;
            }


            InteractionTargetProxy proxy =
                hit.collider.GetComponentInParent<
                    InteractionTargetProxy>();

            if (proxy == null ||
                !proxy.TryGetTarget(
                    out IInteractable target))
            {
                ClearFocus();
                return;
            }


            MonoBehaviour targetBehaviour =
                proxy.TargetBehaviour;

            if (targetBehaviour == null ||
                !targetBehaviour.isActiveAndEnabled)
            {
                ClearFocus();
                return;
            }


            InteractionContext context =
                new(
                    gameObject,
                    interactionOrigin,
                    hit.collider,
                    hit.point,
                    hit.normal,
                    hit.distance);

            InteractionOffer offer =
                target.GetInteractionOffer(
                    context);

            SetFocus(
                targetBehaviour,
                target,
                context,
                offer);
        }


        private void SetFocus(
            MonoBehaviour targetBehaviour,
            IInteractable target,
            in InteractionContext context,
            in InteractionOffer offer)
        {
            bool targetChanged =
                focusedBehaviour !=
                targetBehaviour;


            if (targetChanged)
            {
                NotifyFocusExited();

                ResetHold();

                focusedBehaviour =
                    targetBehaviour;

                focusedTarget =
                    target;

                focusedContext =
                    context;

                focusedOffer =
                    offer;

                NotifyFocusEntered();

                OfferChanged?.Invoke(
                    focusedOffer);

                return;
            }


            focusedContext =
                context;

            if (!OffersEqual(
                    focusedOffer,
                    offer))
            {
                focusedOffer =
                    offer;

                if (!focusedOffer.CanInteract)
                {
                    ResetHold();
                }

                OfferChanged?.Invoke(
                    focusedOffer);
            }
        }


        private void ProcessInteraction(
            bool interactPressed,
            float deltaTime)
        {
            if (!HasFocus)
            {
                ResetHold();
                return;
            }


            if (focusedOffer.Mode ==
                InteractionMode.Instant)
            {
                ResetHold();

                if (interactPressed &&
                    focusedOffer.CanInteract)
                {
                    focusedTarget.TryInteract(
                        focusedContext);
                }

                return;
            }


            ProcessHoldInteraction(
                interactPressed,
                deltaTime);
        }


        private void ProcessHoldInteraction(
            bool interactPressed,
            float deltaTime)
        {
            if (waitForInteractRelease)
            {
                if (!inputReader.IsInteractHeld)
                {
                    waitForInteractRelease =
                        false;
                }

                return;
            }


            if (!focusedOffer.CanInteract)
            {
                ResetHold();
                return;
            }


            if (interactPressed)
            {
                isHolding = true;
                holdElapsed = 0f;

                SetHoldProgress(0f);
            }


            if (!isHolding)
                return;


            if (!inputReader.IsInteractHeld)
            {
                ResetHold();
                return;
            }


            holdElapsed +=
                deltaTime;

            float duration =
                Mathf.Max(
                    0.05f,
                    focusedOffer.HoldDuration);

            SetHoldProgress(
                Mathf.Clamp01(
                    holdElapsed / duration));


            if (holdElapsed < duration)
                return;


            focusedTarget.TryInteract(
                focusedContext);

            isHolding = false;
            holdElapsed = 0f;

            waitForInteractRelease = true;

            SetHoldProgress(0f);
        }


        private void ClearFocus()
        {
            if (!HasFocus)
            {
                ResetHold();
                return;
            }

            NotifyFocusExited();

            focusedBehaviour = null;
            focusedTarget = null;

            focusedContext = default;
            focusedOffer = default;

            ResetHold();

            FocusCleared?.Invoke();
        }


        private void NotifyFocusEntered()
        {
            if (focusedBehaviour is
                IInteractionFocusReceiver receiver)
            {
                receiver.OnInteractionFocusEntered(
                    focusedContext);
            }
        }


        private void NotifyFocusExited()
        {
            if (focusedBehaviour is
                IInteractionFocusReceiver receiver)
            {
                receiver.OnInteractionFocusExited(
                    focusedContext);
            }
        }


        private void ResetHold()
        {
            isHolding = false;
            holdElapsed = 0f;

            SetHoldProgress(0f);
        }


        private void SetHoldProgress(
            float progress)
        {
            progress =
                Mathf.Clamp01(
                    progress);

            if (Mathf.Approximately(
                    holdProgress,
                    progress))
            {
                return;
            }

            holdProgress = progress;

            HoldProgressChanged?.Invoke(
                holdProgress);
        }


        private static bool OffersEqual(
            in InteractionOffer left,
            in InteractionOffer right)
        {
            return
                left.CanInteract ==
                right.CanInteract
                && string.Equals(
                    left.Prompt,
                    right.Prompt,
                    StringComparison.Ordinal)
                && left.Mode ==
                right.Mode
                && Mathf.Approximately(
                    left.HoldDuration,
                    right.HoldDuration)
                && left.Priority ==
                right.Priority;
        }
    }
}
