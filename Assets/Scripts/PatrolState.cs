using UnityEngine;

public class PatrolState : IAIState
{
    private AIBase ai;
    private int currentPatrolIndex = 0;
    private Transform[] patrolPoints;
    public void Enter()
    {
        ai.agent.isStopped = false; // resumes movement
        patrolPoints = ai.patrolPoints;
        ai.MoveTo(ai.patrolPoints[currentPatrolIndex].position);
    }
    
    public PatrolState(CivAI ai)
    {
        this.ai = ai;
    }

    public void Stay()
    {
        // If close enough to the current patrol point, go to the next one
        if (Vector3.Distance(ai.transform.position, ai.patrolPoints[currentPatrolIndex].position) < 3f)
        {
            Debug.Log("ran");
            currentPatrolIndex = (currentPatrolIndex + 1) % ai.patrolPoints.Length; // // Cycle through points
            ai.MoveTo(ai.patrolPoints[currentPatrolIndex].position); // go to next point
        }
        ai.FaceDirection();
    }

    public void Exit()
    {
        // informs navmesh of stop
        ai.agent.isStopped = true;
    }

}
