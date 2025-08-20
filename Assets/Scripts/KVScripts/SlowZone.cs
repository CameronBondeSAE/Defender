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
        if (!IsServer) return; 

        var slowable = other.GetComponentInParent<CharacterSlow>();
        if (slowable != null)
        {
            slowable.ApplySlow(slowMultiplier, slowDuration);
        }
    }
}