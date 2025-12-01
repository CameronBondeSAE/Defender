using Anthill.AI;
using UnityEngine;
using UnityEngine.AI;

public class EscortCivsToMothership : AntAIState
{
    private SmartAlienControl control;
    private NavMeshAgent agent;
    private bool hasCompletedDrop = false; 
    private float originalSpeed;

    public override void Create(GameObject aGameObject)
    {
        control = aGameObject.GetComponent<SmartAlienControl>();
        agent   = aGameObject.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        hasCompletedDrop = false;

        if (control == null || control.mothershipDropPoint == null)
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
            originalSpeed = agent.speed;
            
            if (control.escortMoveSpeed > 0f)
            {
                agent.speed = control.escortMoveSpeed;
            }

            agent.isStopped = false;
            agent.SetDestination(control.mothershipDropPoint.position);
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
            control.mothershipDropPoint.position,
            control.interactRange
        );

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

        if (control != null && !hasCompletedDrop)
        {
            control.needsScan = true;
        }
    }
}
