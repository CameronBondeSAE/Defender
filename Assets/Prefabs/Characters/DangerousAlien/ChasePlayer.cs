using UnityEngine;
using UnityEngine.AI;
using Anthill.AI;

public class ChasePlayer : AntAIState
{
    private DangerousAlienControl control;
    private NavMeshAgent navMeshAgent;
    private Transform player;
    private float repathInterval = 0.3f;
    private float repathTimer;

    public override void Create(GameObject aGameObject)
    {
        control      = aGameObject.GetComponent<DangerousAlienControl>();
        navMeshAgent = aGameObject.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        if (control == null || navMeshAgent == null || !navMeshAgent.enabled)
        {
            Finish();
            return;
        }

        player = control.playerTransform;

        if (player == null)
        {
            Finish();
            return;
        }

        navMeshAgent.isStopped = false;
        repathTimer = 0f;
        navMeshAgent.SetDestination(player.position);
    }

    public override void Execute(float aDeltaTime, float aTimeScale)
    {
        if (control == null || navMeshAgent == null || !navMeshAgent.enabled || player == null)
        {
            Finish();
            return;
        }
        repathTimer += aDeltaTime;
        if (repathTimer >= repathInterval)
        {
            navMeshAgent.SetDestination(player.position);
            repathTimer = 0f;
        }

        float distanceToPlayer =
            Vector3.Distance(
                navMeshAgent.transform.position,
                player.position);

        if (distanceToPlayer <= control.attackRange)
        {
            Finish();
        }
        
        if (distanceToPlayer > control.chaseRange)
        {
            Finish(); 
            return;
        }
        if (control.HasAlly && !control.IsInsideProtectionRange())
        {
            Finish();
            return;
        }
    }

    public override void Exit()
    {
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.isStopped = false;
        }
    }
}