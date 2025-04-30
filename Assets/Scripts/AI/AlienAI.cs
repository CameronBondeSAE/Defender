using UnityEngine;
using AIAnimation;

public class AlienAI : AIBase
{
    public float tagDistance;
    public Transform mothership;
    public bool isReached = false;
    
    private AIAnimationController animController;

    [HideInInspector] public AIBase currentTargetCiv;

    protected override void Start()
    {
        base.Start();
        ChangeState(new SearchState(this));
        animController = GetComponentInChildren<AIAnimationController>();
    }

     void Update()
    {
        base.Update();
        if (isReached)
        {
            ChangeState(new SearchState(this));
        }
    }
    public void StopMoving()
    {
        agent.isStopped = true;
    }

    public void ResumeMoving()
    {
        agent.isStopped = false;
    }
}
