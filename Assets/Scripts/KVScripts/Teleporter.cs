using System;
using UnityEngine;
using Unity.Netcode;

public class Teleporter : NetworkBehaviour
{
    [Header("Teleporter Settings")]
    public Transform destination; // The exit teleporter
    public AudioClip teleportClip;
    public float audioHearingRadius = 15f;
    public AudioSource audioSource;
    
    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!IsServer) return;

        var playerObject = other.GetComponentInParent<NetworkObject>();
        if (playerObject != null)
        {
            playerObject.transform.position = destination.position;
            PlayTeleportSFXRpc(playerObject.OwnerClientId, destination.position);
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    private void PlayTeleportSFXRpc(ulong teleportingPlayerId, Vector3 teleportPosition)
    {
        
    }
}