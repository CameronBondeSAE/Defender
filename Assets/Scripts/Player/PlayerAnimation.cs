using Unity.Netcode;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class PlayerAnimation : NetworkBehaviour
{
    public enum PlayerState
    {
        Idle,
        Run,
        Death
    }
    private PlayerState currentState;
    public Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator; // must assign in inspector!
    void Start()
    {
        //if (IsOwner) // owner asks, server sets
       //RequestSetStateClientRpc(PlayerState.Idle);
       SetAnimationStateServer(PlayerState.Idle);
    }

    /// <summary>
    /// Call this function from other scripts to set animation on the OWNER ONLYYY
    /// </summary>
    /// <param name="newState"></param>
    public void RequestState(PlayerState newState)
    {
        if (!IsOwner) return;
        //RequestSetStateClientRpc(newState);
        SetAnimationStateServer(newState);
    }
    // [Rpc(SendTo.Server)] // client to server
    // private void RequestSetStateClientRpc(PlayerState newState)
    // {
    //     SetAnimationStateServer(newState);
    // }
    public void SetAnimationStateServer(PlayerState newState)
    {
        if (currentState == newState) return; 
        currentState = newState;
        HandleAnimation(); // runs on server
    }

    private void HandleAnimation()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
                animator.Play("Idle");
                break;
            case PlayerState.Run:
                animator.Play("Run");
                break;
            case PlayerState.Death:
                animator.Play("Death");
                break;
        }
    }
}
