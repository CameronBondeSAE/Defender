using UnityEngine;
using AIAnimation;
using System.Collections;

public class HitState : MonoBehaviour, IAIState
{
    private AIBase ai;
    private AIAnimationController animController;
    private IAIState previousState;

    public HitState(AIBase ai, IAIState previousState)
    {
        this.ai = ai;
        this.previousState = previousState;
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
        yield return new WaitForSeconds(2f);
        ai.ChangeState(previousState);
    }
}
