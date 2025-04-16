using UnityEngine;

public class DeathState : IAIState
{
    private AIBase ai;

    public DeathState(AIBase ai) => this.ai = ai;

    public void Enter() => ai.agent.isStopped = true;

    public void Stay() { }

    public void Exit() => ai.agent.isStopped = false;
}
