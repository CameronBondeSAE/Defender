using Anthill.AI;
using UnityEngine;
using UnityEngine.AI;

public class DangerousAlienSense : MonoBehaviour, ISense
{
     private DangerousAlienControl control;
    private Transform selfTransform;
    private NavMeshAgent navMeshAgent;
    private AntAIAgent antAgent;

    // [Header("Detection")]
    // public float playerVisionRange = 20f;
    // public float playerAttackRange = 3f;
    // public float fieldOfViewAngle = 120f;
    // public int fieldOfViewRayCount = 7;
    
    public float lowHealthFraction = 0.3f;
    public enum DangerousAlien
    {
        HasPlayer = 0,
        PlayerInChaseRange = 1,
        ThreatNearby = 2,
        HasDodgeTarget = 3,
        AllyAtCrate = 4,
        DefendingCrate = 5,
        AllyInTurretRange = 6,
        NeedsScan = 7,
        IsMoving = 8,
        LowHealth = 9,
        HasAlly = 10,
        InProtectionRange = 11,
        InAttackRange = 12
    }
    private void Awake()
    {
        control       = GetComponent<DangerousAlienControl>();
        selfTransform = transform;
        navMeshAgent  = GetComponent<NavMeshAgent>();
        antAgent      = GetComponent<AntAIAgent>();
    }

    public void CollectConditions(AntAIAgent aAgent, AntAICondition aWorldState)
    {
        // bool playerVisible = false;
        // bool inAttackRange = false;
        // Transform player = control.playerTransform;
        //
        // if (player != null)
        // {
        //     Vector3 origin = selfTransform.position + Vector3.up * 1.1f;
        //     Vector3 toPlayer = player.position - origin;
        //     float distanceToPlayer = toPlayer.magnitude;
        //
        //     if (distanceToPlayer <= playerVisionRange)
        //     {
        //         float angleToPlayer =
        //             Vector3.Angle(selfTransform.forward, toPlayer.normalized);
        //
        //         if (angleToPlayer <= (fieldOfViewAngle * 0.5f))
        //         {
        //             bool blocked =
        //                 Physics.Raycast(
        //                     origin,
        //                     toPlayer.normalized,
        //                     distanceToPlayer,
        //                     detectionMask,
        //                     QueryTriggerInteraction.Ignore);
        //
        //             if (!blocked)
        //             {
        //                 playerVisible = true;
        //             }
        //         }
        //     }
        //     inAttackRange = distanceToPlayer <= control.attackRange;
        // }

       if (control == null)
        return;

    // movement
    bool currentlyMoving = false;
    if (navMeshAgent != null && navMeshAgent.enabled)
    {
        currentlyMoving =
            !navMeshAgent.isStopped &&
            navMeshAgent.hasPath &&
            navMeshAgent.remainingDistance >
            (navMeshAgent.stoppingDistance + 0.05f);
    }
    control.isMoving = currentlyMoving;

    // player ranges
    bool hasPlayer = (control.playerTransform != null);
    float distanceToPlayer = hasPlayer
        ? Vector3.Distance(selfTransform.position, control.playerTransform.position)
        : float.MaxValue;

    bool playerInChaseRange = hasPlayer && distanceToPlayer <= control.chaseRange;
    bool inAttackRange      = hasPlayer && distanceToPlayer <= control.attackRange;

    // dodge
    UsableItem_Base explosive =
        control.FindNearestExplosiveThreat(control.explosiveSenseRadius);

    control.currentExplosiveThreat = explosive;

    bool hasExplosiveThreat = (explosive != null);

    bool hasDodgeTarget = false;
    if (hasExplosiveThreat)
    {
        Vector3 dodgePoint;
        if (control.TryFindBestDodgePoint(explosive, out dodgePoint))
        {
            hasDodgeTarget = true;
            control.currentDodgePoint = dodgePoint;
        }
    }

    // ally/crate
    bool allyAtCrate    = false;
    bool defendingCrate = control.isDefendingCrate;

    if (control.TryGetAllyCrateTarget(out NetworkedCrate crate))
    {
        allyAtCrate = true;
        control.allyCrateTarget = crate;
    }

    bool hasAlly          = control.TryFindSmartAlly();
    bool allyInTurretRange = false;
    bool inProtectionRange = false;

    if (hasAlly && control.smartAlly != null)
    {
        float distToAlly =
            Vector3.Distance(selfTransform.position, control.smartAlly.transform.position);

        inProtectionRange = distToAlly <= control.protectionRange;

        if (explosive != null)
        {
            float distanceAllyToThreat =
                Vector3.Distance(
                    control.smartAlly.transform.position,
                    explosive.transform.position);

            if (distanceAllyToThreat <= control.turretAssistRadius)
            {
                allyInTurretRange = true;
            }
        }
    }

    // player lowhealth check
    bool lowHealth = false;
    if (control.playerHealth != null)
    {
        float curHealth = control.playerHealth.currentHealth.Value;
        float fraction =
            curHealth / Mathf.Max(1f, control.playerHealth.maxHealth);
        lowHealth = fraction <= lowHealthFraction;
    }
    
    aWorldState.BeginUpdate(aAgent.planner);
    {
        aWorldState.Set((int)DangerousAlien.HasPlayer,          hasPlayer);
        aWorldState.Set((int)DangerousAlien.PlayerInChaseRange, playerInChaseRange);
        aWorldState.Set((int)DangerousAlien.ThreatNearby,       hasExplosiveThreat);
        aWorldState.Set((int)DangerousAlien.HasDodgeTarget,     hasDodgeTarget);
        aWorldState.Set((int)DangerousAlien.AllyAtCrate,        allyAtCrate);
        aWorldState.Set((int)DangerousAlien.DefendingCrate,     defendingCrate);
        aWorldState.Set((int)DangerousAlien.AllyInTurretRange,  allyInTurretRange);
        aWorldState.Set((int)DangerousAlien.NeedsScan,          control.needsScan);
        aWorldState.Set((int)DangerousAlien.IsMoving,           control.isMoving);
        aWorldState.Set((int)DangerousAlien.LowHealth,          lowHealth);
        aWorldState.Set((int)DangerousAlien.HasAlly,            hasAlly);
        aWorldState.Set((int)DangerousAlien.InProtectionRange,  inProtectionRange);
        aWorldState.Set((int)DangerousAlien.InAttackRange,  inAttackRange);
    }
    aWorldState.EndUpdate();
    }
}
