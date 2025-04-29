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
        animController = ai.agent.gameObject.GetComponent<AIAnimationController>();
    }

    public void Stay()
    {
        animController.SetAnimation(AIAnimationController.AnimationState.Idle);
    }

    public void Exit() => ai.agent.isStopped = false;
}
