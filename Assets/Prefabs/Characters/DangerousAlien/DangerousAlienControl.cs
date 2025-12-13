using AIAnimation;
using Defender;
using UnityEngine;
using UnityEngine.AI;

public class DangerousAlienControl : CharacterBase
{
    public NavMeshAgent navMeshAgent;
    public PlayerHealth playerHealth;

    public float scanDuration = 1f;

    [Header("Anim")]
    public AIAnimationController animationController;
    private AIAnimationController.AnimationState currentAnimationState;

    [Header("Attack Settings")]
    public float damageMin = 5f;
    public float damageMax = 10f;
    public float attackRange = 2f;
    public float chaseRange = 15f;
    public float attackCooldownSeconds = 3f;
    public float attackDurationSeconds = 0.8f;
    public GameObject attackHitboxPrefab;     
    public Transform attackHitboxRoot;     
    [HideInInspector] public GameObject attackHitboxInstance;

    [Header("Dodge Settings")]
    public float explosiveSenseRadius = 10f;  
    public float dodgeSampleRadius = 6f;
    public int dodgeSampleCount = 16;
    public float dodgeMoveSpeed = 8f;
    public float normalMoveSpeed = 4f;
    public float dodgeArrivalTolerance = 0.3f;
    public Vector3 currentDodgePoint;

    [Header("Teamwork Settings")]
    public float crateDefendRadius = 8f;
    public float allyAssistRadius = 12f;
    public float turretAssistRadius = 18f;
    public float accompanyDistance = 15;
    public float protectionRange = 30f;
    
    public Transform playerTransform;
    public UsableItem_Base currentExplosiveThreat;
    public Vector3 currentDodgeTarget;
    public bool hasDodgeTarget;
    public bool isDodging;
    public bool isDefendingCrate;

    public SmartAlienControl smartAlly;
    public NetworkedCrate allyCrateTarget;
    
    public bool needsScan = true;
    public bool isMoving;

    private float nextAttackTime;
    
    public bool HasAlly => smartAlly != null;

    private void Start()
    {

        if (navMeshAgent == null)
            navMeshAgent = GetComponent<NavMeshAgent>();

        if (animationController == null)
            animationController = GetComponentInChildren<AIAnimationController>();

        if (navMeshAgent != null)
            normalMoveSpeed = navMeshAgent.speed;
        SetupAttackHitboxInstance();
    }
    private void SetupAttackHitboxInstance()
    {
        if (attackHitboxInstance != null)
        {
            return;
        }

        if (attackHitboxPrefab != null && attackHitboxRoot != null)
        {
            attackHitboxInstance = Instantiate(
                attackHitboxPrefab,
                attackHitboxRoot.position,
                attackHitboxRoot.rotation,
                attackHitboxRoot);

            attackHitboxInstance.SetActive(false);

            DangerousAlienHitBox hitBox = attackHitboxInstance.GetComponent<DangerousAlienHitBox>();
            if (hitBox != null)
            {
                hitBox.owner = this;
            }
        }
    }

    private void Update()
    {
        if (smartAlly == null)
            smartAlly = FindObjectOfType<SmartAlienControl>();
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            playerHealth = playerObj.GetComponent<PlayerHealth>();
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }
        if (navMeshAgent == null || !navMeshAgent.enabled || animationController == null)
        {
            return;
        }

        bool actuallyMoving =
            !navMeshAgent.isStopped &&
            navMeshAgent.hasPath &&
            navMeshAgent.remainingDistance > (navMeshAgent.stoppingDistance + 0.05f);

        isMoving = actuallyMoving;

        AIAnimationController.AnimationState desired =
            actuallyMoving ? AIAnimationController.AnimationState.Walk
                           : AIAnimationController.AnimationState.Idle;

        if (desired != currentAnimationState)
        {
            currentAnimationState = desired;
            animationController.SetAnimation(desired);
        }
    }

    #region Attack Helpers

    public bool CanAttackNow()
    {
        return Time.time >= nextAttackTime;
    }

    public void MarkAttackUsed()
    {
        nextAttackTime = Time.time + attackCooldownSeconds;
    }

    public float RollAttackDamage()
    {
        return Random.Range(damageMin, damageMax);
    }

    #endregion

    #region Dodge Helpers

    public UsableItem_Base FindNearestExplosiveThreat(float maxRadius)
    {
        UsableItem_Base[] allItems = FindObjectsOfType<UsableItem_Base>();
        UsableItem_Base best = null;

        float maxRadiusSquared = maxRadius * maxRadius;
        Vector3 origin = transform.position;

        int i = 0;
        while (i < allItems.Length)
        {
            UsableItem_Base item = allItems[i];
            if (item != null)
            {
                if (item.RoleForAI == UsableItem_Base.ItemRoleForAI.Threat)
                {
                    bool isActiveThreat =
                        item.IsActivated ||
                        item.IsCountdownActive ||
                        item.IsExpiryActive;

                    if (isActiveThreat)
                    {
                        float distanceSquared =
                            (item.transform.position - origin).sqrMagnitude;

                        if (distanceSquared < maxRadiusSquared)
                        {
                            maxRadiusSquared = distanceSquared;
                            best = item;
                        }
                    }
                }
            }

            i = i + 1;
        }

        return best;
    }

    public bool TryFindBestDodgePoint(UsableItem_Base explosive, out Vector3 bestPoint)
    {
        bestPoint = transform.position;
        hasDodgeTarget = false;

        if (explosive == null || navMeshAgent == null)
        {
            return false;
        }

        Vector3 explosivePos = explosive.transform.position;
        Vector3 origin = transform.position;

        float bestScore = float.MinValue;
        Vector3 candidatePoint = origin;

        int i = 0;
        while (i < dodgeSampleCount)
        {
            float angle = (360f / dodgeSampleCount) * i;
            float radians = angle * Mathf.Deg2Rad;

            Vector3 direction =
                new Vector3(Mathf.Cos(radians), 0f, Mathf.Sin(radians));

            Vector3 rawPoint = origin + direction * dodgeSampleRadius;

            NavMeshHit hit;
            bool validOnNavmesh =
                NavMesh.SamplePosition(
                    rawPoint,
                    out hit,
                    1.5f,
                    navMeshAgent.areaMask);

            if (validOnNavmesh)
            {
                Vector3 navPoint = hit.position;
                float distanceFromExplosive =
                    Vector3.Distance(navPoint, explosivePos);

                float distanceFromCurrent =
                    Vector3.Distance(navPoint, origin);

                float distanceScore = distanceFromExplosive;
                float moveCost = distanceFromCurrent * 0.5f;

                float score = distanceScore - moveCost;

                if (score > bestScore)
                {
                    bestScore = score;
                    candidatePoint = navPoint;
                }
            }

            i = i + 1;
        }

        if (bestScore > float.MinValue)
        {
            bestPoint = candidatePoint;
            hasDodgeTarget = true;
            currentDodgeTarget = candidatePoint;
            return true;
        }

        return false;
    }

    #endregion

    #region Teamwork Helpers

    public bool TryFindSmartAlly()
    {
        if (smartAlly != null)
        {
            return true;
        }
        smartAlly = FindObjectOfType<SmartAlienControl>();
        return smartAlly != null;
    }

    public bool TryGetAllyCrateTarget(out NetworkedCrate crate)
    {
        crate = null;

        if (!TryFindSmartAlly())
        {
            return false;
        }

        crate = smartAlly.currentCrateTarget;
        allyCrateTarget = crate;

        if (crate == null)
        {
            return false;
        }

        float distanceToCrate =
            Vector3.Distance(transform.position, crate.transform.position);

        return distanceToCrate <= crateDefendRadius;
    }
    

    public float DistanceToAlly()
    {
        if (!HasAlly) return float.MaxValue;
        return Vector3.Distance(transform.position, smartAlly.transform.position);
    }

    public bool IsWithinAccompanyDistance()
    {
        if (!HasAlly) return false;
        return DistanceToAlly() <= accompanyDistance;
    }

    public bool IsInsideProtectionRange()
    {
        if (!HasAlly) return false;
        return DistanceToAlly() <= protectionRange;
    }


    #endregion
}
