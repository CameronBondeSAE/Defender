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

        float rangeToUse = (control != null) ? control.interactRange : interactRange;
        float dist = Vector3.Distance(agent.transform.position, target.transform.position);
        if (dist > rangeToUse)
        {
            return;
        }
        UsableItem_Base usableItemBase = target;
        if (usableItemBase != null)
        {
            usableItemBase.ForceDeactivate();
        }
        else
        {
            // in case some items only have IUsable/didn't use my base class
            // stop it any way
            IUsable usable = target;
            if (usable != null)
            {
                usable.StopUsing();
            }
        }
            Finish();
    }

    public override void Exit()
    {
        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
        }
        if (control != null)
        {
            control.needsScan = true;   
            control.currentThreatTarget = null;
        }

    }
}