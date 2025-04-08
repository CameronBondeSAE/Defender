using UnityEngine;

/// <summary>
/// Civ Controller
/// </summary>
public class CivAI : AIBase
{
    public Vector3[] patrolPoints;
    protected override void Start()
    {
        base.Start();
        ChangeState(new PatrolState(this));
    }
    
    // Function when they are tagged
    public void OnTagged(Transform tagger) 
    {
        ChangeState(new FollowState(this, tagger));
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Alien")) 
        {
            OnTagged(other.transform);
        }
    }
    
    private Vector3[] GetPatrolPointsPositions()
    {
        Vector3[] points = new Vector3[patrolPoints.Length];
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            points[i] = patrolPoints[i];
        }
        return points;
    }
}
