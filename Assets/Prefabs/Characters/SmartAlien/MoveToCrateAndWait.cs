using Anthill.AI;
using UnityEngine;
using UnityEngine.AI;
/// <summary>
/// in this state he will go to the mothership drop zone, chill there for a moment so the planner can re-evaluate
/// </summary>
public class MoveToCrateAndWait : AntAIState
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
        if (control == null || control.currentCrateTarget == null)
        {
            Finish();
            return;
        }

        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
            Vector3 approachPos = control.GetCrateApproachPosition();
            agent.SetDestination(approachPos);
        }
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

        if (control.IsAgentNearCrate(control.currentCrateTarget))
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