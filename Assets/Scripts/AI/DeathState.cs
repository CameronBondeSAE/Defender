using UnityEngine;
using AIAnimation;

public class DeathState : IAIState
{
    private AIBase ai;
    private AIAnimationController animController;

    public DeathState(AIBase ai) => this.ai = ai;

    public void Enter()
    {
        ai.agent.isStopped = true;
        animController = ai.agent.gameObject.GetComponentInChildren<AIAnimationController>();
    }

    public void Stay()
    {
        animController.SetAnimation(AIAnimationController.AnimationState.Dead);
    }

    public void Exit() => ai.agent.isStopped = false;
}
