using UnityEngine;
using UnityEngine.AI;
using Anthill.AI;
using Defender;
/// <summary>
/// in this state he'll run to the nearest snack (picked by SmartAlienSense), and once close enough
/// call SmartAlienControl.OnItemPickedUp on the snack
/// </summary>
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
        if (control == null)
        {
            Finish();
            return;
        }

        // snack will be chosen by SmartAlienSense in the last scan
        snackTarget = control.currentSnackTarget;
        // if no snack, bail & choose another action
        if (snackTarget == null)
        {
            Debug.Log("no snack target");
            Finish();
            return;
        }

        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
            agent.SetDestination(snackTarget.transform.position);
        }
    }

    public override void Execute(float aDeltaTime, float aTimeScale)
    {
        // switch (currentStep)
        // {
        //     case Step.MoveToSnack:
        //     {
        //         if (snackTarget == null)
        //         {
        //             Finish();
        //             return;
        //         }
        //
        //         float dist = Vector3.Distance(agent.transform.position, snackTarget.transform.position);
        //         if (dist <= control.interactRange)
        //         {
        //             // pick it up via IPickup
        //             IPickup pickup = snackTarget.GetComponent<IPickup>();
        //             if (pickup == null)
        //             {
        //                 pickup = snackTarget.GetComponentInChildren<IPickup>(true);
        //             }
        //
        //             if (pickup != null)
        //             {
        //                 CharacterBase character = control.GetComponent<CharacterBase>();
        //                 if (character != null)
        //                 {
        //                     pickup.Pickup(character);
        //
        //                     UsableItem_Base usableItemBase = snackTarget;
        //                     if (usableItemBase == null)
        //                     {
        //                         usableItemBase = snackTarget.GetComponent<UsableItem_Base>();
        //                     }
        //                     control.OnItemPickedUp(usableItemBase);
        //                 }
        //             }
        //
        //             Finish();
        //         }
        //         break;
        //     }
        //     case Step.MoveToCrate:
        //     {
        //         if (crateTarget == null)
        //         {
        //             Finish();
        //             return;
        //         }
        //
        //         float dist = Vector3.Distance(agent.transform.position, crateTarget.transform.position);
        //         if (dist <= control.interactRange)
        //         {
        //             currentStep = Step.RequestFromCrate;
        //         }
        //         break;
        //     }
        //     case Step.RequestFromCrate:
        //     {
        //         if (crateTarget == null)
        //         {
        //             Finish();
        //             return;
        //         }
        //         UsableItem_Base item;
        //         bool gotItem = crateTarget.TryGiveItemToSmartAlien(control, out item);   
        //
        //         if (gotItem)
        //         {
        //             control.OnItemPickedUp(item); // registers and re-parents 
        //
        //             if (item != null && item.RoleForAI == UsableItem_Base.ItemRoleForAI.Snack)
        //             {
        //                 Finish(); // got snack
        //                 return;
        //             }
        //
        //             // if it is some other threat device, planner will adapt via world state
        //             Finish();
        //         }
        //         else
        //         {
        //             // can't get item 
        //             Finish();
        //         }
        //         break;
        //     }
        // }
        if (agent == null || !agent.enabled || snackTarget == null)
        {
            Finish();
            return;
        }

        float dist = Vector3.Distance(agent.transform.position, snackTarget.transform.position);
        if (dist > control.interactRange)
        {
            return;
        }
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

                control.OnItemPickedUp(usableItemBase); // sets heldItem & parents to ItemHoldPos
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
    }
}

