using System.Collections;
using UnityEngine;

public class BoosterStim : MonoBehaviour, IUsable
{
    [SerializeField] private float stimDuration = 10f;
    [SerializeField] private float speedIncrease = 4f;
    [SerializeField] private bool stimUsed = false;

    [SerializeField] private PlayerMovement playerMovement;

    [SerializeField] private GameObject stimBoostVisual;

    public void StopUsing() // IUsable
    {
        Debug.Log("Booster Stim, StopUsing");
    }

    public void Use() // IUsable
    {
        Debug.Log("Booster Stim, Use");
       //playerMovement = GetComponentInParent<PlayerMovement>(); // NOTE - when items get put as a child use this instead?
        playerMovement = FindAnyObjectByType<PlayerMovement>(); // temporary

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

    public void ActivateBoosterStim() // start
    {
        stimUsed = true;
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
    }

    // TO-DO LIST / IDEAS

    // stim affects everyone
    // network it


}
