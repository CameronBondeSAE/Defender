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
            if (!control.IsAgentNear(crateTarget.transform.position, control.interactRange))
            {
                agent.SetDestination(crateTarget.transform.position);
            }
        }
    }

    public override void Execute(float aDeltaTime, float aTimeScale)
    {
        if (control == null || crateTarget == null)
        {
            Finish();
            return;
        }

        if (agent == null || !agent.enabled)
        {
            Finish();
            return;
        }
        if (!control.IsAgentNear(crateTarget.transform.position, control.interactRange))
        {
            return;
        }

        // only one item per spawn!
        UsableItem_Base item;
        bool gotItem = crateTarget.TryGiveItemToSmartAlien(control, out item);

        if (gotItem && item != null)
        {
            control.OnItemPickedUp(item); 
        }
        // whether he got something or not, finish this state anyway
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
