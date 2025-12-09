using UnityEngine;
using UnityEngine.AI;
using Anthill.AI;
using AIAnimation;

public class DefendCrate : AntAIState
{
    private DangerousAlienControl control;
    private NavMeshAgent navMeshAgent;
    private AIAnimationController animationController;

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

        if (control.allyCrateTarget == null)
        {
            Finish();
            return;
        }

        control.isDefendingCrate = true;
        navMeshAgent.isStopped = false;

        Vector3 cratePosition = control.allyCrateTarget.transform.position;
        Vector3 offset = (control.transform.position - cratePosition).normalized;
        if (offset.sqrMagnitude < 0.01f)
        {
            offset = control.transform.right;
        }

        Vector3 defendPosition =
            cratePosition + offset * (control.crateDefendRadius * 0.5f);

        navMeshAgent.SetDestination(defendPosition);

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

        if (control.allyCrateTarget == null)
        {
            Finish();
            return;
        }

        if (!navMeshAgent.pathPending &&
            navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.1f)
        {
            Finish();
        }
    }

    public override void Exit()
    {
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.isStopped = false;
        }

        control.isDefendingCrate = false;
    }
}

