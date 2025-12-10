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
        if(ai.patrolPoints.Length > 0)
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
            if (ai.patrolPoints == null || ai.patrolPoints.Length == 0)
            {
                return;
            }

            currentPoint = (currentPoint + 1) % ai.patrolPoints.Length;
            ai.MoveTo(ai.patrolPoints[currentPoint].position);
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
        
        if (animController != null)
        {
            bool isMoving = false;

            if (ai.useRigidbody && ai.rb != null)
            {
                // if rb
                isMoving = ai.rb.linearVelocity.sqrMagnitude > 0.05f;
            }
            else if (ai.Agent != null && ai.Agent.enabled)
            {
                // if navmesh
                isMoving = ai.Agent.velocity.sqrMagnitude > 0.05f;
            }

            animController.SetAnimation(
                isMoving
                    ? AIAnimationController.AnimationState.Walk
                    : AIAnimationController.AnimationState.Idle
            );
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
