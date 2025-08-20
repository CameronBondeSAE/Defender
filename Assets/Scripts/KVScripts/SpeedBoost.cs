using UnityEngine;
using Unity.Netcode;

public class SpeedBoost : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return; // Server is authoritative for physics and triggering
        var boostable = other.GetComponentInParent<CharacterBoost>();
        
        if (boostable != null)
        {
            boostable.RequestBoost(); // Direct call on server-side instance
        }
    }
}