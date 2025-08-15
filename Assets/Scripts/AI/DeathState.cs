using UnityEngine;
using AIAnimation;
/// <summary>
/// Stops an AI and plays death animation
/// </summary>
public class DeathState : IAIState
{
    private AIBase ai;
    private AIAnimationController animController;

    public DeathState(AIBase ai) => this.ai = ai;

    public void Enter()
    {
	    if (ai.agent != null && ai.agent.enabled) ai.agent.isStopped = true;
	    animController = ai.gameObject.GetComponentInChildren<AIAnimationController>();
    }

    public void Stay()
    {
        animController.SetAnimation(AIAnimationController.AnimationState.Dead);
    }

    public void Exit() => ai.agent.isStopped = false;
}
