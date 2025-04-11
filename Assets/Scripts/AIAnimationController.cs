using UnityEngine;

namespace AIAnimation
{
    public class AIAnimationController : MonoBehaviour
    {
        public enum AnimationState
        {
            Idle,
            Walk,
            GetHit,
            Attack,
            Dead
        }

        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void SetAnimation(AnimationState state)
        {
            if (animator == null) return;

            switch (state)
            {
                case AnimationState.Idle:
                    animator.Play("Idle");
                    break;
                case AnimationState.Walk:
                    animator.Play("Walk");
                    break;
                case AnimationState.GetHit:
                    animator.Play("GetHit");
                    break;
                case AnimationState.Attack:
                    animator.Play("Attack");
                    break;
                case AnimationState.Dead:
                    animator.Play("Dead");
                    break;
            }
        }
    }
}
