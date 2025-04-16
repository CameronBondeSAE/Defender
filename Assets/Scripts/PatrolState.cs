using UnityEngine;

public class PatrolState : IAIState
{
    private CivilianAI ai;
    private int currentPoint = 0;

    public PatrolState(CivilianAI ai) => this.ai = ai;

    public void Enter() => ai.MoveTo(ai.patrolPoints[currentPoint].position);

    public void Stay()
    {
        if (Vector3.Distance(ai.transform.position, ai.patrolPoints[currentPoint].position) < 1f)
        {
            currentPoint = (currentPoint + 1) % ai.patrolPoints.Length;
            ai.MoveTo(ai.patrolPoints[currentPoint].position);
        }

        ai.FaceDirection();
    }

    public void Exit() { }
}
