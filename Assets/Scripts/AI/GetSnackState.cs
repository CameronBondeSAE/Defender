using AIAnimation;
using UnityEngine;

public class GetSnackState : IAIState
{
    private AIBase ai;
    private Transform snackTarget;
    private AIAnimationController animController;
    private SnackObject snackScript;
    private float lastBiteTime = 0f;
    private float eatingInterval = 1f;
    private bool snackGone = false;

    public GetSnackState(AIBase ai, Transform snackTarget)
    {
        this.ai = ai;
        this.snackTarget = snackTarget;
        snackScript = snackTarget.GetComponent<SnackObject>();
    }

    public void Enter()
    {
        animController = ai.agent.gameObject.GetComponentInChildren<AIAnimationController>();
        ai.ResumeMoving();
        lastBiteTime = 0f;
    }

    public void Stay()
    {
        if (snackGone)
        {
            // If walking civ, go back to patrol. If idle civ, go idle.
            if (ai is WalkingCivilianAI)
                ai.ChangeState(new PatrolState(ai));
            else
                ai.ChangeState(new IdleState(ai));
            return;
        }

        animController.SetAnimation(AIAnimationController.AnimationState.Walk);
        float dist = Vector3.Distance(ai.transform.position, snackTarget.position);
        if (dist > 1.2f)
        {
            ai.MoveTo(snackTarget.position);
        }
        else
        {
            animController.SetAnimation(AIAnimationController.AnimationState.Idle);

            // Eat!
            if (Time.time - lastBiteTime > eatingInterval)
            {
                lastBiteTime = Time.time;
                bool stillEdible = snackScript.TakeBite();
                if (!stillEdible)
                {
                    snackGone = true;
                }
            }
            ai.StopMoving();
        }
    }

    public void Exit()
    {
        ai.ResumeMoving();
    }
}
