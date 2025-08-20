using AIAnimation;
using UnityEngine;
using AIAnimation;

public class IdleState : IAIState
{
    private AIBase ai;
    private AIAnimationController animController;

    public IdleState(AIBase ai) => this.ai = ai;

    public void Enter()
    {
        ai.StopMoving();
        animController = ai.gameObject.GetComponentInChildren<AIAnimationController>();
    }

    public void Stay()
    {
        animController.SetAnimation(AIAnimationController.AnimationState.Idle);
    }

    public void Exit()
    {
	    if (ai.agent != null && ai.agent.enabled) ai.agent.isStopped = false;
    }
}
