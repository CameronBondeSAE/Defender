using UnityEngine;
using AIAnimation;

public class PatrolState : IAIState
{
    private AIBase ai;
    private int currentPoint = 0;
    private AIAnimationController animController;

    public PatrolState(AIBase ai) => this.ai = ai;

    public void Enter()
    {
        animController = ai.agent.gameObject.GetComponent<AIAnimationController>();
        ai.MoveTo(ai.patrolPoints[currentPoint].position);
    } 

    public void Stay()
    {
        animController.SetAnimation(AIAnimationController.AnimationState.Walk);
        Vector3 origin = ai.transform.position + Vector3.up * 1.2f;
        Vector3 direction = ai.transform.forward;
        Debug.DrawRay(origin, direction * currentPoint, Color.green, 10f);
        if (Vector3.Distance(ai.transform.position, ai.patrolPoints[currentPoint].position) < 1f)
        {
            currentPoint = (currentPoint + 1) % ai.patrolPoints.Length;
            ai.MoveTo(ai.patrolPoints[currentPoint].position);
        }
    }

    public void Exit() { }
}
