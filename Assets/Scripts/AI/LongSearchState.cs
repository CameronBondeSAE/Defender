using UnityEngine;
using UnityEngine;
using AIAnimation;
using System.Collections;
using System.Collections.Generic;


public class LongSearchState : MonoBehaviour, IAIState
{
    private AlienAI ai;
    private AIAnimationController animController;
    private bool isGrabbing = false;
    private bool isBusy = false;
    private float grabTimeout = 5f;
    private float grabStartTime;
    private List<AIBase> collectedCivilians = new List<AIBase>();
    private const int maxCivilians = 3;

    public LongSearchState(AlienAI ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        animController = ai.agent.gameObject.GetComponent<AIAnimationController>();
        isGrabbing = false;
        isBusy = false;
        collectedCivilians.Clear();
    }

    public void Stay()
    {
        if (isBusy || collectedCivilians.Count >= maxCivilians)
            return;

        animController.SetAnimation(AIAnimationController.AnimationState.Walk);

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
            if (civ == null || collectedCivilians.Contains(civ))
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

            if (closestDistance < ai.tagDistance)
            {
                isBusy = true;
                ai.StopMoving();
                civBase.ChangeState(new FollowState(civBase, ai.transform));
                grabStartTime = Time.time;
                ai.StartCoroutine(GrabThenContinue(civBase));
            }
        }
    }

    public void Exit()
    {
        isGrabbing = false;
        isBusy = false;
        ai.ResumeMoving();
        ai.currentTargetCiv = null;
    }

    private IEnumerator GrabThenContinue(AIBase civ) // rather than returning after capturing one civ, this alien will keep going until he captures his designated amount
    {
        animController.SetAnimation(AIAnimationController.AnimationState.Grab);
        Debug.Log("Grabbing civilian...");
        yield return new WaitForSeconds(0.8f);

        if (Time.time - grabStartTime >= grabTimeout)
        {
            Debug.Log("Abandoned civilian due to timeout");
            ai.currentTargetCiv = null;
        }
        else
        {
            collectedCivilians.Add(civ);
            Debug.Log($"Collected {collectedCivilians.Count} civilians");
        }

        isBusy = false;

        if (collectedCivilians.Count >= maxCivilians)
        {
            ai.ChangeState(new ReturnState(ai));
        }
    }
}
