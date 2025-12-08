using Anthill.AI;
using UnityEngine;
using UnityEngine.AI;

public class DangerousAlienSense : MonoBehaviour, ISense
{
     private DangerousAlienControl control;
    private Transform selfTransform;
    private NavMeshAgent navMeshAgent;
    private AntAIAgent antAgent;

    [Header("Detection")]
    public float playerVisionRange = 20f;
    public float playerAttackRange = 3f;
    public float fieldOfViewAngle = 120f;
    public int fieldOfViewRayCount = 7;
    public LayerMask detectionMask;
    
    public float lowHealthFraction = 0.3f;

    public enum DangerousAlien
    {
        PlayerVisible      = 0,
        InAttackRange      = 1,
        HasExplosiveThreat = 2,
        HasDodgeTarget     = 3,
        AllyAtCrate        = 4,
        DefendingCrate     = 5,
        AllyInTurretRange  = 6,
        NeedsScan          = 7,
        IsMoving           = 8,
        LowHealth          = 9
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
        if (control == null)
        {
            return;
        }
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
        bool playerVisible = false;
        bool inAttackRange = false;

        Transform player = control.playerTransform;

        if (player != null)
        {
            Vector3 origin = selfTransform.position + Vector3.up * 1.1f;
            Vector3 toPlayer = player.position - origin;
            float distanceToPlayer = toPlayer.magnitude;

            if (distanceToPlayer <= playerVisionRange)
            {
                float angleToPlayer =
                    Vector3.Angle(selfTransform.forward, toPlayer.normalized);

                if (angleToPlayer <= (fieldOfViewAngle * 0.5f))
                {
                    bool blocked =
                        Physics.Raycast(
                            origin,
                            toPlayer.normalized,
                            distanceToPlayer,
                            detectionMask,
                            QueryTriggerInteraction.Ignore);

                    if (!blocked)
                    {
                        playerVisible = true;
                    }
                }
            }

            inAttackRange = distanceToPlayer <= control.attackRange;
        }
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
            }
        }
        bool allyAtCrate = false;
        bool defendingCrate = control.isDefendingCrate;

        if (control.TryGetAllyCrateTarget(out NetworkedCrate crate))
        {
            allyAtCrate = true;
        }
        bool allyInTurretRange = false;
        if (control.TryFindSmartAlly() && explosive != null)
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
        bool lowHealth = false;
        if (control.health != null)
        {
            float curHealth = control.health.currentHealth.Value;
            float fraction =
                curHealth /
                Mathf.Max(1f, control.health.maxHealth);
            lowHealth = fraction <= lowHealthFraction;
        }

        aWorldState.BeginUpdate(aAgent.planner);
        {
            aWorldState.Set((int)DangerousAlien.PlayerVisible,      playerVisible);
            aWorldState.Set((int)DangerousAlien.InAttackRange,      inAttackRange);
            aWorldState.Set((int)DangerousAlien.HasExplosiveThreat, hasExplosiveThreat);
            aWorldState.Set((int)DangerousAlien.HasDodgeTarget,     hasDodgeTarget);
            aWorldState.Set((int)DangerousAlien.AllyAtCrate,        allyAtCrate);
            aWorldState.Set((int)DangerousAlien.DefendingCrate,     defendingCrate);
            aWorldState.Set((int)DangerousAlien.AllyInTurretRange,  allyInTurretRange);
            aWorldState.Set((int)DangerousAlien.NeedsScan,          control.needsScan);
            aWorldState.Set((int)DangerousAlien.IsMoving,           control.isMoving);
            aWorldState.Set((int)DangerousAlien.LowHealth,          lowHealth);
        }
        aWorldState.EndUpdate();
    }
}
