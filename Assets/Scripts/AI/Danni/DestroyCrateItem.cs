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
        if (Vector3.Distance(agent.transform.position, disposePosition) <= control.interactRange)
        {
            UsableItem_Base item = control.heldItem;
            if (item != null)
            {
                item.DestroyItem();
            }

            control.heldItem = null;

            // after destroying, scan again so he can decide
            // whether to go back to the crate or deal with a threat/snack etc
            control.needsScan = true;

            Finish();
        }
    }

    public override void Exit()
    {
        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
        }
    }
}
