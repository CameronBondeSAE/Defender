using System.Collections;
using Defender;
using Unity.Netcode;
using UnityEngine;

public class BoosterStim : UsableItem_Base, IUsable, IPickup
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

    #endregion

    #region IPickup

    public override void Pickup()
    {
        //Debug.Log("Stim PickedUp");
    }

    public override void Drop()
    {
        //Debug.Log("Stim Dropped");
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
