using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class CharacterSlow : NetworkBehaviour
{
    private PlayerMovement playerMovement;
    private Coroutine slowRoutine;
    private float originalSpeed;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
            originalSpeed = playerMovement.MoveSpeed;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (playerMovement != null && Mathf.Approximately(originalSpeed, 0f))
            originalSpeed = playerMovement.MoveSpeed;
    }

    public void ApplySlow(float multiplier, float duration)
    {
        if (!IsServer) return; 

        if (slowRoutine != null) 
            StopCoroutine(slowRoutine);

        slowRoutine = StartCoroutine(SlowEffect(multiplier, duration));
    }

    private IEnumerator SlowEffect(float multiplier, float duration)
    {
        ApplySlowRpc(multiplier);

        yield return new WaitForSeconds(duration);

        ResetSpeedRpc();
        slowRoutine = null;
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void ApplySlowRpc(float multiplier)
    {
        if (playerMovement == null) return;

        if (Mathf.Approximately(originalSpeed, 0f))
            originalSpeed = playerMovement.MoveSpeed;

        playerMovement.MoveSpeed *= multiplier;
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void ResetSpeedRpc()
    {
        if (playerMovement != null)
            playerMovement.MoveSpeed = originalSpeed;
    }
}