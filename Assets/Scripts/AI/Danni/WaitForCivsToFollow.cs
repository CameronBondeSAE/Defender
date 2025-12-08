using Anthill.AI;
using Defender;       
using UnityEngine;
using UnityEngine.AI;

public class WaitForCivsToFollow : AntAIState
{
    private SmartAlienControl control;
    private NavMeshAgent agent;

    [Header("Wait Settings")]
    public float civFollowRadius = 2.5f;
    public float maxWaitTime = 6f;

    private float waitTimer;

    public override void Create(GameObject aGameObject)
    {
        control = aGameObject.GetComponent<SmartAlienControl>();
        agent   = aGameObject.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        waitTimer = 0f;

        // stop moving while wait
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
        }
    }

    public override void Execute(float aDeltaTime, float aTimeScale)
    {
        if (control == null)
        {
            Finish();
            return;
        }

        waitTimer += aDeltaTime * aTimeScale;
        
        if (control.currentCivGroup == null || control.currentCivGroup.Count == 0)
        {
            Finish();
            return;
        }
        
        int closeCount = 0;
        Vector3 myPos = control.transform.position;

        for (int i = 0; i < control.currentCivGroup.Count; i++)
        {
            AIBase civ = control.currentCivGroup[i];
            if (civ == null) continue;

            float dist = Vector3.Distance(myPos, civ.transform.position);
            if (dist <= civFollowRadius)
            {
                closeCount++;
            }
        }

        // if enough civs are close, he'd be done waiting
        // move on to Escort
        if (closeCount >= control.minCivCrowdSize)
        {
            Finish();
            return;
        }

        // if he waits too long and civs never really grouped up,
        // finish anyway so planner pick a new action
        if (waitTimer >= maxWaitTime)
        {
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
