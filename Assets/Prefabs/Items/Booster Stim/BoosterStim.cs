using UnityEngine;

public class BoosterStim : MonoBehaviour, IUsable
{
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

    
}
