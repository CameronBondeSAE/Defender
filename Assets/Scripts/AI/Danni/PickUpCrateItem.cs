using Anthill.AI;
using UnityEngine;
using UnityEngine.AI;
using Defender;

public class PickupCrateItem : AntAIState
{
    private SmartAlienControl control;
    private NavMeshAgent agent;
    private NetworkedCrate crateTarget;

    public override void Create(GameObject aGameObject)
    {
        control = aGameObject.GetComponent<SmartAlienControl>();
        agent   = aGameObject.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        if (control == null)
        {
            Finish();
            return;
        }

        crateTarget = control.currentCrateTarget;
        if (crateTarget == null || control.heldItem != null)
        {
            Finish();
            return;
        }

        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
            if (!control.IsAgentNearCrate(crateTarget))
            {
                Vector3 approachPos = control.GetCrateApproachPosition();
                agent.SetDestination(approachPos);
            }
        }
    }

    public override void Execute(float aDeltaTime, float aTimeScale)
    {
        if (control == null || crateTarget == null || agent == null || !agent.enabled)
        {
            Finish();
            return;
        }
        if (!control.IsAgentNearCrate(crateTarget))
        {
            return;
        }

        UsableItem_Base item;
        bool gotItem = crateTarget.TryGiveItemToSmartAlien(control, out item);

        if (gotItem && item != null)
        {
            control.OnItemPickedUp(item);
        }

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
