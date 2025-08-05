using System.Collections;
using UnityEngine;

public class BoosterStim : MonoBehaviour, IUsable
{
    private float stimDuration = 10f;

    public void StopUsing()
    {
        Debug.Log("Booster Stim, StopUsing");
    }

    public void Use()
    {
        Debug.Log("Booster Stim, Use");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ActivateBoosterStim()
    {
        StartCoroutine(speedBoostTimer());

    }

    IEnumerator speedBoostTimer()
    {
        // speed player
        yield return new WaitForSeconds(stimDuration);

    }
}
