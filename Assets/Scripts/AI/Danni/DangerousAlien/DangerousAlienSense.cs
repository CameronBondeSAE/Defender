using Anthill.AI;
using UnityEngine;
using UnityEngine.AI;

public class DangerousAlienSense : MonoBehaviour, ISense
{
    // private DangerousAlienControl control;
    private Transform selfTransform;
    private NavMeshAgent navAgent;
    private Transform playerTransform;
    private AntAIAgent antAgent;

    [Header("Detection")] 
    public float playerVisionRange = 20f;
    public float playerAttackRange;
    public float fieldOfViewAngle;
    public int fieldOfViewRayCount;
    public LayerMask detectionMask;
    public float stoppingBufferDistance;
    
    public float lowHealthFraction = 0.25f;
    
    public enum DangerousAlien
    {
        PlayerVisible = 0,
        InAttackRange = 1,
        HasExplosiveThreat = 2,
        HasDodgeTarget = 3,
        AllyAtCrate = 4,
        DefendingCrate = 5,
        AllyInTurretRange = 6,
        NeedsScan = 7,
        IsMoving = 8,
        LowHealth = 9
    }

    private void Awake()
    {
        selfTransform = transform;
        // control = GetComponent<DangerousAlienControl>();
        navAgent = GetComponent<NavMeshAgent>();
        antAgent = GetComponent<AntAIAgent>();
    }
    
    public void CollectConditions(AntAIAgent aAgent, AntAICondition aWorldState)
    {
        // if (control == null) return;
        bool currentlyMoving = false;
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            currentlyMoving = !navAgent.isStopped &&
                              navAgent.hasPath &&
                              navAgent.remainingDistance > (navAgent.stoppingDistance + stoppingBufferDistance);
        }
        // control.isMoving = currentlyMoving;
        bool playerVisible = false;
        bool inAttackRange = false;
        
       // Transform player =
    }
}
