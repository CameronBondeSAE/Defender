using System.Collections;
using Defender;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// A booster stim that boosts player speed for a set duration of time visually being empty, once the duration is used up it will despawn
/// </summary>


public class BoosterStim : UsableItem_Base
{
    [Header("Booster Stim Stats")]
    [SerializeField] private float stimDuration = 10f;
    [SerializeField] private float speedIncrease = 4f;
    [SerializeField] private bool stimUsed = false;

    [SerializeField] private PlayerMovement playerMovement;

    [SerializeField] private GameObject stimBoostVisual;


    #region IUsable

    public override void StopUsing()
    {
        base.StopUsing();
        //Debug.Log("Booster Stim, StopUsing");
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
    private void StopUsing_Rpc()
    {

    }


    public override void Use(CharacterBase characterTryingToUse)
    {
        base.Use(characterTryingToUse);

        Debug.Log("Booster Stim, Use");
       //playerMovement = GetComponentInParent<PlayerMovement>(); // NOTE - when items get put as a child use this instead?
        // playerMovement = FindAnyObjectByType<PlayerMovement>(); // TODO temporary
		playerMovement = characterTryingToUse.GetComponent<PlayerMovement>(); // CAM NOTE: We added this to the IUseable JUST FOR YOU
		
        if (stimUsed == false)
        {
            ActivateBoosterStim();
        }


        if(stimBoostVisual != null)
        {
            Renderer render = stimBoostVisual.GetComponent<Renderer>();
            render.material.color = Color.black;
        }

    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
    private void Use_Rpc()
    {

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

    }


    public override void Drop()
    {
        //Debug.Log("Stim, Dropped");
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
    private void Drop_Rpc()
    {

    }

    #endregion


    public void ActivateBoosterStim() // start
    {
        stimUsed = true;
        //Debug.Log("Stim Boost Active");

        playerMovement.MoveSpeed += speedIncrease; 
        StartCoroutine(BoosterStimDuration());
    }

    IEnumerator BoosterStimDuration() // while
    {
        yield return new WaitForSeconds(stimDuration);

        DeactivateBoosterStim();
    }

    public void DeactivateBoosterStim() // end
    {
        playerMovement.MoveSpeed -= speedIncrease;
        //Debug.Log("Stim Boost Deactive");

        GetComponent<NetworkObject>().Despawn();
    }


    // TO-DO LIST / IDEAS (nickA)

    // network it (sync for everyone + position and use)


}
