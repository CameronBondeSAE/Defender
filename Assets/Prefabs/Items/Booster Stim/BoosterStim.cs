using System.Collections;
using Defender;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// A booster stim that boosts player speed for a set duration of time, once the duration is used up it will despawn
/// </summary>


public class BoosterStim : UsableItem_Base
{
    [Header("Booster Stim Stats")]
    [SerializeField] private float stimDuration = 10f;
    [SerializeField] private float speedIncrease = 4f;
    [SerializeField] private bool stimUsed = false;

    [SerializeField] private PlayerMovement playerMovement;


    #region IUsable

    public override void StopUsing()
    {
        base.StopUsing();
        //Debug.Log("Booster Stim, StopUsing");
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
    private void StopUsing_Rpc()
    {
        //unused
    }


    public override void Use(CharacterBase characterTryingToUse)
    {
        base.Use(characterTryingToUse);

        playerMovement = characterTryingToUse.GetComponent<PlayerMovement>(); // targets only the person that actually uses it

        Use_Rpc();

    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
    private void Use_Rpc()
    {
        Debug.Log("Booster Stim, Use");

        if (stimUsed == false)
        {
            ActivateBoosterStim_Rpc();
        }

    }

    #endregion

    #region IPickup

    public override void Pickup(CharacterBase characterTryingPickup)
    {
        //Debug.Log("Stim, PickedUp");
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
    private void Pickup_Rpc()
    {
        // unused
    }


    public override void Drop()
    {
        //Debug.Log("Stim, Dropped");
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
    private void Drop_Rpc()
    {
        // unused
    }

    #endregion

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
    public void ActivateBoosterStim_Rpc() // start  
    {
        stimUsed = true;
        //Debug.Log("Stim Boost Active");

        playerMovement.MoveSpeed += speedIncrease;  // gives speed to player
        StartCoroutine(BoosterStimDuration());
    }

    IEnumerator BoosterStimDuration() // while
    {
        yield return new WaitForSeconds(stimDuration);

        DeactivateBoosterStim_Rpc();
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
    public void DeactivateBoosterStim_Rpc() // end
    {
        playerMovement.MoveSpeed -= speedIncrease; // removes speed from player
        //Debug.Log("Stim Boost Deactive");

        GetComponent<NetworkObject>().Despawn();
    }

}
