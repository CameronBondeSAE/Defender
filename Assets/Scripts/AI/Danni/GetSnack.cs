using UnityEngine;
using UnityEngine.AI;
using Anthill.AI;
using Defender;

public class GetSnack : AntAIState
{
    private SmartAlienControl control;
    private NavMeshAgent agent;
    private UsableItem_Base snackTarget;
    private NetworkedCrate crateTarget;
    private enum Step
    {
        None,
        MoveToSnack,
        MoveToCrate,
        RequestFromCrate
    }

    private Step currentStep;

    public override void Create(GameObject aGameObject)
    {
        control = aGameObject.GetComponent<SmartAlienControl>();
        agent = aGameObject.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        snackTarget = control.currentSnackTarget;
        crateTarget = control.currentCrateTarget;

        if (snackTarget != null)
        {
            currentStep = Step.MoveToSnack;
            if (agent != null && agent.enabled)
            {
                agent.isStopped = false;
                agent.SetDestination(snackTarget.transform.position);
            }
        }
        else if (crateTarget != null)
        {
            currentStep = Step.MoveToCrate;
            if (agent != null && agent.enabled)
            {
                agent.isStopped = false;
                agent.SetDestination(crateTarget.transform.position);
            }
        }
        else
        {
            currentStep = Step.None;
            Finish();
        }
    }

    public override void Execute(float aDeltaTime, float aTimeScale)
    {
       if (agent == null || !agent.enabled)
        {
            Finish();
            return;
        }

        switch (currentStep)
        {
            case Step.MoveToSnack:
            {
                if (snackTarget == null)
                {
                    Finish();
                    return;
                }

                float dist = Vector3.Distance(agent.transform.position, snackTarget.transform.position);
                if (dist <= control.interactRange)
                {
                    // pick it up via IPickup
                    IPickup pickup = snackTarget.GetComponent<IPickup>();
                    if (pickup == null)
                    {
                        pickup = snackTarget.GetComponentInChildren<IPickup>(true);
                    }

                    if (pickup != null)
                    {
                        CharacterBase character = control.GetComponent<CharacterBase>();
                        if (character != null)
                        {
                            pickup.Pickup(character);

                            UsableItem_Base usableItemBase = snackTarget;
                            if (usableItemBase == null)
                            {
                                usableItemBase = snackTarget.GetComponent<UsableItem_Base>();
                            }
                            control.OnItemPickedUp(usableItemBase);
                        }
                    }

                    Finish();
                }
                break;
            }
            case Step.MoveToCrate:
            {
                if (crateTarget == null)
                {
                    Finish();
                    return;
                }

                float dist = Vector3.Distance(agent.transform.position, crateTarget.transform.position);
                if (dist <= control.interactRange)
                {
                    currentStep = Step.RequestFromCrate;
                }
                break;
            }
            case Step.RequestFromCrate:
            {
                if (crateTarget == null)
                {
                    Finish();
                    return;
                }
                UsableItem_Base item;
                bool gotItem = crateTarget.TryGiveItemToSmartAlien(control, out item);   

                if (gotItem)
                {
                    control.OnItemPickedUp(item); // registers and re-parents 

                    if (item != null && item.RoleForAI == UsableItem_Base.ItemRoleForAI.Snack)
                    {
                        Finish(); // got snack
                        return;
                    }

                    // if it is some other threat device, planner will adapt via world state
                    Finish();
                }
                else
                {
                    // can't get item 
                    Finish();
                }
                break;
            }
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

