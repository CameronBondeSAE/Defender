using UnityEngine;
using UnityEngine.AI;
using Anthill.AI;
using AIAnimation;

public class DodgeExplosive : AntAIState
{
    private DangerousAlienControl control;
    private NavMeshAgent navMeshAgent;
    private AIAnimationController animationController;

    private float originalSpeed;

    public override void Create(GameObject aGameObject)
    {
        control            = aGameObject.GetComponent<DangerousAlienControl>();
        navMeshAgent       = aGameObject.GetComponent<NavMeshAgent>();
        animationController = aGameObject.GetComponentInChildren<AIAnimationController>();
    }

    public override void Enter()
    {
        if (control == null || navMeshAgent == null || !navMeshAgent.enabled)
        {
            Finish();
            return;
        }

        if (control.currentExplosiveThreat == null || !control.hasDodgeTarget)
        {
            Finish();
            return;
        }

        originalSpeed = navMeshAgent.speed;
        navMeshAgent.speed = control.dodgeMoveSpeed;
        navMeshAgent.isStopped = false;

        navMeshAgent.SetDestination(control.currentDodgeTarget);
        control.isDodging = true;

        if (animationController != null)
        {
            animationController.SetAnimation(AIAnimationController.AnimationState.Walk);
        }
    }

    public override void Execute(float aDeltaTime, float aTimeScale)
    {
        if (control == null || navMeshAgent == null || !navMeshAgent.enabled)
        {
            Finish();
            return;
        }

        if (control.currentExplosiveThreat == null)
        {
            Finish();
            return;
        }

        float distanceToTarget =
            Vector3.Distance(
                navMeshAgent.transform.position,
                control.currentDodgeTarget);

        if (!navMeshAgent.pathPending &&
            distanceToTarget <= control.dodgeArrivalTolerance)
        {
            Finish();
        }
    }

    public override void Exit()
    {
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.speed    = originalSpeed;
            navMeshAgent.isStopped = false;
        }

        control.isDodging    = false;
        control.hasDodgeTarget = false;
    }
}
