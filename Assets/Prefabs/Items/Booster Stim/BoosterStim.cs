using System.Collections;
using UnityEngine;

public class BoosterStim : MonoBehaviour, IUsable
{
    [SerializeField] private float stimDuration = 10f;
    [SerializeField] private float speedIncrease = 4f;

    [SerializeField] private PlayerMovement playerMovement;

    public void StopUsing()
    {
        Debug.Log("Booster Stim, StopUsing");
    }

    public void Use()
    {
        Debug.Log("Booster Stim, Use");
       //playerMovement = GetComponentInParent<PlayerMovement>(); // NOTE - when items get put as a child use this instead
        playerMovement = FindAnyObjectByType<PlayerMovement>(); // temporary
        ActivateBoosterStim();

    }

    public void ActivateBoosterStim()
    {
        playerMovement.MoveSpeed += speedIncrease; 
        StartCoroutine(BoosterStimDuration());

    }

    IEnumerator BoosterStimDuration()
    {
        Debug.Log("stim timer active");
        yield return new WaitForSeconds(stimDuration);

        DeactivateBoosterStim();
    }

    public void DeactivateBoosterStim()
    {
        playerMovement.MoveSpeed -= speedIncrease;
    }

    // TO-DO LIST / IDEAS

    // Rechargeable stim??
    // stim affects everyone
    // network it


}
