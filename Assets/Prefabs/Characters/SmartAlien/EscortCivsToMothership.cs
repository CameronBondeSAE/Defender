using Anthill.AI;
using UnityEngine;
using UnityEngine.AI;
/// <summary>
/// once the snack has pulled a civ crowd together, he'll take them to the mothership drop zone,
/// mark them as delivered, then wipe the group + snack so the escort flag resets
/// </summary>
public class EscortCivsToMothership : AntAIState
{
    private SmartAlienControl control;
    private NavMeshAgent agent;
    private bool hasCompletedDrop = false; 
    private float originalSpeed;
    private Vector3 dropDestination;
    public override void Create(GameObject aGameObject)
    {
        control = aGameObject.GetComponent<SmartAlienControl>();
        agent   = aGameObject.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        hasCompletedDrop = false;

        if (control == null)
        {
            Finish();
            return;
        }

        dropDestination = control.GetMothershipDestination();
        // // in case the above doesn't work...
        // if (control.mothershipDropPoint == null)
        // {
        //     MothershipDropZone ms = Object.FindObjectOfType<MothershipDropZone>();
        //     if (ms != null)
        //     {
        //         control.mothershipDropPoint = ms.transform;
        //     }
        // }

        if (control.mothershipDropPoint == null)
        {
            Finish();
            return;
        }

        control.civsAtMothership = false;

        if (!control.escortInProgress)
        {
            control.escortInProgress = true;
        }

        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
            originalSpeed       = agent.speed;
            agent.speed         = control.escortMoveSpeed;

            agent.SetDestination(dropDestination);
        }
        
        if (control.sfx != null)
        {
            control.sfx.PlayEscortStart();
        }
    }

    public override void Execute(float aDeltaTime, float aTimeScale)
    {
        if (control == null || control.mothershipDropPoint == null)
        {
            Finish();
            return;
        }

        if (agent == null || !agent.enabled)
        {
            Finish();
            return;
        }
        if (hasCompletedDrop)              
        {
            Finish();
            return;
        }
        bool atDropPoint = control.IsAgentNear(
            dropDestination,
            control.interactRange + 0.75f); // lil offset

        if (!atDropPoint)
        {
            return; 
        }

        agent.isStopped = true;

        hasCompletedDrop         = true;    
        control.civsAtMothership = true;
        control.escortInProgress = false;
        control.snackDeployed    = false;
        if (control.heldItem != null)
        {
            control.heldItem.DestroyItem();
            control.heldItem = null;
        }
        if (control.currentCivGroup != null)
        {
            control.currentCivGroup.Clear();
        }
        control.needsScan = true;           
        
        if (control.sfx != null)
        {
            control.sfx.PlayCivsDroppedOff();
        }

        Finish();
    }

    public override void Exit()
    {
        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;

            // restore speed
            if (originalSpeed > 0f)
            {
                agent.speed = originalSpeed;
            }
        }
        
        // destroy snack after escort is done
        if (control != null && control.heldItem != null)
        {
            if (control.heldItem.RoleForAI == UsableItem_Base.ItemRoleForAI.Snack)
            {
                control.heldItem.DestroyItem();
            }

            control.heldItem = null;
        }


        if (control != null && !hasCompletedDrop)
        {
            control.needsScan = true;
        }
    }
}
