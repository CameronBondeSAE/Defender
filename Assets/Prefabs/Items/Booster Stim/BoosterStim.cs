using System.Collections;
using Defender;
using UnityEngine;

public class BoosterStim : MonoBehaviour, IUsable, IPickup
{
    [SerializeField] private float stimDuration = 10f;
    [SerializeField] private float speedIncrease = 4f;
    [SerializeField] private bool stimUsed = false;

    [SerializeField] private PlayerMovement playerMovement;

    [SerializeField] private GameObject stimBoostVisual;


    #region IUsable

    public void StopUsing()
    {
        //Debug.Log("Booster Stim, StopUsing");
    }

    public void Use(CharacterBase characterTryingToUse)
    {
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

    public void Pickup()
    {
        //Debug.Log("Stim PickedUp");
    }

    public void Drop()
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
    }


    // TO-DO LIST / IDEAS (nickA)

    // network it (sync for everyone + position and use), sync stimUsed variable for late join?


}
