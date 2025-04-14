using UnityEngine;
using UnityEngine.AI;

public class FollowState : IAIState
{
    private AIBase ai;
    private Transform target;
    private float followDistance = 5f;
    private float reachThreshold = 0.2f; // determines 'reached'
    private bool targetRestored = false; // Tracks if the obstacle/agent has been toggled back
    
    private NavMeshObstacle tempObstacle; // Temporary NavMeshObstacle to prevent Civ walking into the target/block their way
    private NavMeshAgent targetAgent;

    public FollowState(AIBase ai, Transform target)
    {
        // Stores this AI and its target to this state
        this.ai = ai;
        this.target = target;
    }

    public void Enter()
    {
        targetRestored = false;
        // Try to get an existing NavMeshObstacle from the target
        tempObstacle = target.GetComponent<NavMeshObstacle>();
        if (tempObstacle == null)
        {
            // If not found, add one to the target
            tempObstacle = target.gameObject.AddComponent<NavMeshObstacle>();
            tempObstacle.carving = true; // enable the target to make a cutout on the navmesh
        }
        else
        {
            tempObstacle.enabled = true;
        }
        targetAgent = target.GetComponent<NavMeshAgent>();
        if (targetAgent != null && targetAgent.enabled)
        {
            targetAgent.enabled = false;
        }
    }
    public void Stay()
    {
        if (target == null) return;

        // Follow position calculation
        Vector3 offsetDirection = -target.forward.normalized;
        Vector3 followPos = target.position + offsetDirection * followDistance;

        ai.MoveTo(followPos);
        ai.FaceDirection();

        // Check if civ has reached follow position
        float distanceToFollowPos = Vector3.Distance(ai.transform.position, followPos);
        if (!targetRestored && distanceToFollowPos <= reachThreshold)
        {
            // Restore target NavMeshAgent and disable its obstacle
            if (targetAgent != null)
            {
                targetAgent.enabled = true;
            }
            if (tempObstacle != null)
            {
                tempObstacle.enabled = false;
            }
            targetRestored = true;
        }

    }
    public void Exit()
    {
       
    }
}
