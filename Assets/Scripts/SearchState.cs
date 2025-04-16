using UnityEngine;

public class SearchState : IAIState
{
    private AlienAI ai;

    public SearchState(AlienAI ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {

    }

    public void Stay()
    {
        GameObject[] civObjects = GameObject.FindGameObjectsWithTag("Civilian");

        for (int i = 0; i < civObjects.Length; i++)
        {
            CivilianAI civ = civObjects[i].GetComponent<CivilianAI>();

            if (civ != null && civ != ai.currentTargetCiv)
            {
                float distance = Vector3.Distance(ai.transform.position, civ.transform.position);

                if (distance < ai.searchRange)
                {
                    ai.currentTargetCiv = civ;
                    ai.ChangeState(new ReturnState(ai));
                    civ.ChangeState(new FollowState(civ, ai.transform));
                    return;
                }
            }
        }

        ai.FaceDirection();
    }

    public void Exit()
    {

    }
}
