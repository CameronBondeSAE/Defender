using UnityEngine;
using AIAnimation;

/// <summary>
/// AIs who transition into this state (in this case - specifically the Aliens) will head towards the mothership
/// </summary>
public class ReturnState : IAIState
{
    private AlienAI ai; // Only aliens use this state for now
    public ReturnState(AlienAI ai) => this.ai = ai;
    private AIAnimationController animController;
    
    private bool hasReachedDropZone = false;
    private float lastMoveUpdate = 0f;
    private float moveUpdateInterval = 0.2f;

    public void Enter()
    {
        animController = ai.agent.gameObject.GetComponentInChildren<AIAnimationController>();
        hasReachedDropZone = false;
        if (ai.mothership != null)
        {
            var dropZoneScript = ai.mothership.GetComponentInChildren<MothershipDropZone>();
            Vector3 dropOffPos = dropZoneScript?.DropOffZone?.position ?? ai.mothership.position;
            ai.MoveTo(dropOffPos);
        }
    }

    public void Stay()
    {
        animController.SetAnimation(AIAnimationController.AnimationState.Walk);
        if (ai.mothership == null)
        {
            Debug.Log("missing mothership reference");
            ai.ChangeState(new SearchState(ai));
            return;
        }
        if (ai.currentTargetCiv == null || ai.currentTargetCiv.escortingAlien != ai)
        {
            ai.currentTargetCiv = null;
            ai.ChangeState(new SearchState(ai));
            return;
        }
        var dropZoneScript = ai.mothership.GetComponentInChildren<MothershipDropZone>();
        if (dropZoneScript == null) return;
        Vector3 dropOffPos = dropZoneScript.DropOffZone != null
            ? dropZoneScript.DropOffZone.position:
            dropZoneScript.transform.position;
        
        float distance = Vector3.Distance(ai.transform.position, dropOffPos);
        if (!hasReachedDropZone && distance < 4f)
        {
            hasReachedDropZone = true;
            Debug.Log(ai.name + " has reached the drop zone");
            //ai.StopMoving();
        }
        else if (!hasReachedDropZone)
        {
            if (Time.time - lastMoveUpdate > moveUpdateInterval)
            {
                ai.MoveTo(dropOffPos);
                lastMoveUpdate = Time.time;
            }
        }

        if (hasReachedDropZone && ai.currentTargetCiv != null)
        {
            float civDistanceToBeam = Vector3.Distance(ai.currentTargetCiv.transform.position, dropOffPos);
            if (civDistanceToBeam > 10f)
            {
                Debug.Log("reached the drop zone, civ seems stuck, abandoning");
                ai.currentTargetCiv.SetAbducted(false);
                ai.currentTargetCiv.escortingAlien = null;
                ai.currentTargetCiv = null;
                ai.ChangeState(new SearchState(ai));
            }
        }
    }

    public void Exit()
    {
        ai.ResumeMoving();
    }
}