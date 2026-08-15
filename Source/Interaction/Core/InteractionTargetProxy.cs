using UnityEngine;

namespace InvadersOverboard.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class InteractionTargetProxy :
        MonoBehaviour
    {
        [SerializeField]
        private MonoBehaviour targetBehaviour;


        public MonoBehaviour TargetBehaviour =>
            targetBehaviour;


        public bool TryGetTarget(
            out IInteractable target)
        {
            if (targetBehaviour == null ||
                targetBehaviour is not
                    IInteractable interactable)
            {
                target = null;
                return false;
            }

            target = interactable;
            return true;
        }


        private void Reset()
        {
            MonoBehaviour[] behaviours =
                GetComponentsInParent<
                    MonoBehaviour>(
                    true);

            foreach (MonoBehaviour behaviour
                     in behaviours)
            {
                if (behaviour is not IInteractable)
                    continue;

                targetBehaviour = behaviour;
                return;
            }
        }


        private void OnValidate()
        {
            if (targetBehaviour != null &&
                targetBehaviour is not IInteractable)
            {
                Debug.LogError(
                    $"{name}: Target Behaviour must implement " +
                    $"{nameof(IInteractable)}.",
                    this);
            }
        }
    }
}