using UnityEngine;

public class PatrolState : IAIState
{
    private CivAI ai;
    private int currentPatrolIndex = 0;
    private Vector3[] patrolPoints;
    public void Enter()
    {
        ai.agent.isStopped = false; // resumes movement
        ai.agent.SetDestination(ai.patrolPoints[currentPatrolIndex]);
    }
    
    public PatrolState(CivAI ai)
    {
        this.ai = ai;
    }

    public void Stay()
    {
        // If close enough to the current patrol point, go to the next one
        if (Vector3.Distance(ai.transform.position, ai.patrolPoints[currentPatrolIndex]) < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length; // // Cycle through points
            ai.agent.SetDestination(patrolPoints[currentPatrolIndex]); // go to next point
        }
        ai.FaceDirection();
    }

    public void Exit()
    {
        // informs navmesh of stop
        ai.agent.isStopped = true;
    }

    // stores the AI reference to this state
}
