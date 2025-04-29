using UnityEngine;
using AIAnimation;
using System.Collections;

public class SearchState : MonoBehaviour, IAIState
{
    private AlienAI ai;
    private AIAnimationController animController;
    private bool isGrabbing = false;
    private bool isBusy = false;
    private float grabTimeout = 5f; // Time before abandoning the civ if stuck
    private float grabStartTime;

    public SearchState(AlienAI ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        animController = ai.agent.gameObject.GetComponent<AIAnimationController>();
    }

    public void Stay()
    {
        if (isGrabbing) return;
        animController.SetAnimation(AIAnimationController.AnimationState.Walk);
        GameObject[] civObjects = GameObject.FindGameObjectsWithTag("Civilian");
        if (civObjects.Length == 0)
        {
            ai.ChangeState(new PatrolState(ai));
            return;
        }
        
        GameObject closestCiv = null;
        float closestDistance = float.MaxValue;
        
        // Search for the closest civ
        foreach (var civObj in civObjects)
        {
            AIBase civ = civObj.GetComponent<AIBase>();
            if (civ == null || civ == ai.currentTargetCiv)
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
            // Check for grab
            if (closestDistance < ai.tagDistance)
            {
                isGrabbing = true;
                ai.StopMoving();
                civBase.ChangeState(new FollowState(civBase, ai.transform));
                grabStartTime = Time.time; // Start tracking time
                ai.StartCoroutine(GrabThenReturn());
            }
        }
        
        if (ai.currentTargetCiv != null) // ignore other civs if already got a civ
        {
            Debug.Log(ai.currentTargetCiv);
            return;
        }

    }
    
    public void Exit()
    {
        isGrabbing = false;
        ai.ResumeMoving();
    }

    private IEnumerator GrabThenReturn()
    {
        yield return null;
        animController.SetAnimation(AIAnimationController.AnimationState.Grab);
        Debug.Log("is grabbing");
        yield return new WaitForSeconds(.8f); 
        ai.ChangeState(new ReturnState(ai));
    }
}
