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

    public void Enter()
    {
        animController = ai.agent.gameObject.GetComponentInChildren<AIAnimationController>();
        if (ai.mothership != null)
            ai.MoveTo(ai.mothership.position);
    }

    public void Stay()
    {
        animController.SetAnimation(AIAnimationController.AnimationState.Walk);
        // Do nothing if mothership reference is missing
        if (ai.mothership == null) return;

        // Check distance to mothership
        float dist = Vector3.Distance(ai.transform.position, ai.mothership.position);
        if (dist < 1f)
        {
            // If carrying a civ, make the civ idle again when arrived
            if (ai.currentTargetCiv != null)
                ai.currentTargetCiv.ChangeState(new IdleState(ai.currentTargetCiv));

            // Clear target civ reference
            ai.currentTargetCiv = null;

            // Switch back to searching state
            ai.ChangeState(new SearchState(ai));
        }
    }

    public void Exit() { }
}