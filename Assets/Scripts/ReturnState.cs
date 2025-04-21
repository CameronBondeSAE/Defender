using UnityEngine;
using AIAnimation;

public class ReturnState : IAIState
{
    private AlienAI ai;

    public ReturnState(AlienAI ai) => this.ai = ai;
    private AIAnimationController animController;

    public void Enter()
    {
        animController = ai.agent.gameObject.GetComponent<AIAnimationController>();
        if (ai.mothership != null)
            ai.MoveTo(ai.mothership.position);
    }

    public void Stay()
    {
        animController.SetAnimation(AIAnimationController.AnimationState.Walk);
        if (ai.mothership == null) return;

        float dist = Vector3.Distance(ai.transform.position, ai.mothership.position);
        if (dist < 1f)
        {
            if (ai.currentTargetCiv != null)
                ai.currentTargetCiv.ChangeState(new IdleState(ai.currentTargetCiv));

            ai.currentTargetCiv = null;
            ai.ChangeState(new SearchState(ai));
        }

        ai.FaceDirection();
    }

    public void Exit() { }
}