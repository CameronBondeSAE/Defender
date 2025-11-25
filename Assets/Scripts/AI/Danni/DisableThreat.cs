using UnityEngine;
using UnityEngine.AI;
using Anthill.AI;

public class DisableThreat : AntAIState
{
    private SmartAlienControl control;
    private NavMeshAgent agent;
    private UsableItem_Base target;
    private float interactRange = 2.0f;

    public override void Create(GameObject aGameObject)
    {
        control = aGameObject.GetComponent<SmartAlienControl>();
        agent = aGameObject.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        target = control.currentThreatTarget;
        if (target == null)
        {
            Finish();
            return;
        }

        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
            agent.SetDestination(target.transform.position);
        }
    }

    public override void Execute(float aDeltaTime, float aTimeScale)
    {
        if (target == null)
        {
            Finish();
            return;
        }

        if (agent == null || !agent.enabled)
        {
            Finish();
            return;
        }

        float rangeToUse = control != null ? control.interactRange : 2.0f; 
        float dist = Vector3.Distance(agent.transform.position, target.transform.position);
        if (dist <= interactRange)
        {
            // turn it off
            IUsable usable = target as IUsable;
            if (usable != null)
            {
                usable.StopUsing();
            }
            // disarm countdown and expiry 
            UsableItem_Base usableItemBase = target as UsableItem_Base;
            if (usableItemBase != null)
            {
                usableItemBase.Disarm();
            }
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