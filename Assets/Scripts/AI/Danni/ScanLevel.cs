using Anthill.AI;
using UnityEngine;
using UnityEngine.AI;

public class ScanLevel : AntAIState
{
    private SmartAlienControl control;
    private NavMeshAgent agent;
    
    public float idleDuration;

    private float timer;

    public override void Create(GameObject aGameObject)
    {
        control = aGameObject.GetComponent<SmartAlienControl>();
        agent   = aGameObject.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        if (control == null)
        {
            Finish();
            return;
        }
        control.needsScan = true;      

        timer = 0f;                   

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    public override void Execute(float aDeltaTime, float aTimeScale)
    {
        if (control == null)
        {
            Finish();
            return;
        }

        timer += aDeltaTime * aTimeScale;

        float duration = (control.scanDuration > 0f)
            ? control.scanDuration
            : idleDuration;

        if (timer >= duration)
        {
            control.needsScan = false; 
            Finish();
        }
    }

    public override void Exit()
    {
        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
        }

        if (control != null)
        {
            control.needsScan = false; 
        }
    }
}