using UnityEngine;
using AIAnimation;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// AIs who enter this state will flee from repellent source
/// </summary>

public class FleeState : MonoBehaviour, IAIState
{
    private AIBase ai;
    private AIAnimationController animController;
    private Vector3 repellentSource;
    private float repllentRadius;
    private float fleeDuration;
    private float fleeTimer;
    private float lastMoveUpdate = 0f;
    private float moveUpdateInterval = 0.25f;
    private float safeDistanceMultiplier = 1.5f;

    public FleeState(AIBase ai, Vector3 repellentPosition, float radius, float duration)
    {
        this.ai = ai;
        this.repellentSource = repellentPosition;
        this.repllentRadius = radius;
        this.fleeDuration = duration;
    }

    public void Enter()
    {
        animController = ai.GetComponent<AIAnimationController>();

        if (animController != null)
        {
            animController.SetAnimation(AIAnimationController.AnimationState.Walk);
        }
        
       AlienAI alienAI = ai as AlienAI;
       if (alienAI != null)
       {
           alienAI.currentTargetCiv = null;
       }
        
        lastMoveUpdate = Time.time;
        fleeTimer = 0f;
        
        Vector3 fleeDirection = (ai.transform.position - repellentSource).normalized;
        Vector3 targetPosition = ai.transform.position + fleeDirection * (repllentRadius * safeDistanceMultiplier);
        
        ai.MoveTo(targetPosition);
    }

    public void Stay()
    {
        fleeTimer += Time.deltaTime;

        if (fleeTimer >= fleeDuration)
        {
            ai.ChangeState(ai.GetDefaultState());
            return;
        }
        
        float distanceToSource = Vector3.Distance(ai.transform.position, repellentSource);
        if (distanceToSource < repllentRadius * safeDistanceMultiplier)
        {
            ai.ChangeState(ai.GetDefaultState());
            return;
        }

        if (Time.time - lastMoveUpdate >= moveUpdateInterval)
        {
            Vector3 fleeDirection = (ai.transform.position - repellentSource).normalized;
            Vector3 targetPosition = ai.transform.position + fleeDirection * (repllentRadius * safeDistanceMultiplier);
            
            ai.MoveTo(targetPosition);
            lastMoveUpdate = Time.time;
        }
    }

    public void Exit()
    {
        if (animController != null)
        {
            animController.SetAnimation(AIAnimationController.AnimationState.Walk);
        }
    }
    
}
