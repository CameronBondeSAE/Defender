using UnityEngine;
using UnityEngine.AI;

public class FollowState : IAIState
{
    private AIBase ai;
    private Transform target;
    private float followDistance = 2f;
    private float positionUpdateInterval = 1f; // How often the follow position updates
    private float timeSinceLastUpdate = 0f;

    private NavMeshObstacle tempObstacle; // Temporary NavMeshObstacle to prevent Civ walking into the target/block their way

    public FollowState(AIBase ai, Transform target)
    {
        // Stores this AI and its target to this state
        this.ai = ai;
        this.target = target;
    }

    public void Enter()
    {
        timeSinceLastUpdate = 0f; // Reset update timer when enter state
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
    }
    public void Stay()
    {
        if (target == null) return;
        // Only update the follow position every positionUpdateInterval seconds
        if (timeSinceLastUpdate < positionUpdateInterval) return;
        timeSinceLastUpdate = 0f; // timer reset
        // Get the direction behind the target (negative of its forward)
        Vector3 offsetDirection = -target.forward.normalized;
        // Calculate the follow position using the offset
        Vector3 followPos = target.position + offsetDirection * followDistance;
        ai.MoveTo(followPos);
        ai.FaceDirection();
    }
    public void Exit()
    {
        // Clean up the NavMeshObstacle on the target
        if (tempObstacle != null)
        {
            tempObstacle.enabled = false;
        }
    }
}
