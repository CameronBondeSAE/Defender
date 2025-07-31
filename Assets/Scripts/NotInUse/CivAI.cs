using UnityEngine;
using AIAnimation;
using UnityEngine.AI;

/// <summary>
/// Civ Controller
/// </summary>
public class CivAI : AIBase
{
    /*private AIAnimationController animController;
    private bool isTagged;
    private NavMeshTest alienScript;

    public void Start()
    {
        animController = GetComponent<AIAnimationController>();
        base.Start();
        if (!isTagged)
        {
            ChangeState(new PatrolState(this));
            animController.SetAnimation(AIAnimationController.AnimationState.Walk);
        }
    }
    
    public void OnTagged(GameObject tagger) 
    {
        if (isTagged) return;
        isTagged = true;
        alienScript = tagger.GetComponent<NavMeshTest>();
        alienScript.onWayToMothership = true;
        ChangeState(new FollowState(this, tagger));
        animController.SetAnimation(AIAnimationController.AnimationState.Walk);
    }

    private void OnTriggerEnter(Collider other)
    {
        // For fallback self-tagging logic
        if (!isTagged && (other.CompareTag("Player") || other.CompareTag("Alien")))
        {
            OnTagged(other.gameObject);
        }
    }*/
    
    // private Vector3[] GetPatrolPointsPositions()
    // {
    //     Vector3[] points = new Vector3[patrolPoints.Length];
    //     for (int i = 0; i < patrolPoints.Length; i++)
    //     {
    //         points[i] = patrolPoints[i];
    //     }
    //     return points;
    // }
}
