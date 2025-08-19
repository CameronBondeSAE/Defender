using AIAnimation;
using UnityEngine;

public class GetSnackState : IAIState
{
    private AIBase ai;
    private Transform snackTarget;
    private AIAnimationController animController;
    private SnackObject snackObject;
    private SnackHealth snackHealth;
    
    private float lastBiteTime = 0f;
    private float eatingInterval = 1f;
    private bool snackGone = false;

    public GetSnackState(AIBase ai, Transform snackTarget)
    {
        this.ai = ai;
        this.snackTarget = snackTarget;

        if (snackTarget != null)
        {
            snackObject = snackTarget.GetComponent<SnackObject>();
            snackHealth = snackTarget.GetComponent<SnackHealth>();
            if (snackHealth != null && snackObject != null)
                snackHealth = snackObject.snackHealth;
        }
    }

    public void Enter()
    {
        if (ai == null) return;
        animController = ai.gameObject.GetComponentInChildren<AIAnimationController>();
        ai.ResumeMoving();
        lastBiteTime = 0f;

        if (snackTarget == null || snackObject == null || snackHealth == null || snackHealth.isDead)
        {
            snackGone = true;
            ReturnToDefaultBehaviour();
            return;
        }
        snackHealth.OnDeath += HandleSnackDeath;
    }

    public void Stay()
    {
        if (snackGone || snackTarget == null || snackObject == null || snackHealth == null || snackHealth.isDead)
        {
            snackGone = true;
            ReturnToDefaultBehaviour();
            return;
        }
        animController.SetAnimation(AIAnimationController.AnimationState.Walk);
        float dist = Vector3.Distance(ai.transform.position, snackTarget.position);
        if (dist > 1.2f)
        {
            ai.MoveTo(snackTarget.position);
            return;
        }
        animController.SetAnimation(AIAnimationController.AnimationState.Idle);
            // Eat!
            if (Time.time - lastBiteTime > eatingInterval)
            {
                lastBiteTime = Time.time;
                bool stillEdible = snackObject.TakeBite();
                 if (!stillEdible)
                 {
                     snackGone = true;
                     ReturnToDefaultBehaviour();
                     return;
                 }
            }
            ai.StopMoving();
    }

    public void Exit()
    {
        snackHealth.OnDeath -= HandleSnackDeath;
        ai.ResumeMoving();
    }

    private void HandleSnackDeath()
    {
        snackGone = true;
        ReturnToDefaultBehaviour();
    }

    private void ReturnToDefaultBehaviour()
    {
        snackHealth.OnDeath -= HandleSnackDeath;
        if (ai is WalkingCivilianAI)
        {
            ai.ChangeState(new PatrolState(ai));
        }
        else
        {
            ai.ChangeState(new IdleState(ai));
        }
    }

}
