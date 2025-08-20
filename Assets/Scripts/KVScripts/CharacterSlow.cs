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

    public void ApplySlow(float multiplier, float duration)
    {
        if (!IsServer) return;

       
        if (slowRoutine != null) StopCoroutine(slowRoutine);
        slowRoutine = StartCoroutine(SlowEffect(multiplier, duration));
    }

    private IEnumerator SlowEffect(float multiplier, float duration)
    {
        // Tell all clients to apply slow
        ApplySlowRpc(multiplier);

        yield return new WaitForSeconds(duration);

        // Reset to normal
        ResetSpeedRpc();
        slowRoutine = null;
    }

    [Rpc(SendTo.Everyone)]
    private void ApplySlowRpc(float multiplier)
    {
        if (playerMovement != null)
        {
            
            if (Mathf.Approximately(originalSpeed, 0f))
                originalSpeed = playerMovement.MoveSpeed;

            playerMovement.MoveSpeed *= multiplier;
        }
    }

    [Rpc(SendTo.Everyone)]
    private void ResetSpeedRpc()
    {
        if (playerMovement != null)
        {
            playerMovement.MoveSpeed = originalSpeed;
        }
    }
}