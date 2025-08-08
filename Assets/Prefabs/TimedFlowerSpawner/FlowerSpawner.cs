using UnityEngine;

public class FlowerSpawner : MonoBehaviour, IUsable, IPickup
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Use()
    {
	    Debug.Log("Flower spawner Used");
    }

    public void StopUsing()
    {
    }

    public void Pickup()
    {
    }

    public void Drop()
    {
    }
}
