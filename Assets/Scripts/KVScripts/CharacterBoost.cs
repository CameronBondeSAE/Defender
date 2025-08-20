using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Netcode;

public class CharacterBoost : NetworkBehaviour
{
    [Header("Character Boost Values")]
    public float boostForce = 10f;
    public float boostCoolDown = 2f;
    
    [Header("Boost Values Values")]
    public float audioHearingRadius = 15f;
    public AudioSource audioSource;
    public AudioClip boostClip;
    
    [Header("PlayerMovement Values")]
    private PlayerMovement playerMovement;
    private Rigidbody rb;
    private float lastBoostTime; 
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
        
        if(audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void RequestBoost()
    {
        if (!IsServer) return;
        Vector3 boostDirection = playerMovement.moveDirection.normalized;

        if (boostDirection == Vector3.zero)
            boostDirection = transform.forward;
        rb.AddForce(boostDirection * boostForce, ForceMode.VelocityChange);

        PlayerBoostSFXRpc(transform.position);

    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void PlayerBoostSFXRpc(Vector3 boostPosition)
    {
        if (IsOwner)
        {
            audioSource.PlayOneShot(boostClip);
            Debug.LogError("CLIP PLAYED");
        }
        else
        {
            
            if (Vector3.Distance(boostPosition, transform.position) <= audioHearingRadius) 
                audioSource.PlayOneShot(boostClip);
            Debug.LogError("PLAYERS CLIP PLAYED");
        }
    }
}
