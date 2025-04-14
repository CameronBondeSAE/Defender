using UnityEngine;
using UnityEngine.AI;

public class FollowState : IAIState
{
    private AIBase ai;
    private Transform target;
    private float followDistance = 2f;
    private float reachThreshold = .5f; // determines 'reached'
    private bool targetRestored = false; // Tracks if the obstacle/agent has been toggled back
    private float updateFollowPosThreshold = 1f; 
    private Vector3 currentFollowPos;
    
   // private NavMeshObstacle tempObstacle; // Temporary NavMeshObstacle to prevent Civ walking into the target/block their way
    //private NavMeshAgent targetAgent;

    public FollowState(AIBase ai, Transform target)
    {
        Debug.Log("Entering FollowState");
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
        Debug.Log($"Remaining agent distance: {ai.agent.remainingDistance}, PathPending: {ai.agent.pathPending}, HasPath: {ai.agent.hasPath}");
        UpdateFollowPos();
        // Calculate desired follow pos
        ai.MoveTo(currentFollowPos);
        ai.FaceDirection();
    }
    public void Exit()
    {
       
    }
    private void UpdateFollowPos()
    {
        currentFollowPos = target.position - target.forward.normalized * followDistance;
    }
}
