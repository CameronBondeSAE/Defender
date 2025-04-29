using UnityEngine;
using AIAnimation;
using System.Collections;

public class SearchState : MonoBehaviour, IAIState
{
    private AlienAI ai;
    private AIAnimationController animController;
    private bool isGrabbing = false;

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

        for (int i = 0; i < civObjects.Length; i++)
        {
            WalkingCivilianAI civ = civObjects[i].GetComponent<WalkingCivilianAI>();
            if (civ == null || civ == ai.currentTargetCiv)
                continue;
                float distance = Vector3.Distance(ai.transform.position, civ.transform.position);
                //Debug.Log(distance);
                if (distance < ai.tagDistance)
                {
                    ai.currentTargetCiv = civ;
                    ai.isReached = false;
                    isGrabbing = true;
                    civ.ChangeState(new FollowState(civ, ai.transform));
                    ai.StopMoving(); 
                    ai.StartCoroutine(GrabThenReturn());
                }
                else
                {
                    ai.MoveTo(civ.transform.position);
                }
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
