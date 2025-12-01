using System.Collections.Generic;
using UnityEngine;
using Anthill.AI;
using Defender;
using UnityEngine.AI;

public class SmartAlienSense : MonoBehaviour, ISense
{
   private SmartAlienControl control;
    private Transform selfTransform;
    private AntAIAgent agent;
    private NavMeshAgent navAgent;
    private CharacterBase character;

    [Header("Threat Sensing")]
    public float threatSenseRadius = 20f;  
    public LayerMask threatMask;

    [Header("Debug Visuals")]
    public bool showDebugRays = false;
    public enum SmartAlien
    {
        HasSnack = 0,
        SnackDeployed = 1,
        HasCivTargets = 2,
        CivsDelivered = 3,
        ThreatNearby = 4,
        ThreatCloserThanSnack = 5,
        HasCrate = 6,
        NearCrate = 7,
        HasUsefulItem = 8,
        EscortInProgress = 9,
        NeedsScan = 10,
        IsMoving = 11,
        HasSnackTarget = 12
    }

    private void Awake()
    {
        control       = GetComponent<SmartAlienControl>();
        selfTransform = transform;
        agent         = GetComponent<AntAIAgent>();
        navAgent      = GetComponent<NavMeshAgent>();
        character     = GetComponent<CharacterBase>();
    }
    public void CollectConditions(AntAIAgent aAgent, AntAICondition aWorldState)
    {
        if (control == null)
        {
            return;
        }
        // movement
        bool currentlyMoving = false;
        if (navAgent != null && navAgent.enabled)
        {
            currentlyMoving =
                !navAgent.isStopped &&
                navAgent.hasPath &&
                navAgent.remainingDistance > navAgent.stoppingDistance + 0.05f;
        }
        control.isMoving = currentlyMoving;
        
        // snack sensing
        bool hasSnackItem =
            (control.heldItem != null &&
             control.heldItem.RoleForAI == UsableItem_Base.ItemRoleForAI.Snack);
        bool hasUsefulItem = (control.heldItem != null);
        
        // scan world for things
        UsableItem_Base nearestSnack  = control.FindNearestItem(
            UsableItem_Base.ItemRoleForAI.Snack,
            control.snackSearchRadius);
        
        // pick radius for making decision regarding threat (normal vs when escorting)
        float threatRadius = control.threatSearchRadius;
        if (control.escortInProgress && control.escortThreatInterruptRadius > 0f)
        {
            threatRadius = control.escortThreatInterruptRadius;
        }

        UsableItem_Base nearestThreat = FindNearestActiveThreat(threatRadius);
        NetworkedCrate nearestCrate   = control.FindNearestCrate();
        control.currentSnackTarget  = nearestSnack;
        control.currentThreatTarget = nearestThreat;
        control.currentCrateTarget  = nearestCrate;
        bool hasSnackTarget = (nearestSnack != null); 

        // looking for civ crowd
        Vector3 crowdCenter;
        System.Collections.Generic.List<AIBase> crowdMembers;
        control.FindNearestCrowd(out crowdCenter, out crowdMembers);
        control.currentCrowdCenter = crowdCenter;
        control.currentCivGroup    = crowdMembers;
        
        float snackDist = (nearestSnack != null)
            ? Vector3.Distance(selfTransform.position, nearestSnack.transform.position)
            : float.MaxValue;

        float threatDist = (nearestThreat != null)
            ? Vector3.Distance(selfTransform.position, nearestThreat.transform.position)
            : float.MaxValue;

        bool threatNearby          = (nearestThreat != null);
        bool threatCloserThanSnack = threatNearby && (threatDist <= snackDist);
        bool hasCrate  = (nearestCrate != null);
        bool nearCrate = false;

        if (hasCrate)
        {
            nearCrate = control.IsAgentNearCrate(nearestCrate);
        }

        bool hasCivTargets =
            (control.currentCivGroup != null &&
             control.currentCivGroup.Count > 0);

        // if escorting civs, ignore new threat/crates/items and stick to job
        if (control.escortInProgress)
        {
            // threatNearby          = false;
            // threatCloserThanSnack = false;
            hasCrate              = false;
            nearCrate             = false;
            hasUsefulItem         = hasSnackItem;
        }

        bool civsDelivered = control.civsAtMothership;
        
        // debug visuals
        if (showDebugRays && nearestThreat != null)
        {
            Debug.DrawLine(
                selfTransform.position,
                nearestThreat.transform.position,
                Color.red);
        }

        if (showDebugRays && nearestSnack != null)
        {
            Debug.DrawLine(
                selfTransform.position,
                nearestSnack.transform.position,
                Color.green);
        }
        aWorldState.BeginUpdate(aAgent.planner);
        {
            aWorldState.Set((int)SmartAlien.HasSnack,           hasSnackItem);
            aWorldState.Set((int)SmartAlien.SnackDeployed,      control.snackDeployed);
            aWorldState.Set((int)SmartAlien.HasCivTargets,      hasCivTargets);
            aWorldState.Set((int)SmartAlien.CivsDelivered,      civsDelivered);
            aWorldState.Set((int)SmartAlien.ThreatNearby,       threatNearby);
            aWorldState.Set((int)SmartAlien.ThreatCloserThanSnack, threatCloserThanSnack);
            aWorldState.Set((int)SmartAlien.HasCrate,           hasCrate);
            aWorldState.Set((int)SmartAlien.NearCrate,          nearCrate);
            aWorldState.Set((int)SmartAlien.HasUsefulItem,      hasUsefulItem);
            aWorldState.Set((int)SmartAlien.EscortInProgress,   control.escortInProgress);
            aWorldState.Set((int)SmartAlien.NeedsScan,          control.needsScan);
            aWorldState.Set((int)SmartAlien.IsMoving,           control.isMoving);
            aWorldState.Set((int)SmartAlien.HasSnackTarget,        hasSnackTarget);
        }
        aWorldState.EndUpdate();
    }
    
    // helper to find the nearest threat in radius
    private UsableItem_Base FindNearestActiveThreat(float maxRadius)
    {
        if (control == null)
        {
            return null;
        }

        UsableItem_Base[] allItems = FindObjectsOfType<UsableItem_Base>();

        UsableItem_Base best    = null;
        float           maxSqr  = maxRadius * maxRadius;
        Vector3         origin  = selfTransform.position;

        for (int i = 0; i < allItems.Length; i++)
        {
            UsableItem_Base item = allItems[i];
            if (item == null)
            {
                continue;
            }

            if (item.RoleForAI != UsableItem_Base.ItemRoleForAI.Threat)
            {
                continue;
            }

            // only count it as a threat if it is actually ACTIVE
            bool isActiveThreat =
                item.IsActivated ||      // has gone off at least once
                item.IsCountdownActive || 
                item.IsExpiryActive;

            if (!isActiveThreat)
            {
                continue;
            }

            float distSqr = (item.transform.position - origin).sqrMagnitude;
            if (distSqr < maxSqr)
            {
                best   = item;
                maxSqr = distSqr;
            }
        }
        return best;
    }
}
