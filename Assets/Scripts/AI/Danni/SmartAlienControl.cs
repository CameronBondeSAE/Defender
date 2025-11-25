using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Anthill.AI;
using Defender;

public class SmartAlienControl : CharacterBase
{
    public NavMeshAgent agent;

    [Header("Mothership & Civ Settings")]
    public Transform mothershipDropPoint;
    public float threatSearchRadius = 20f;
    public float snackSearchRadius = 30f;
    public float crateSearchRadius = 30f;
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
    }
    
    // init item holding but does NOT call IPickup.Pickup; that's done by the crate or state
    public void OnItemPickedUp(UsableItem_Base item)
    {
        heldItem = item;
        if (item == null) return;

        if (itemHoldPoint != null)
        {
            item.transform.SetParent(itemHoldPoint, false);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
        }
    }

    #region Helpers (to find crate, item, civ)

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

    #endregion
}
