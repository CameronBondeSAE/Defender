using UnityEngine;
using Unity.Netcode;

public class SpeedBoost : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var boostable = other.GetComponentInParent<CharacterBoost>();
        if (boostable == null) return;

        if (IsServer)
        {
            // Host/server directly applies boost
            boostable.RequestBoost();
        }
        else if (IsClient && boostable.IsOwner)
        {
            // Client tells server to apply boost for them
            boostable.RequestBoostServerRpc();
        }
    }
}