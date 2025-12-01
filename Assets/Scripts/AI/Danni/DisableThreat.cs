using UnityEngine;
using UnityEngine.AI;
using Anthill.AI;
using NicholasScripts;  

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
        float dist       = Vector3.Distance(agent.transform.position, target.transform.position);

        if (dist > rangeToUse)
        {
            return;
        }

        // the disarming chunk
        UsableItem_Base usableItemBase = target;

        if (usableItemBase != null)
        {
            // only treat as a threat if it is actually active
            bool isActiveThreat =
                usableItemBase.IsActivated ||
                usableItemBase.IsCountdownActive ||
                usableItemBase.IsExpiryActive;

            if (isActiveThreat)
            {
                // stop countdown/expiry /UI stuff
                usableItemBase.ForceDeactivate();

                // if this item is a turret, disable the turret view logic to accomodate to nick's code
                Model_Turret turretModel = usableItemBase as Model_Turret;
                if (turretModel != null)
                {
                    // turn off the activation bool so BaseTurret.Update stops firing bullets
                    turretModel.isActivated = false;
                    BaseTurret turret = turretModel.GetComponentInParent<BaseTurret>();
                    if (turret != null)
                    {
                        turret.SetPowered(false);
                    }
                }
            }
        }
        else
        {
            // for other things that only implement IUsable
            IUsable usable = target;
            if (usable != null)
            {
                usable.StopUsing();
            }
        }

        // clear and rescan
        if (control != null)
        {
            control.currentThreatTarget = null;
            control.needsScan           = true;
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