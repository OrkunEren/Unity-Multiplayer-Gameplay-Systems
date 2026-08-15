using UnityEngine;

namespace InvadersOverboard.Player.Visuals
{
    [DisallowMultipleComponent]
    public sealed class PlayerVisualInstance : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        public Animator Animator => animator;

        private void Reset()
        {
            animator = GetComponentInChildren<Animator>(true);
        }

        private void Awake()
        {
            if (animator == null)
            {
                Debug.LogError(
                    $"{name}: Animator is not assigned on PlayerVisualInstance.",
                    this);

                enabled = false;
                return;
            }

            // CharacterMotor owns movement.
            animator.applyRootMotion = false;
        }

        public Transform GetBone(HumanBodyBones bone)
        {
            if (animator == null || !animator.isHuman)
                return null;

            return animator.GetBoneTransform(bone);
        }
    }
}