using UnityEngine;
using AIAnimation;
using System.Collections;
using System.Collections.Generic;

public class SearchState : MonoBehaviour, IAIState
{
   private AlienAI ai;
    private AIAnimationController animController;
    private bool isGrabbing = false;
    private bool isBusy = false;
    private float grabTimeout = 10f;
    private float grabStartTime;
    
    private HashSet<AIBase> ignoredCivs = new HashSet<AIBase>();

    public SearchState(AlienAI ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        animController = ai.agent.gameObject.GetComponentInChildren<AIAnimationController>();
        isGrabbing = false;
        isBusy = false;

        animController.SetAnimation(AIAnimationController.AnimationState.Walk);

        // Find closest civ only once
        GameObject[] civObjects = GameObject.FindGameObjectsWithTag("Civilian");
        if (civObjects.Length == 0)
        {
            ai.ChangeState(new PatrolState(ai));
            return;
        }

        GameObject closestCiv = null;
        float closestDistance = float.MaxValue;

        foreach (var civObj in civObjects)
        {
            AIBase civ = civObj.GetComponent<AIBase>();
            if (civ == null || ignoredCivs.Contains(civ)) // skipping ignored civs
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
            //ai.ChangeState(new PatrolState(ai));
        }
    }

    public void Stay()
    {
        if (isGrabbing) return;

        if (ai.currentTargetCiv == null)
        {
            ai.ChangeState(new PatrolState(ai));
            return;
        }

        float distanceToTarget = Vector3.Distance(ai.transform.position, ai.currentTargetCiv.transform.position);
        ai.MoveTo(ai.currentTargetCiv.transform.position);

        if (distanceToTarget <= ai.tagDistance)
        {
            ai.StartCoroutine(GrabThenReturn());
            if (grabStartTime >= grabTimeout)
            {
                AbandonCurrentTargetAndSearchNew(); // failsafe
                //ai.currentTargetCiv.gameObject.SetActive(false);// failsafe
            }
        }
    }

    public void Exit()
    {
        isGrabbing = false;
        isBusy = false;
        ai.ResumeMoving();
    }

    private IEnumerator GrabThenReturn()
    {
        yield return null;
        isBusy = true;
        isGrabbing = true;
        ai.StopMoving();
        ai.currentTargetCiv.StopMoving();
        animController.SetAnimation(AIAnimationController.AnimationState.Grab);
        Debug.Log("is grabbing");
        yield return new WaitForSeconds(0.8f);
        ai.currentTargetCiv.ChangeState(new FollowState(ai.currentTargetCiv, ai.transform));
        ai.ChangeState(new ReturnState(ai));
        isBusy = false;
    }
    
    // if stuck on a civ, mark him and do not target him again
    private void AbandonCurrentTargetAndSearchNew()
    {
        if (ai.currentTargetCiv != null)
        {
            ignoredCivs.Add(ai.currentTargetCiv);
            ai.currentTargetCiv = null;
        }

        GameObject[] civObjects = GameObject.FindGameObjectsWithTag("Civilian");
        GameObject closestCiv = null;
        float closestDistance = float.MaxValue;
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


