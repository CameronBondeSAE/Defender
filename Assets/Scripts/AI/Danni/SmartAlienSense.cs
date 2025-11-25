using System.Collections.Generic;
using UnityEngine;
using Anthill.AI;
using Defender;

public class SmartAlienSense : MonoBehaviour, ISense
{
    
    // private void Awake()
    // {
    //     control = GetComponent<SmartAlienControl>();
    //     transform = ((Component)this).transform;
    // }
    //
    // public void CollectConditions(AntAIAgent aAgent, AntAICondition aWorldState)
    // {
    //     // find nearest snack and threat item to make decisions
    //     UsableItem_Base nearestSnack = control.FindNearestItem(UsableItem_Base.ItemRoleForAI.Snack, control.snackSearchRadius);
    //     UsableItem_Base nearestThreat = control.FindNearestItem(UsableItem_Base.ItemRoleForAI.Threat, control.threatSearchRadius);
    //
    //     control.currentSnackTarget = nearestSnack;
    //     control.currentThreatTarget = nearestThreat;
    //     control.currentCrateTarget = control.FindNearestCrate();
    //
    //     float snackDist = (nearestSnack != null) 
    //         ? Vector3.Distance(transform.position, nearestSnack.transform.position)
    //         : float.MaxValue;
    //
    //     float threatDist = (nearestThreat != null)
    //         ? Vector3.Distance(transform.position, nearestThreat.transform.position)
    //         : float.MaxValue;
    //
    //     bool hasSnackItem = (control.heldItem != null && control.heldItem.RoleForAI == UsableItem_Base.ItemRoleForAI.Snack);
    //     bool threatNearby = (nearestThreat != null);
    //     bool hasCrate = (control.currentCrateTarget != null);
    //     bool threatCloserThanSnack = threatNearby && (threatDist < snackDist);
    //     
    //     aWorldState.BeginUpdate(aAgent.planner);
    //     {
    //         aWorldState.Set("Has Snack", hasSnackItem);
    //         aWorldState.Set("Threat Nearby", threatNearby);
    //         aWorldState.Set("Threat Closer Than Snack", threatCloserThanSnack);
    //         aWorldState.Set("Has Crate", hasCrate);
    //         
    //         // aWorldState.Set("Snack Deployed", snackDeployed);
    //         // aWorldState.Set("Has Civ Targets", hasCivTargets);
    //         // aWorldState.Set("Civs Delivered", civDelivered);
    //     }
    //     aWorldState.EndUpdate();
    // }
    
    private AntAIAgent agent;             
    private SmartAlienControl control;    
    private Transform selfTransform;    

    [Header("Threat")]
    public float threatSenseRadius = 20f;
    public LayerMask threatMask; 

    [Header("Debug Visuals")]
    public bool showDebugRays = true;  
    
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
        HasUsefulItem = 8
    }

    private void Awake()
    {
        agent         = GetComponent<AntAIAgent>();
        control       = GetComponent<SmartAlienControl>();
        selfTransform = transform;
    }
    private void OnDrawGizmosSelected() // for threat radius
    {
        if (!showDebugRays) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, threatSenseRadius);
    }
    
    public void CollectConditions(AntAIAgent aAgent, AntAICondition aWorldState)
    {
        if (control == null)
            control = GetComponent<SmartAlienControl>();
        if (selfTransform == null)
            selfTransform = transform;
        
        UsableItem_Base nearestSnack  =
            control.FindNearestItem(UsableItem_Base.ItemRoleForAI.Snack,  control.snackSearchRadius);
        UsableItem_Base nearestThreat =
            control.FindNearestItem(UsableItem_Base.ItemRoleForAI.Threat, control.threatSearchRadius);

        control.currentSnackTarget  = nearestSnack;
        control.currentThreatTarget = nearestThreat;
        control.currentCrateTarget  = control.FindNearestCrate();

        float snackDist = (nearestSnack != null)
            ? Vector3.Distance(selfTransform.position, nearestSnack.transform.position)
            : float.MaxValue;

        float threatDist = (nearestThreat != null)
            ? Vector3.Distance(selfTransform.position, nearestThreat.transform.position)
            : float.MaxValue;
        Vector3 crowdCenter;
        List<AIBase> civs;
        control.ComputeNearestCrowd(out crowdCenter, out civs);
        control.currentCrowdCenter = crowdCenter;
        control.currentCivGroup    = civs;
        

        bool hasSnackItem =
            (control.heldItem != null &&
             control.heldItem.RoleForAI == UsableItem_Base.ItemRoleForAI.Snack);

        bool snackDeployed = control.snackDeployed;

        bool hasCivTargets =
            (control.currentCivGroup != null &&
             control.currentCivGroup.Count > 0);
        
        bool civsDelivered = control.civsAtMothership;

        bool threatNearby = (nearestThreat != null &&
                             threatDist <= threatSenseRadius);

        bool threatCloserThanSnack =
            threatNearby && (threatDist < snackDist);

        bool hasCrate = (control.currentCrateTarget != null);

        bool nearCrate = false;
        if (hasCrate)
        {
            Vector3 cratePos = control.currentCrateTarget.transform.position;
            nearCrate = control.IsAgentNear(cratePos, control.interactRange);
        }

        bool hasUsefulItem =
            (control.heldItem != null &&
             control.heldItem.RoleForAI != UsableItem_Base.ItemRoleForAI.None);

        aWorldState.BeginUpdate(aAgent.planner);
        {
            aWorldState.Set((int)SmartAlien.HasSnack,             hasSnackItem);
            aWorldState.Set((int)SmartAlien.SnackDeployed,        snackDeployed);
            aWorldState.Set((int)SmartAlien.HasCivTargets,        hasCivTargets);
            aWorldState.Set((int)SmartAlien.CivsDelivered,        civsDelivered);
            aWorldState.Set((int)SmartAlien.ThreatNearby,         threatNearby);
            aWorldState.Set((int)SmartAlien.ThreatCloserThanSnack, threatCloserThanSnack);
            aWorldState.Set((int)SmartAlien.HasCrate,             hasCrate);
            aWorldState.Set((int)SmartAlien.NearCrate,            nearCrate);
            aWorldState.Set((int)SmartAlien.HasUsefulItem,        hasUsefulItem);
        }
        aWorldState.EndUpdate();
    }
}
