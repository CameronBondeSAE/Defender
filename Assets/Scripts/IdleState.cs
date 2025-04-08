using UnityEngine;

public class IdleState : IAIState
{
    private AIBase ai;
    
    public IdleState(AIBase ai)
    {
        this.ai = ai;
    }
    public void Enter()
    {
        ai.agent.isStopped = true;
    }

    public void Stay()
    {
        
    }

    public void Exit()
    {
        ai.agent.isStopped = false;
    }
}
