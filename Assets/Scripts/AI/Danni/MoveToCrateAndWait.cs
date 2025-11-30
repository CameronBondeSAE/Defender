using Anthill.AI;
using UnityEngine;
using UnityEngine.AI;

public class MoveToCrateAndWait : AntAIState
{
    private SmartAlienControl control;
    private NavMeshAgent agent;
    private Vector3 standPos;

    public override void Create(GameObject aGameObject)
    {
        control = aGameObject.GetComponent<SmartAlienControl>();
        agent   = aGameObject.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        if (control == null || control.currentCrateTarget == null || agent == null || !agent.enabled)
        {
            Finish();
            return;
        }
        Vector3 cratePos = control.currentCrateTarget.transform.position;
        Vector3 dir      = control.transform.position - cratePos;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
        {
            dir = control.transform.forward;
        }

        dir.Normalize();
        standPos = cratePos + dir * 1.0f;

        agent.isStopped = false;
        agent.SetDestination(standPos);
    }

    public override void Execute(float aDeltaTime, float aTimeScale)
    {
        if (control == null || control.currentCrateTarget == null)
        {
            Finish();
            return;
        }

        if (agent == null || !agent.enabled)
        {
            Finish();
            return;
        }

        if (control.IsAgentNear(standPos, control.interactRange))
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
        // if (control != null)
        // {
        //     control.needsScan = true;   
        // }
    }
}