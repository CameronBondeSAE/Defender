using Defender;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class HealingStim : UsableItem_Base
{

    [Header("Healing Stim")]
    [SerializeField] private float stimHealingAmount = 25f;

    [SerializeField] private Health health;

    public override void Use(CharacterBase characterTryingToUse)
    {
        base.Use(characterTryingToUse);

        health = characterTryingToUse.GetComponent<Health>(); // targets only the person that actually uses it
        //Debug.Log("health stim test: " + characterTryingToUse.gameObject.name);
        //Debug.Log("health stim test: " + health);

        Use_Rpc();

    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
    public void Use_Rpc()
    {
        Debug.Log("Healing Stim, Use");

        if (health != null)
        {
            health.Heal(stimHealingAmount);
            //Debug.Log("Heal Stim: " + stimHealingAmount);
        }

        StartCoroutine(destroyDelay());
    }

    IEnumerator destroyDelay()
    {
        yield return new WaitForSeconds(1f); // allows sound to play

        GetComponent<NetworkObject>().Despawn();
    }

}
