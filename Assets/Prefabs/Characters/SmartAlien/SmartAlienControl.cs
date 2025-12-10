using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Anthill.AI;
using Defender;
using mothershipScripts;
using Unity.Netcode;
using AIAnimation;

public class SmartAlienControl : CharacterBase
{
    public NavMeshAgent agent;
    public SmartAlienSfx sfx;
    [SerializeField] AIAnimationController animController;
    private AIAnimationController.AnimationState currentAnimState;
    
    [Header("Decision Settings")]
    public Transform mothershipDropPoint;
    public float mothershipNavmeshSampleRadius = 5f;
    public float threatSearchRadius;
    public float escortThreatInterruptRadius = 10f; 
    public float snackSearchRadius;
    public float crateSearchRadius;
    public float interactRange = 2.0f;
    public int minCivCrowdSize = 3;
    public float civCrowdRadius = 5f;

    [Header("Debugs")]
    public UsableItem_Base heldItem;              
    public UsableItem_Base currentThreatTarget;
    public UsableItem_Base currentSnackTarget;
    public NetworkedCrate  currentCrateTarget;
    public Vector3         currentCrowdCenter;
    public List<AIBase>    currentCivGroup = new List<AIBase>();
    
    [Header("Held Item Settings")]  
    public Transform itemHoldPoint;
    
    [Header("Planner Flags)")]
    public bool snackDeployed;
    public bool escortInProgress;
    public bool civsAtMothership;
    public bool needsScan = true;
    public float scanDuration;
    public bool isMoving;
    
    [Header("Crate Pickup Settings")]
    public float crateNearRadius    = 2f;
    public float crateDestroyDistance = 5f;
    
    [Header("Movement Settings")]
    public float escortMoveSpeed = 5f;

    private void Awake()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
        needsScan       = true;
        civsAtMothership = false;
        escortInProgress = false;
        snackDeployed    = false;
        FindMothership(); 
        animController = GetComponentInChildren<AIAnimationController>();
        if (sfx == null)
        {
            sfx = GetComponent<SmartAlienSfx>();
        }
    }
    
    private void Update()
    {
        if (agent == null || agent.isActiveAndEnabled || !agent.enabled || animController == null)
        {
            return;
        }
        bool actuallyMoving =
            !agent.isStopped &&
            agent.hasPath &&
            agent.remainingDistance > (agent.stoppingDistance + 0.05f);
        var desiredState = actuallyMoving
            ? AIAnimationController.AnimationState.Walk
            : AIAnimationController.AnimationState.Idle;
       animController.SetAnimation(desiredState);
    }
    
    // init item holding but does NOT call IPickup.Pickup; that's done by the crate or state
    public void OnItemPickedUp(UsableItem_Base item)
    {
        heldItem = item;
        if (item == null) return;

        // find the actual netobj
        NetworkObject itemNO = item.GetComponent<NetworkObject>();
        NetworkObject myNO   = GetComponent<NetworkObject>();

        // the transform where we want the item to visually sit
        Transform visualTarget = (itemHoldPoint != null) ? itemHoldPoint : transform;

        if (itemNO != null && myNO != null && myNO.IsSpawned && itemNO.IsSpawned)
        {
            // parent under his netobj rather than holdpos
            bool parentOk = itemNO.TrySetParent(myNO);

            if (!parentOk)
            {
                Debug.LogWarning($"alien setParent failed for {itemNO.name}");
            }
            item.transform.position = visualTarget.position;
            item.transform.rotation = visualTarget.rotation;
            item.transform.localScale = Vector3.one;
        }
        else
        {
            // Fallback for non-networked items (or if something is not spawned yet)
            item.transform.SetParent(visualTarget, false);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
        }
    }

    #region Helpers (to find crate, item, civ, mothership)
    
    private void FindMothership()
    {
        if (mothershipDropPoint != null)
        {
            return;
        }

        // find the first MothershipBase in the scene
        MothershipDropZone mothership = FindObjectOfType<MothershipDropZone>();
        if (mothership != null)
        {
            mothershipDropPoint = mothership.transform;
            Debug.Log($"auto-assigned mothershipDropPoint to {mothership.name}");
        }
        else
        {
            Debug.LogWarning("no MothershipBase found in scene");
        }
    }

    public UsableItem_Base FindNearestItem(UsableItem_Base.ItemRoleForAI role, float maxRadius)
    {
        UsableItem_Base[] allItems = FindObjectsOfType<UsableItem_Base>();
        UsableItem_Base best = null;
        float bestDistSqr = maxRadius * maxRadius;
        Vector3 pos = transform.position;

        for (int i = 0; i < allItems.Length; i++)
        {
            UsableItem_Base item = allItems[i];
            if (item == null) continue;
            if (item.RoleForAI != role) continue;

            float dSqr = (item.transform.position - pos).sqrMagnitude;
            if (dSqr < bestDistSqr)
            {
                best = item;
                bestDistSqr = dSqr;
            }
        }

        return best;
    }

    public NetworkedCrate FindNearestCrate()
    {
        NetworkedCrate[] crates = FindObjectsOfType<NetworkedCrate>();
        NetworkedCrate best = null;
        float bestDistSqr = crateSearchRadius * crateSearchRadius;
        Vector3 pos = transform.position;

        for (int i = 0; i < crates.Length; i++)
        {
            NetworkedCrate crate = crates[i];
            if (crate == null) continue;

            float dSqr = (crate.transform.position - pos).sqrMagnitude;
            if (dSqr < bestDistSqr)
            {
                best = crate;
                bestDistSqr = dSqr;
            }
        }

        return best;
    }

    public void FindNearestCrowd(out Vector3 crowdCenter, out List<AIBase> crowdMembers)
    {
        crowdCenter = Vector3.zero;
        crowdMembers = new List<AIBase>();

        AIBase[] allCivs = FindObjectsOfType<AIBase>();
        if (allCivs == null || allCivs.Length == 0)
        {
            return;
        }

        // crowd finder using the score thing I had in project 2 for ambush points, pick the point that has most civs in its vicinity
        int bestCount = 0;
        Vector3 bestCenter = Vector3.zero;

        for (int i = 0; i < allCivs.Length; i++)
        {
            AIBase civ = allCivs[i];
            if (civ == null) continue;

            Vector3 center = civ.transform.position;
            int count = 0;

            for (int j = 0; j < allCivs.Length; j++)
            {
                AIBase other = allCivs[j];
                if (other == null) continue;

                float dist = Vector3.Distance(center, other.transform.position);
                if (dist <= civCrowdRadius)
                {
                    count++;
                }
            }

            if (count > bestCount)
            {
                bestCount = count;
                bestCenter = center;
            }
        }

        if (bestCount >= minCivCrowdSize)
        {
            crowdCenter = bestCenter;
            // collect the civs
            for (int i = 0; i < allCivs.Length; i++)
            {
                AIBase civ = allCivs[i];
                if (civ == null) continue;

                float dist = Vector3.Distance(bestCenter, civ.transform.position);
                if (dist <= civCrowdRadius)
                {
                    crowdMembers.Add(civ);
                }
            }
        }
    }

    public bool IsAgentNear(Vector3 targetPos, float range)
    {
        float dist = Vector3.Distance(transform.position, targetPos);
        return dist <= range;
    }
    
    public Vector3 GetCrateDisposePosition()
    {
        if (currentCrateTarget == null)
        {
            return transform.position;
        }
        Vector3 cratePos = currentCrateTarget.transform.position;
        Vector3 dir      = transform.position - cratePos;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f)
        {
            dir = transform.forward;
        }
        dir.Normalize();
        return cratePos + dir * crateDestroyDistance;
    }
    
    // need to find an actually reachable position to use as motherhship drop pos, since it's ignored by navmesh
    public Vector3 GetMothershipDestination()
    {
        if (mothershipDropPoint == null || agent == null)
        {
            return transform.position;
        }

        Vector3 target = mothershipDropPoint.position;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(
                target,
                out hit,
                mothershipNavmeshSampleRadius,
                agent.areaMask))
        {
            return hit.position;   
        }
        return target;
    }

    public bool IsAgentNearCrate(NetworkedCrate crate)
    {
        if (crate == null) return false;

        float range = (crateNearRadius > 0f)
            ? crateNearRadius
            : interactRange;

        return IsAgentNear(crate.transform.position, range);
    }
    public Vector3 GetCrateApproachPosition()
    {
        if (currentCrateTarget == null)
        {
            return transform.position;
        }

        Vector3 cratePos = currentCrateTarget.transform.position;
        
        Vector3 dir = transform.position - cratePos;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
        {
            dir = transform.forward;
        }
        dir.Normalize();
        float distFromCrate = 1.0f;
        return cratePos + dir * distFromCrate;
    }
    
    #endregion
}
