using Anthill.AI;
using UnityEngine;
using UnityEngine.AI;

public class DestroyCrateItem : AntAIState
{
    private SmartAlienControl control;
    private NavMeshAgent agent;

    private Vector3 disposePosition;
    private bool hasTarget;

    public override void Create(GameObject aGameObject)
    {
        control = aGameObject.GetComponent<SmartAlienControl>();
        agent   = aGameObject.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        hasTarget = false;

        if (control == null || control.heldItem == null)
        {
            Finish();
            return;
        }
        if (control.heldItem.RoleForAI == UsableItem_Base.ItemRoleForAI.Snack)
        {
            Finish();
            return;
        }
        disposePosition = control.GetCrateDisposePosition();
        hasTarget       = true;
        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
            agent.SetDestination(disposePosition);
        }
    }

    public override void Execute(float aDeltaTime, float aTimeScale)
    {
        if (!hasTarget || agent == null || !agent.enabled || control == null || control.heldItem == null)
        {
            Finish();
            return;
        }
        float remaining = agent.hasPath ? agent.remainingDistance : Mathf.Infinity;
        float dist      = Vector3.Distance(agent.transform.position, disposePosition);

        float destroyRange = control.interactRange + 0.5f; // small offset as navmesh agent may not move to exactly the destroy pos

        bool atDisposePoint =
            (remaining <= destroyRange) ||
            (dist      <= destroyRange);

        if (!atDisposePoint)
        {
            return; 
        }

        // actually destroy the held item
        UsableItem_Base item = control.heldItem;
        if (item != null)
        {
            item.DestroyItem(); 
        }

        control.heldItem = null;
        control.needsScan = true; // start the next planning cycle

        Finish();
    }

    public override void Exit()
    {
        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
        }
    }
}
