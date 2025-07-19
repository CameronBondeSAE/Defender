using UnityEngine;
using AIAnimation;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// AIs who enter this state will search the game for civs and goes out to catch them.
/// </summary>
public class SearchState : MonoBehaviour, IAIState
{
   private AlienAI ai;
    private AIAnimationController animController;
    private bool isGrabbing = false;
    private bool isBusy = false;
    private float grabTimeout = 10f;
    private float grabStartTime;

    private HashSet<AIBase> ignoredCivs = new HashSet<AIBase>();

    // Constructor that assigns the AlienAI controller reference
    public SearchState(AlienAI ai)
    {
        this.ai = ai;
    }

    // When entering this state — finds the nearest civilian and starts moving towards them
    public void Enter()
    {
        animController = ai.agent.gameObject.GetComponent<AIAnimationController>();
        isGrabbing = false;
        isBusy = false;

        animController.SetAnimation(AIAnimationController.AnimationState.Walk);

        // Find all civilians
        GameObject[] civObjects = GameObject.FindGameObjectsWithTag("Civilian");
        if (civObjects.Length == 0)
        {
            ai.ChangeState(new PatrolState(ai)); // Go back to patrol if no civs are found
            return;
        }

        GameObject closestCiv = null;
        float closestDistance = float.MaxValue;

        // Loop through all civs to find the closest one
        foreach (var civObj in civObjects)
        {
            AIBase civ = civObj.GetComponent<AIBase>();
            if (civ == null || ignoredCivs.Contains(civ)) // Skip ignored or null civs
                continue;

            float distance = Vector3.Distance(ai.transform.position, civ.transform.position);
            if (distance < closestDistance)
            {
                closestCiv = civObj;
                closestDistance = distance;
            }
        }

        // Move towards the selected civilian target
        if (closestCiv != null)
        {
            AIBase civBase = closestCiv.GetComponent<AIBase>();
            ai.currentTargetCiv = civBase;
            ai.MoveTo(closestCiv.transform.position);
        }
    }

    // Continuously called to keep moving towards the target and check for grabbing conditions
    public void Stay()
    {
        if (isGrabbing) return; // Don’t do anything if currently grabbing

        if (ai.currentTargetCiv == null)
        {
            ai.ChangeState(new PatrolState(ai));
            return;
        }

        float distanceToTarget = Vector3.Distance(ai.transform.position, ai.currentTargetCiv.transform.position);
        ai.MoveTo(ai.currentTargetCiv.transform.position);

        // If close enough to grab
        if (distanceToTarget <= ai.tagDistance)
        {
            ai.StartCoroutine(GrabThenReturn());
            if (grabStartTime >= grabTimeout)
            {
                AbandonCurrentTargetAndSearchNew(); // Failsafe in case gets stuck grabbing the same civ
            }
        }
    }

    // Called when exiting this state
    public void Exit()
    {
        isGrabbing = false;
        isBusy = false;
        ai.ResumeMoving();
    }

    // Coroutine that handles grabbing the civilian, playing animation, then changing state
    private IEnumerator GrabThenReturn()
    {
        yield return null;
        isBusy = true;
        isGrabbing = true;
        ai.StopMoving();
        ai.currentTargetCiv.StopMoving();
        animController.SetAnimation(AIAnimationController.AnimationState.Grab);
        //Debug.Log("is grabbing");
        yield return new WaitForSeconds(0.8f); // Simulate grab duration
        ai.currentTargetCiv.ChangeState(new FollowState(ai.currentTargetCiv, ai.transform));
        ai.ChangeState(new ReturnState(ai));
        isBusy = false;
    }

    // Failsafe: abandons the current civilian target and looks for a new one
    private void AbandonCurrentTargetAndSearchNew()
    {
        if (ai.currentTargetCiv != null)
        {
            ignoredCivs.Add(ai.currentTargetCiv); // Mark this civ as ignored
            ai.currentTargetCiv = null;
        }

        GameObject[] civObjects = GameObject.FindGameObjectsWithTag("Civilian");
        GameObject closestCiv = null;
        float closestDistance = float.MaxValue;

        // Same search loop as Enter()
        foreach (var civObj in civObjects)
        {
            AIBase civ = civObj.GetComponent<AIBase>();
            if (civ == null || ignoredCivs.Contains(civ))
                continue;

            float distance = Vector3.Distance(ai.transform.position, civ.transform.position);
            if (distance < closestDistance)
            {
                closestCiv = civObj;
                closestDistance = distance;
            }
        }

        if (closestCiv != null)
        {
            AIBase civBase = closestCiv.GetComponent<AIBase>();
            ai.currentTargetCiv = civBase;
            ai.MoveTo(closestCiv.transform.position);
        }
        else
        {
            ai.ChangeState(new PatrolState(ai));
        }
    }
}


