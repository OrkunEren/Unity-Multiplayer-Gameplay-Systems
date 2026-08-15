using InvadersOverboard.Interaction;
using InvadersOverboard.Player.Interaction;
using TMPro;
using UnityEngine;

namespace InvadersOverboard.Presentation.Interaction
{
    [DisallowMultipleComponent]
    public sealed class InteractionPromptPresenter :
        MonoBehaviour
    {
        [Header("Dependencies")]

        [SerializeField]
        private CanvasGroup rootGroup;

        [SerializeField]
        private TMP_Text promptText;

        [SerializeField]
        private GameObject holdProgressRoot;

        [SerializeField]
        private RectTransform holdProgressFill;


        [Header("Formatting")]

        [SerializeField]
        private string instantFormat =
            "[E] {0}";

        [SerializeField]
        private string holdFormat =
            "[Hold E] {0}";

        [SerializeField]
        private Color availableColor =
            Color.white;

        [SerializeField]
        private Color unavailableColor =
            new(0.55f, 0.55f, 0.55f, 1f);


        private PlayerInteractor boundInteractor;

        private InteractionOffer currentOffer;


        public bool IsBoundTo(
            PlayerInteractor interactor)
        {
            return boundInteractor ==
                   interactor;
        }


        private void Awake()
        {
            if (rootGroup == null)
            {
                Debug.LogError(
                    $"{nameof(InteractionPromptPresenter)} " +
                    "requires a root CanvasGroup.",
                    this);
            }

            if (promptText == null)
            {
                Debug.LogError(
                    $"{nameof(InteractionPromptPresenter)} " +
                    "requires a prompt text.",
                    this);
            }

            Hide();
        }


        private void OnEnable()
        {
            RefreshFromBoundInteractor();
        }


        private void OnDestroy()
        {
            Unbind();
        }


        public void Bind(
            PlayerInteractor interactor)
        {
            if (boundInteractor == interactor)
            {
                RefreshFromBoundInteractor();
                return;
            }

            Unbind();

            boundInteractor = interactor;

            if (boundInteractor == null)
            {
                Hide();
                return;
            }

            boundInteractor.OfferChanged +=
                HandleOfferChanged;

            boundInteractor.FocusCleared +=
                HandleFocusCleared;

            boundInteractor.HoldProgressChanged +=
                HandleHoldProgressChanged;

            RefreshFromBoundInteractor();
        }


        public void Unbind()
        {
            if (boundInteractor != null)
            {
                boundInteractor.OfferChanged -=
                    HandleOfferChanged;

                boundInteractor.FocusCleared -=
                    HandleFocusCleared;

                boundInteractor.HoldProgressChanged -=
                    HandleHoldProgressChanged;
            }

            boundInteractor = null;

            currentOffer = default;

            Hide();
        }


        private void RefreshFromBoundInteractor()
        {
            if (!isActiveAndEnabled ||
                boundInteractor == null ||
                !boundInteractor.HasFocus)
            {
                Hide();
                return;
            }

            HandleOfferChanged(
                boundInteractor.CurrentOffer);

            HandleHoldProgressChanged(
                boundInteractor.HoldProgress);
        }


        private void HandleOfferChanged(
            InteractionOffer offer)
        {
            currentOffer = offer;

            if (string.IsNullOrWhiteSpace(
                    currentOffer.Prompt))
            {
                Hide();
                return;
            }

            string format =
                currentOffer.Mode ==
                InteractionMode.Hold
                    ? holdFormat
                    : instantFormat;

            promptText.text =
                string.Format(
                    format,
                    currentOffer.Prompt);

            promptText.color =
                currentOffer.CanInteract
                    ? availableColor
                    : unavailableColor;

            if (holdProgressRoot != null)
            {
                holdProgressRoot.SetActive(
                    currentOffer.CanInteract &&
                    currentOffer.Mode ==
                    InteractionMode.Hold);
            }

            SetVisible(true);
        }


        private void HandleFocusCleared()
        {
            currentOffer = default;

            Hide();
        }


        private void HandleHoldProgressChanged(
            float progress)
        {
            if (holdProgressFill == null)
                return;

            Vector3 scale =
                holdProgressFill.localScale;

            scale.x =
                Mathf.Clamp01(
                    progress);

            holdProgressFill.localScale =
                scale;
        }


        private void Hide()
        {
            if (holdProgressRoot != null)
            {
                holdProgressRoot.SetActive(false);
            }

            if (holdProgressFill != null)
            {
                Vector3 scale =
                    holdProgressFill.localScale;

                scale.x = 0f;

                holdProgressFill.localScale =
                    scale;
            }

            SetVisible(false);
        }


        private void SetVisible(
            bool visible)
        {
            if (rootGroup == null)
                return;

            rootGroup.alpha =
                visible ? 1f : 0f;

            rootGroup.interactable = false;
            rootGroup.blocksRaycasts = false;
        }
    }
}