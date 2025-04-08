using UnityEngine;

public class FollowState : IAIState
{
    private AIBase ai;
    private Transform target;
    public FollowState(AIBase ai, Transform target)
    {
        // Stores this AI and its target to this state
        this.ai = ai;
        this.target = target;
    }

    public void Enter()
    {
        
    }
    public void Stay()
    {
        if (target == null) return;
        ai.MoveTo(target.position);
        ai.FaceDirection();
    }
    public void Exit()
    {
        
    }
}
