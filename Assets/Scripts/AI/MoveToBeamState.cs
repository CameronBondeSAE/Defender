using UnityEngine;
using AIAnimation;
public class MoveToBeamState : IAIState
{
    private AIBase ai;
    private Vector3 beamPosition;
    private bool startedSuckUp = false;
    // failsafes.....
    private float stateStartTime;
    private float timeout = 5f;
    private AIAnimationController animController;

    public MoveToBeamState(AIBase ai, Vector3 beamPosition)
    {
        this.ai = ai;
        this.beamPosition = beamPosition;
    }
    public void Enter()
    {
        Debug.Log("MoveToBeamState Enter");
        stateStartTime = Time.time;
        if (ai.agent != null && ai.agent.enabled)
        {
            ai.ResumeMoving();
        }
        else
        {
            Debug.LogWarning("ai is disabled on this civ");
        }
    }
    public void Stay()
    {
        if (startedSuckUp) return;
        float distance = Vector3.Distance(ai.transform.position, beamPosition);
        // added timeout check
        if (Time.time - stateStartTime > timeout)
        {
            Debug.Log("Timed out reaching beam, sucking up anyways");
            startedSuckUp = true;
            ai.StartSuckUp(5f, 1.5f);
            return;
        }
        if (distance < 0.5f)
        {
            Debug.Log("civ is getting sucked up correctly");
            animController = ai.agent.gameObject.GetComponentInChildren<AIAnimationController>();
            animController.SetAnimation(AIAnimationController.AnimationState.GettingSucked);
            startedSuckUp = true;
            ai.StartSuckUp(5f, 1.5f);
        }
        else if (ai.agent != null && ai.agent.enabled)
        {
            ai.MoveTo(beamPosition);
        }
        else
        {
            Debug.LogWarning("ai is disabled on this civ, suck up anyway");
            startedSuckUp = true;
            ai.StartSuckUp(5f, 1.5f);
        }
    }
    public void Exit() { }
}