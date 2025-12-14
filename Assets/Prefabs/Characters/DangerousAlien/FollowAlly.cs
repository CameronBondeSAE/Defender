using Anthill.AI;
using UnityEngine;
using UnityEngine.AI;
/// <summary>
/// in this state, if he is outside accompanyDistance to his ally, he'll pick a spot near them and move there,
/// and once he's close enough he'll Finish() & planner'll choose what to do next
/// </summary>
public class FollowAlly : AntAIState
{
    private DangerousAlienControl control;
    private NavMeshAgent agent;

    public override void Create(GameObject aGameObject)
    {
        control = aGameObject.GetComponent<DangerousAlienControl>();
        agent   = aGameObject.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        if (control == null || agent == null || !control.HasAlly)
        {
            Finish();
            return;
        }

        agent.isStopped = false;
        MoveNextToAlly();
    }

    public override void Execute(float aDeltaTime, float aTimeScale)
    {
        if (control == null || agent == null || !control.HasAlly)
        {
            Finish();
            return;
        }
        if (!control.IsWithinAccompanyDistance())
        {
            MoveNextToAlly();
        }
        else
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

    private void MoveNextToAlly()
    {
        Vector3 allyPos = control.smartAlly.transform.position;
        Vector3 dir = control.transform.position - allyPos;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
        {
            dir = control.transform.forward;
        }
        dir.Normalize();

        // stand at accompanyDistance from ally
        Vector3 target = allyPos + dir * control.accompanyDistance;
        agent.SetDestination(target);
    }
}
