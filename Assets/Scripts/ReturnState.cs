using UnityEngine;

public class ReturnState : IAIState
{
    private AlienAI ai;

    public ReturnState(AlienAI ai) => this.ai = ai;

    public void Enter()
    {
        if (ai.mothership != null)
            ai.MoveTo(ai.mothership.position);
    }

    public void Stay()
    {
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