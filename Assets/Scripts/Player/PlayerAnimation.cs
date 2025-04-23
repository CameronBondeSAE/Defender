using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        ReadyGun,
        Run,
        RunShoot,
        Death
    }

    private PlayerState currentState;
    public Animator animator;

    void Start()
    {
        SetAnimationState(PlayerState.Idle); 
    }

    public void SetAnimationState(PlayerState newState)
    {
        if (currentState == newState) return; 
        currentState = newState;
        HandleAnimation();
    }

    private void HandleAnimation()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
                animator.Play("Idle");
                break;
            // case PlayerState.ReadyGun:
            //     animator.Play("ReadyGun");
            //     break;
            case PlayerState.Run:
                animator.Play("Run");
                break;
            // case PlayerState.RunShoot:
            //     animator.Play("RunShoot");
            //     break;
            case PlayerState.Death:
                animator.Play("Death");
                break;
        }
    }
}
