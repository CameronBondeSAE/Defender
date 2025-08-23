using UnityEngine;
using Unity.Netcode;

public class CharacterBoost : NetworkBehaviour
{
    [Header("Character Boost Values")]
    public float boostForce = 10f;

    [Header("Boost Audio")]
    public float audioHearingRadius = 15f;
    public AudioSource audioSource;
    public AudioClip boostClip;

    [Header("PlayerMovement & Rigidbody")]
    private PlayerMovement playerMovement;
    private Rigidbody rb;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }


    public void RequestBoost()
    {
        if (!IsServer) return; // server only
        ApplyBoost();
    }
    
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void RequestBoostServerRpc()
    {
        ApplyBoost();
    }

    private void ApplyBoost()
    {
        Vector3 boostDirection = playerMovement.moveDirection.normalized;
        if (boostDirection == Vector3.zero)
            boostDirection = transform.forward;

        rb.AddForce(boostDirection * boostForce, ForceMode.VelocityChange);

        // Play audio for owner + nearby players
        PlayBoostRpc(transform.position);
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void PlayBoostRpc(Vector3 boostPosition)
    {
        if (audioSource == null || boostClip == null) return;


        if (NetworkManager.Singleton.LocalClientId == OwnerClientId)
        {
            audioSource.PlayOneShot(boostClip);
        }

        else if (Vector3.Distance(transform.position, boostPosition) <= audioHearingRadius)
        {
            audioSource.PlayOneShot(boostClip);
        }
    }
}