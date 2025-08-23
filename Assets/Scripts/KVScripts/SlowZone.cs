using UnityEngine;
using Unity.Netcode;

public class SlowZone : NetworkBehaviour
{
    [Header("Slow Zone Settings")]
    [Range(0.1f, 1f)]
    public float slowMultiplier = 0.5f;
    public float slowDuration = 3f;

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer)
        {
            TryApplySlow(other);
        }
        else if (IsClient) // client relay in case server missed it
        {
            var netObj = other.GetComponentInParent<NetworkObject>();
            if (netObj != null && netObj.IsOwner) // only tell server about your own player
            {
                NotifyServerOfSlowServerRpc(netObj);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyServerOfSlowServerRpc(NetworkObjectReference playerRef)
    {
        if (playerRef.TryGet(out NetworkObject netObj))
        {
            TryApplySlow(netObj.GetComponentInParent<Collider>());
        }
    }

    private void TryApplySlow(Collider other)
    {
        var slowable = other.GetComponentInParent<CharacterSlow>();
        if (slowable != null)
        {
            slowable.ApplySlow(slowMultiplier, slowDuration);
        }
    }
}