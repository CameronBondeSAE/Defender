using Anthill.AI;
using UnityEngine;
using UnityEngine.AI;

public class EscortCivsToMothership : AntAIState
{
    private SmartAlienControl control;
    private NavMeshAgent agent;

    public override void Create(GameObject aGameObject)
    {
        control = aGameObject.GetComponent<SmartAlienControl>();
        agent   = aGameObject.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        if (control == null || control.mothershipDropPoint == null)
        {
            Finish();
            return;
        }

        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
            agent.SetDestination(control.mothershipDropPoint.position);
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

        if (control.IsAgentNear(
                control.mothershipDropPoint.position,
                control.interactRange))
        {
            agent.isStopped = true;
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