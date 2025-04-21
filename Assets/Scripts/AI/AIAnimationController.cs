using UnityEngine;
using System.Collections;

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
            Grab,
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
                case AnimationState.Grab:
                    animator.Play("Grab");
                    break;
                case AnimationState.Dead:
                    animator.Play("Dead");
                    break;
            }
        }

        public void PlayGrabAnimation()
        {
            StartCoroutine(PlayAnimationForDuration(AnimationState.Grab));
        }
        
        private IEnumerator PlayAnimationForDuration(AnimationState state)
        {
            string animationName = state.ToString();// please don't make spelling errors:)
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            // Wait for the full length of the animation to be played
            yield return new WaitForSeconds(stateInfo.length);
        }
    }
}
