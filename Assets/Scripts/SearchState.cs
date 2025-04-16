using UnityEngine;
using AIAnimation;

public class SearchState : IAIState
{
    private AlienAI ai;
    private AIAnimationController animController;

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
        animController.SetAnimation(AIAnimationController.AnimationState.Walk);
        GameObject[] civObjects = GameObject.FindGameObjectsWithTag("Civilian");

        for (int i = 0; i < civObjects.Length; i++)
        {
            CivilianAI civ = civObjects[i].GetComponent<CivilianAI>();
            if (civ == null || civ == ai.currentTargetCiv)
                continue;
                float distance = Vector3.Distance(ai.transform.position, civ.transform.position);
                Debug.Log(distance);
                if (distance < ai.tagDistance)
                {
                    ai.currentTargetCiv = civ;
                    ai.isReached = false;
                    ai.ChangeState(new ReturnState(ai));
                    civ.ChangeState(new FollowState(civ, ai.transform));
                    return;
                }
                else
                {
                    ai.MoveTo(civ.transform.position);
                }
        }
    }

    public void Exit()
    {

    }
}
