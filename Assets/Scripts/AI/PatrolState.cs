using UnityEngine;
using AIAnimation; 

/// <summary>
/// Ais who enter this state will move from point to point in a loop.
/// </summary>
public class PatrolState : IAIState
{
    private AIBase ai;
    private int currentPoint = 0;
    private AIAnimationController animController;

    public PatrolState(AIBase ai) => this.ai = ai;

    public void Enter()
    {
        animController = ai.gameObject.GetComponent<AIAnimationController>() ?? 
                         ai.gameObject.GetComponentInChildren<AIAnimationController>() ?? 
                         ai.gameObject.GetComponentInParent<AIAnimationController>();
        if (animController != null)
        {
            animController.SetAnimation(AIAnimationController.AnimationState.Walk);
        }
        ai.ResumeMoving();
        ai.MoveTo(ai.patrolPoints[currentPoint].position);
    } 

    public void Stay()
    {
        // animController.SetAnimation(AIAnimationController.AnimationState.Walk);
        // if (Vector3.Distance(ai.transform.position, ai.patrolPoints[currentPoint].position) < ai.cornerThreshold)
        // {
	       //  Debug.Log($"[PatrolState] {ai.name} has reached its destination");
        //     currentPoint = (currentPoint + 1) % ai.patrolPoints.Length;
        //     ai.MoveTo(ai.patrolPoints[currentPoint].position);
        // }
        // check if reached the destination
        // bool hasReachedDestination = false;
    
        if (ai.useRigidbody)
        {
            // for rb: check if path is null
            // hasReachedDestination = (ai.path == null || ai.cornerIndex >= ai.path.corners.Length);
            if (ai.path == null || ai.cornerIndex >= ai.path.corners.Length)
            {
                Debug.Log($"[PatrolState] {ai.name} has reached waypoint {currentPoint}");
                currentPoint = (currentPoint + 1) % ai.patrolPoints.Length;
                ai.MoveTo(ai.patrolPoints[currentPoint].position);
            }
        }
        else
        {
            // for NavmeshAI: use old distance checks
            // hasReachedDestination = Vector3.Distance(ai.transform.position, ai.patrolPoints[currentPoint].position) < ai.cornerThreshold;
            if (Vector3.Distance(ai.transform.position, ai.patrolPoints[currentPoint].position) < ai.cornerThreshold)
            {
                Debug.Log($"[PatrolState] {ai.name} has reached its destination");
                currentPoint = (currentPoint + 1) % ai.patrolPoints.Length;
                ai.MoveTo(ai.patrolPoints[currentPoint].position);
            }
        }
    
        // if (hasReachedDestination)
        // {
        //     Debug.Log($"[PatrolState] {ai.name} has reached its destination");
        //     currentPoint = (currentPoint + 1) % ai.patrolPoints.Length;
        //     ai.MoveTo(ai.patrolPoints[currentPoint].position);
        // }
    }

    public void Exit() { }
}
