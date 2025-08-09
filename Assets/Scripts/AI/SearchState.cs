using UnityEngine;
using AIAnimation;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// AIs who enter this state will search the game for civs and goes out to catch them.
/// </summary>
public class SearchState : IAIState
{
   private AlienAI ai;
    private AIAnimationController animController;
    private bool isGrabbing = false;
    private bool isBusy = false;
    private float grabTimeout = 10f;
    private float grabStartTime;
    private float lastMoveUpdate = 0f;
    private float moveUpdateInterval = 0.5f;

    private HashSet<AIBase> ignoredCivs = new HashSet<AIBase>();

    // Constructor that assigns the AlienAI controller reference
    public SearchState(AlienAI ai)
    {
        this.ai = ai;
    }
    // When entering this state — finds the nearest civilian and starts moving towards them
    public void Enter()
    {
        // // If already has a civilian that's being escorted, go straight to return
        // if (ai.currentTargetCiv != null && ai.currentTargetCiv.IsAbducted && ai.currentTargetCiv.escortingAlien == ai)
        // {
        //     ai.ChangeState(new ReturnState(ai));
        //     return;
        // }
        // if (ai.currentTargetCiv != null)
        //     return; // already has civ, do not look for more
        //
        // animController = ai.agent.gameObject.GetComponentInChildren<AIAnimationController>();
        // isGrabbing = false;
        // isBusy = false;
        //
        // animController.SetAnimation(AIAnimationController.AnimationState.Walk);
        //
        // // Find all civilians
        // GameObject[] civObjects = GameObject.FindGameObjectsWithTag("Civilian");
        // if (civObjects.Length == 0)
        // {
        //     ai.ChangeState(new PatrolState(ai)); // Go back to patrol if no civs are found
        //     return;
        // }
        //
        // GameObject closestCiv = null;
        // float closestDistance = float.MaxValue;
        //
        // // Loop through all civs to find the closest one
        // foreach (var civObj in civObjects)
        // {
        //     AIBase civ = civObj.GetComponent<AIBase>();
        //     if (civ == null || ignoredCivs.Contains(civ) || civ.IsAbducted) // Skip ignored or null civs
        //         continue;
        //
        //     float distance = Vector3.Distance(ai.transform.position, civ.transform.position);
        //     if (distance < closestDistance)
        //     {
        //         closestCiv = civObj;
        //         closestDistance = distance;
        //     }
        // }
        //
        // // Move towards the selected civilian target
        // if (closestCiv != null)
        // {
        //     AIBase civBase = closestCiv.GetComponent<AIBase>();
        //     ai.currentTargetCiv = civBase;
        //     ai.MoveTo(closestCiv.transform.position);
        // }
        
        //Debug.Log("[Search State] {ai.name} is entering search state.");
        if (ai.currentTargetCiv != null && ai.currentTargetCiv.IsAbducted && ai.currentTargetCiv.escortingAlien == ai)
        {
            //Debug.Log($"[SearchState] {ai.name} already escorting, switching to return");
            ai.ChangeState(new ReturnState(ai));
            return;
        }
        ai.currentTargetCiv = null;
        animController = ai.agent.gameObject.GetComponentInChildren<AIAnimationController>();
        isGrabbing = false;
        isBusy = false;
        animController.SetAnimation(AIAnimationController.AnimationState.Walk);
        FindNewTarget();
    }

    // Continuously called to keep moving towards the target and check for grabbing conditions
    public void Stay()
    {
        if (isGrabbing || isBusy)
        {
            if (isGrabbing && Time.time >= grabStartTime + grabTimeout)
            {
                //Debug.Log($"[SearchState] {ai.name} has timed out"); 
                AbandonCurrentTargetAndSearchNew();
            }

            return;
        }

        if (ai.currentTargetCiv == null)
        {
            FindNewTarget();
            return;
        }
        
        // check if current civ is taken by another alien
        if (ai.currentTargetCiv.IsAbducted && ai.currentTargetCiv.escortingAlien != ai)
        {
            //Debug.Log($"[SearchState] {ai.name} target taken by another alien");
            ai.currentTargetCiv = null;
            FindNewTarget();
            return;
        }
        float distanceToTarget = Vector3.Distance(ai.transform.position, ai.currentTargetCiv.transform.position);
        // update movement less frequently see if reduces conflict..?
        if (Time.time - lastMoveUpdate > moveUpdateInterval)
        {
            ai.MoveTo(ai.currentTargetCiv.transform.position);
            lastMoveUpdate = Time.time;
        }

        if (distanceToTarget <= ai.tagDistance)
        {
            isGrabbing = true;
            isBusy = true;
            grabStartTime = Time.time;
            ai.StartCoroutine(GrabThenReturn());
        }
    }

    // Called when exiting this state
    public void Exit()
    {
        isGrabbing = false;
        isBusy = false;
        ai.ResumeMoving();
    }

    private void FindNewTarget()
    {
        GameObject[] civObjects = GameObject.FindGameObjectsWithTag("Civilian");
        if (civObjects.Length == 0)
        {
            //Debug.Log($"[SearchState {ai.name}] No Civilian found");
            ai.ChangeState(new PatrolState(ai));
            return;
        }
        GameObject closestCiv = null;
        float closestDistance = float.MaxValue;
        foreach (var civObj in civObjects)
        {
            AIBase civ = civObj.GetComponent<AIBase>();
            if (civ == null || ignoredCivs.Contains(civ) || civ.IsAbducted)
                continue;
            float distanceToTarget = Vector3.Distance(ai.transform.position, civ.transform.position);
            if (distanceToTarget < closestDistance)
            {
                closestCiv = civObj;
                closestDistance = distanceToTarget;
            }
        }
        if(closestCiv != null)
        {
            AIBase civBase = closestCiv.GetComponent<AIBase>();
            ai.currentTargetCiv = civBase;
            //Debug.Log($"[SearchState {ai.name}] Found civilian {closestCiv}");
            ai.MoveTo(closestCiv.transform.position);
        }
        else
        {
            //Debug.Log($"[SearchState {ai.name}] No Civilian found, now patrolling");
            ai.ChangeState(new PatrolState(ai));
        }
    }

    // Coroutine that handles grabbing the civilian, playing animation, then changing state
    private IEnumerator GrabThenReturn()
    {
        //Debug.Log("[SearchState] {ai.name} starting grab sequence");
        // If already escorting, don't grab another ====
        if (ai.currentTargetCiv != null && ai.currentTargetCiv.IsAbducted)
        {
            ai.ChangeState(new ReturnState(ai));
            isBusy = false;
            isGrabbing = false;
            yield break;
        }
        ai.StopMoving();
        ai.currentTargetCiv.StopMoving();
        animController.SetAnimation(AIAnimationController.AnimationState.Grab);
        yield return new WaitForSeconds(0.8f); // Simulate grab duration

        // only grab if not already abducted or escorted by another alien,
        // and this alien isn't already escorting anyone
        if (ai.currentTargetCiv != null &&
            !ai.currentTargetCiv.IsAbducted &&
            ai.currentTargetCiv.escortingAlien == null &&
            ai.currentTargetCiv != ai) // Just in case!
        {
            ai.currentTargetCiv.SetAbducted(true); // mark instantly
            ai.currentTargetCiv.escortingAlien = ai; // record leader alien
            ai.currentTargetCiv.ChangeState(new FollowState(ai.currentTargetCiv, ai.transform));
            //Debug.Log($"[AlienAI] {ai.name} has grabbed {ai.currentTargetCiv.name}");
            ai.ChangeState(new ReturnState(ai));
        }
        else
        {
            ai.currentTargetCiv = null; // Clear target
            //ai.ChangeState(new SearchState(ai));
            FindNewTarget();
        }
        isBusy = false;
        isGrabbing = false;
    }

    // Failsafe: abandons the current civilian target and looks for a new one
    private void AbandonCurrentTargetAndSearchNew()
    {
        if (ai.currentTargetCiv != null)
        {
            ignoredCivs.Add(ai.currentTargetCiv); // Mark this civ as ignored
            ai.currentTargetCiv = null;
        }
        isGrabbing = false;
        isBusy = false;
        FindNewTarget();

        // GameObject[] civObjects = GameObject.FindGameObjectsWithTag("Civilian");
        // GameObject closestCiv = null;
        // float closestDistance = float.MaxValue;
        //
        // // Same search loop as Enter()
        // foreach (var civObj in civObjects)
        // {
        //     AIBase civ = civObj.GetComponent<AIBase>();
        //     if (civ == null || ignoredCivs.Contains(civ) || civ.IsAbducted)
        //         continue;
        //
        //     float distance = Vector3.Distance(ai.transform.position, civ.transform.position);
        //     if (distance < closestDistance)
        //     {
        //         closestCiv = civObj;
        //         closestDistance = distance;
        //     }
        // }
        //
        // if (closestCiv != null)
        // {
        //     AIBase civBase = closestCiv.GetComponent<AIBase>();
        //     ai.currentTargetCiv = civBase;
        //     ai.MoveTo(closestCiv.transform.position);
        // }
        // else
        // {
        //     ai.ChangeState(new PatrolState(ai));
        // }
    }
}


