using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using AIAnimation;

/// <summary>
/// AIs who transition into this state will follow their target
/// </summary>
public class FollowState : IAIState
{
    private AIBase ai;    // originally CivilianAI, but this lets all AI use this state                     
    private Transform target;                  
    private AIAnimationController animController; 

    public FollowState(AIBase ai, Transform target)
    {
        this.ai = ai;
        this.target = target;
    }
    public void Enter()
    {
        animController = ai.agent.gameObject.GetComponent<AIAnimationController>();
        ai.ResumeMoving();
    }
    public void Stay()
    {
        animController.SetAnimation(AIAnimationController.AnimationState.Walk);

        // If target exists and is further than follow distance, move towards target
        if (target != null && Vector3.Distance(ai.transform.position, target.position) > ai.followDistance)
        {
            ai.MoveTo(target.position);
        }

        // If target is null (died for example), switch back to idle state
        if (target == null)
        {
            ai.ChangeState(new IdleState(ai));
        }
    }

    public void Exit()
    {
        ai.ResumeMoving();
    }

    // Didn't put into use, but this coroutine stuns the Civ for 5 seconds before resuming movement
    private IEnumerator StunnedDelay()
    { 
        animController.SetAnimation(AIAnimationController.AnimationState.Idle);
        ai.StopMoving();
        yield return new WaitForSeconds(5f);
        ai.ResumeMoving();
    }
    
    #region Drafts
    /*private AIBase ai;
    private GameObject target;
    private NavMeshAgent agent;
    private float followRange = 2f;
    private float followOffset = 1f;
    
    // [Header("Following Settings")]
    // private float followDistance = 0.5f;
    // private float followOffset = 0.3f;
    // private float updateInterval = 0.3f;
    // private float lastUpdateTime;
    // private Vector3 currentFollowPosition;
    // private bool isFollowing;
    public FollowState(CivAI ai, GameObject target)
    {
        this.ai = ai;
        this.target = target;
        agent = ai.GetComponent<NavMeshAgent>();
    }
    public void Enter()
    {
        // isFollowing = true;
        // lastUpdateTime = Time.time;
        // agent.stoppingDistance = followDistance * 0.8f;
        // agent.autoBraking = true;
        // UpdateFollowPosition();
    }
    public void Stay()
    {
        // if (!isFollowing || target == null) return;
        // // Update at intervals for performance
        // if (Time.time - lastUpdateTime > updateInterval)
        // {
        //     UpdateFollowPosition();
        //     lastUpdateTime = Time.time;
        // } 
        float distance = Vector3.Distance(ai.transform.position, target.transform.position);
        // Vector3 followPos = target.transform.position - target.transform.forward * followOffset
        //     + target.transform.right * followOffset;
        if (distance > followRange)
        {
            agent.SetDestination(target.transform.position);
            //agent.destination = target.transform.position;
        }
        //ai.FaceDirection();
        Debug.Log($"Velocity: {agent.velocity}, PathPending: {agent.pathPending}, HasPath: {agent.hasPath}, Stopped: {agent.isStopped}");
    }
    public void Exit()
    {
        // isFollowing = false;
        // agent.ResetPath();
    }*/
    
    
    /*private void UpdateFollowPosition()
    {
        if (target == null) return;
        // Calculate position behind target 
        Vector3 targetForward = target.forward;
        currentFollowPosition = target.position - targetForward * followDistance
                            + target.right * followOffset;
        // Validate position is on NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(currentFollowPosition, out hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            // Fallback to simple position if NavMesh sampling fails
            agent.SetDestination(currentFollowPosition);
        }
    }
    public void OnCollisionWithTarget(Collision collision)
    {
        if (collision.transform == target && !isFollowing)
        {
            isFollowing = true;
            Enter();
        }
    }*/
    /*private AIBase ai;
    private Transform target;
    private float followDistance = .2f;
    //private float reachThreshold = .5f; // determines 'reached'
    private bool targetRestored = false; // Tracks if the obstacle/agent has been toggled back
    private float updateFollowPosThreshold = 0.5f; 
    private Vector3 currentFollowPos;
    
   // private NavMeshObstacle tempObstacle; // Temporary NavMeshObstacle to prevent Civ walking into the target/block their way
    //private NavMeshAgent targetAgent;

    public FollowState(AIBase ai, Transform target)
    {
        // Stores this AI and its target to this state
        this.ai = ai;
        this.target = target;
    }

    public void Enter()
    {

    }
    public void Stay()
    {
        if (target == null) return;

        Vector3 newFollowPos = target.position - target.forward.normalized * followDistance;

        // Check if the target moved enough to justify updating follow pos
        if (Vector3.Distance(newFollowPos, currentFollowPos) > updateFollowPosThreshold)
        {
            Debug.Log("check");
            currentFollowPos = newFollowPos;
            ai.MoveTo(currentFollowPos);
        }

        ai.FaceDirection();
    }
    public void Exit()
    {
       
    }
    private void UpdateFollowPos()
    {
        currentFollowPos = target.position - target.forward.normalized * followDistance;
    }*/
    #endregion
}
