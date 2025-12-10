using UnityEngine;
using Unity.Netcode;

public class Teleporter : NetworkBehaviour
{
    [Header("Teleporter Settings")]
    public Transform destination; // The exit teleporter
    public AudioClip teleportClip;
    public float audioHearingRadius = 15f;
    public AudioSource audioSource;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        var playerObject = other.GetComponentInParent<NetworkObject>();
        if (playerObject != null)
        {
            playerObject.transform.position = destination.position;
            
            PlayTeleportSFXRpc(playerObject.OwnerClientId, destination.position);
        }
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void PlayTeleportSFXRpc(ulong teleportingPlayerId, Vector3 teleportPosition)
    {
        if (audioSource == null || teleportClip == null) return;

        // Teleporting player always hears it
        if (NetworkManager.Singleton.LocalClientId == teleportingPlayerId)
        {
            audioSource.PlayOneShot(teleportClip);
            return;
        }

        // Other clients hear it only if within audioHearingRadius
        if (Vector3.Distance(transform.position, teleportPosition) <= audioHearingRadius)
        {
            audioSource.PlayOneShot(teleportClip);
        }
    }
}