using UnityEngine;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;

namespace AIAnimation // A name space shared only by AIs for clarity
{
    /// <summary>
    /// Same structure as player animation which handles animations shared by all AIs.
    /// Now networked and is server authoritative: server plays states, NetAnim syncs them to clients
    /// </summary>
    public class AIAnimationController : NetworkBehaviour
    {
        public enum AnimationState
        {
            Idle,
            Walk,
            GetHit,
            Attack,
            Grab,
            Dead,
            GettingSucked
        }
        private Animator animator;
        private AnimationState currentState;
        [SerializeField] private NetworkAnimator networkAnimator;

        private void Awake()
        {
            // search order: self > children > parent
            // since our ai set up is all different
            animator = GetComponent<Animator>() ?? 
                       GetComponentInChildren<Animator>() ?? 
                       GetComponentInParent<Animator>();

            networkAnimator = GetComponent<NetworkAnimator>() ??
                              GetComponentInChildren<NetworkAnimator>() ??
                              GetComponentInParent<NetworkAnimator> ();
    
            if (animator == null)
            {
                Debug.LogError($"[AIAnimationController] No Animator found on {name}!");
            }
            else
            {
                Debug.Log($"[AIAnimationController] Found Animator on {animator.gameObject.name}");
            }
        }
        public void SetAnimationStateServer(AnimationState newState) // server only
        {
            if (currentState == newState) return;
            currentState = newState;
            HandleAnimation();
        }
        public void SetAnimation(AnimationState state)
        {
            if (IsServer)
            {
                SetAnimationStateServer(state);
            }
            else if (IsOwner) // Add this check
            {
                // If this client owns the object, request server to set animation
                RequestSetStateServerRpc(state);
            }
        }
        
        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void RequestSetStateServerRpc(AnimationState newState)
        {
            SetAnimationStateServer(newState);
        }
        // [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
        // private void RequestSetStateClientRpc(AnimationState newState)
        // {
        //     SetAnimationStateServer(newState);
        // }
        public void HandleAnimation()
        {
            if (!IsServer || animator == null) return;
            string animationName = currentState.ToString();
            animator.Play(animationName, 0, 0f);
            PlayAnimationClientRpc(animationName);
            // switch (currentState)
            // {
            //     case AnimationState.Idle:
            //         animator.Play("Idle");
            //         break;
            //     case AnimationState.Walk:
            //         animator.Play("Walk");
            //         break;
            //     case AnimationState.GetHit:
            //         animator.Play("GetHit");
            //         break;
            //     case AnimationState.Attack:
            //         animator.Play("Attack");
            //         break;
            //     case AnimationState.Grab:
            //         animator.Play("Grab");
            //         break;
            //     case AnimationState.Dead:
            //         animator.Play("Dead");
            //         break;
            //     case AnimationState.GettingSucked:
            //         animator.Play("GettingSucked");
            //         break;
            // }
        }
        [Rpc(SendTo.NotServer, Delivery = RpcDelivery.Reliable)]
        private void PlayAnimationClientRpc(string animationName)
        {
            if(IsServer) return;
            if(!animator) animator = GetComponentInChildren<Animator>();
            animator.Play(animationName, 0, 0f);
        }
        public void PlayGrabAnimation()
        {
            if(!IsServer) return;
            StartCoroutine(PlayAnimationForDuration(AnimationState.Grab));
        }
        private IEnumerator PlayAnimationForDuration(AnimationState state)
        {
            SetAnimationStateServer(state);
            string animationName = state.ToString();// please don't make spelling errors:)
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            // Wait for the full length of the animation to be played
            yield return new WaitForSeconds(stateInfo.length);
        }
    }
}
