using UnityEngine;
using AIAnimation;
using System.Collections;
/// <summary>
/// Pauses the AI to play the hit animation when they are hit, and return them to their previous state when the animation finishes playing
/// </summary>
public class HitState : MonoBehaviour, IAIState
{
    private AIBase ai;
    private AIAnimationController animController;
    private IAIState previousState;

    public HitState(AIBase ai, IAIState previousState)
    {
        this.ai = ai;
        this.previousState = previousState; // Stores the previous state
    }

    public void Enter()
    {
        animController = ai.gameObject.GetComponentInChildren<AIAnimationController>();
        ai.StopMoving();
        animController.SetAnimation(AIAnimationController.AnimationState.GetHit);
        ai.StartCoroutine(ReturnToPreviousState());
    }

    public void Stay()
    {
        
    }

    public void Exit()
    {
        ai.ResumeMoving();
    }

    private IEnumerator ReturnToPreviousState()
    {
        yield return new WaitForSeconds(3f);
        ai.ChangeState(previousState);
    }
}
