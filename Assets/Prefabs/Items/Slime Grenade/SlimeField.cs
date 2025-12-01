using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Slows down AI characters when they enter this trigger zone
/// </summary>
public class SlimeField : MonoBehaviour
{
    [Header("Slow Down Settings")]
    [Tooltip("Speed multiplier applied to AI in this zone (0.5 = 50% speed)")]
    [Range(0.1f, 1f)]
    public float speedMultiplier = 0.5f;
    
    [Tooltip("Force multiplier for rigidbody-based AI (0.5 = 50% force)")]
    [Range(0.1f, 1f)]
    public float forceMultiplier = 0.5f;
    
    // Store original values to restore later
    private Dictionary<AIBase, float> originalSpeeds = new Dictionary<AIBase, float>();
    private Dictionary<AIBase, float> originalForces = new Dictionary<AIBase, float>();

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object has an AIBase component
        AIBase ai = other.GetComponent<AIBase>();
        
        if (ai != null)
        {
            // Handle rigidbody-based AI
            if (ai.UseRigidbody && ai.rb != null)
            {
                // Store original forward force
                if (!originalForces.ContainsKey(ai))
                {
                    originalForces[ai] = ai.forwardForce;
                }
                
                // Apply slow down
                ai.forwardForce *= forceMultiplier;
                Debug.Log($"{ai.name} entered slow zone. Force reduced to {ai.forwardForce}");
            }
            // Handle NavMeshAgent-based AI
            else if (ai.Agent != null && ai.Agent.enabled)
            {
                // Store original speed
                if (!originalSpeeds.ContainsKey(ai))
                {
                    originalSpeeds[ai] = ai.Agent.speed;
                }
                
                // Apply slow down
                ai.Agent.speed *= speedMultiplier;
                Debug.Log($"{ai.name} entered slow zone. Speed reduced to {ai.Agent.speed}");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the object has an AIBase component
        AIBase ai = other.GetComponent<AIBase>();
        
        if (ai != null)
        {
            // Restore rigidbody-based AI
            if (ai.UseRigidbody && ai.rb != null)
            {
                if (originalForces.ContainsKey(ai))
                {
                    ai.forwardForce = originalForces[ai];
                    originalForces.Remove(ai);
                    Debug.Log($"{ai.name} exited slow zone. Force restored to {ai.forwardForce}");
                }
            }
            // Restore NavMeshAgent-based AI
            else if (ai.Agent != null && ai.Agent.enabled)
            {
                if (originalSpeeds.ContainsKey(ai))
                {
                    ai.Agent.speed = originalSpeeds[ai];
                    originalSpeeds.Remove(ai);
                    Debug.Log($"{ai.name} exited slow zone. Speed restored to {ai.Agent.speed}");
                }
            }
        }
    }
    
    // Clean up if AI is destroyed while in zone
    private void OnDestroy()
    {
        originalSpeeds.Clear();
        originalForces.Clear();
    }
}
