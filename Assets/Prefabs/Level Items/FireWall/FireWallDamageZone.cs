using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireWallDamageZone : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damagePerSecond = 10f;
    [SerializeField] private float damageTickRate = 0.5f; // How often to apply damage (in seconds)
    
    // Track entities currently in the damage zone
    private Dictionary<Health, Coroutine> damageCoroutines = new Dictionary<Health, Coroutine>();
    
    private void OnTriggerEnter(Collider other)
    {
        // Try to get Health component from the object that entered
        Health health = other.GetComponent<Health>();
        
        if (health != null && !health.isDead)
        {
            // Start damaging this entity
            if (!damageCoroutines.ContainsKey(health))
            {
                Coroutine damageCoroutine = StartCoroutine(ApplyDamageOverTime(health));
                damageCoroutines.Add(health, damageCoroutine);
                Debug.Log(other.gameObject.name + " entered fire zone and is taking damage!");
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // Try to get Health component from the object that exited
        Health health = other.GetComponent<Health>();
        
        if (health != null)
        {
            // Stop damaging this entity
            if (damageCoroutines.ContainsKey(health))
            {
                StopCoroutine(damageCoroutines[health]);
                damageCoroutines.Remove(health);
                Debug.Log(other.gameObject.name + " exited fire zone and stopped taking damage!");
            }
        }
    }
    
    private IEnumerator ApplyDamageOverTime(Health health)
    {
        while (health != null && !health.isDead)
        {
            // Calculate damage for this tick
            float damageAmount = damagePerSecond * damageTickRate;
            
            // Apply damage
            health.TakeDamage(damageAmount);
            
            // Wait before next damage tick
            yield return new WaitForSeconds(damageTickRate);
        }
        
        // Clean up if entity died
        if (damageCoroutines.ContainsKey(health))
        {
            damageCoroutines.Remove(health);
        }
    }
    
    // Clean up all coroutines when this object is disabled/destroyed
    private void OnDisable()
    {
        // Stop all damage coroutines
        foreach (var coroutine in damageCoroutines.Values)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        damageCoroutines.Clear();
    }
}