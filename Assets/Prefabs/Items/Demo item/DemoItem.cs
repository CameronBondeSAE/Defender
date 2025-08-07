using System;
using UnityEngine;

public class DemoItem : MonoBehaviour, IUsable, IPickup
{
	public AudioSource audioSource;
	public AudioClip   pickupClip;
	public AudioClip   dropClip;
	
	private void Awake()
	{
		GetComponent<Renderer>().material.color = Color.white;
	}

	public void Use()
    {
	    Debug.Log("DemoItem Used");
	    GetComponent<Renderer>().material.color = Color.green;
    }

    public void StopUsing()
    {
	    Debug.Log("Stopped using");
	    GetComponent<Renderer>().material.color = Color.red;
    }

    public void Pickup()
    {
	    audioSource.PlayOneShot(pickupClip);
    }

    public void Drop()
    {
	    audioSource.PlayOneShot(dropClip);
    }
}
