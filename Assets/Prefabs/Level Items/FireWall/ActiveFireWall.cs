using System;
using System.Collections;
using Defender;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ActiveFireWall : NetworkBehaviour, IUsable
{
    [Header("Fire Wall Settings")]
    [SerializeField] public ParticleSystem fireParticle;
    [SerializeField] public GameObject fireFlame;
    
    [Header("Timing Settings")]
    [SerializeField] private float activeDuration = 5f;
    [SerializeField] private float cooldownDuration = 60f;
    
    private bool isOnCooldown = false;
    private bool isActive = false;
    
    void Start()
    {
        fireParticle.gameObject.SetActive(false);
        if (fireFlame != null)
        {
            fireFlame.SetActive(false);
        }   
    }
    
    public void Use(CharacterBase characterTryingToUse)
    {
        if (isOnCooldown)
        {
            Debug.Log("FireWall is on cooldown!");
            return;
        }
    
        if (isActive)
        {
            Debug.Log("FireWall is already active!");
            return;
        }
    
        StartCoroutine(ActivateFireWall(characterTryingToUse));
    }

    private IEnumerator ActivateFireWall(CharacterBase character)
    {
        isActive = true;
        
        // Activate particle system and flame
        fireParticle.gameObject.SetActive(true);
        fireParticle.Play();
        
        if (fireFlame != null)
        {
            fireFlame.SetActive(true);
        }
        
        Debug.Log("FireWall activated by " + character.name);
        
        // Wait for active duration (5 seconds)
        yield return new WaitForSeconds(activeDuration);
        
        // Deactivate
        fireParticle.Stop();
        fireParticle.gameObject.SetActive(false);
        
        if (fireFlame != null)
        {
            fireFlame.SetActive(false);
        }
        
        isActive = false;
        Debug.Log("FireWall deactivated");
        
        // Start cooldown
        isOnCooldown = true;
        Debug.Log("FireWall cooldown started (60 seconds)");
        
        // Wait for cooldown duration (60 seconds)
        yield return new WaitForSeconds(cooldownDuration);
        
        isOnCooldown = false;
        Debug.Log("FireWall ready to use again!");
    }

    public void StopUsing()
    {
    }
}