using System;
using UnityEngine;

public class DemoItemBase : UsableItem_Base
{
	protected override void Awake()
	{
		base.Awake();
		GetComponent<Renderer>().material.color = Color.white;
	}

	public override void Use()
	{
		base.Use(); 
		Debug.Log("DemoItem Used");
		GetComponent<Renderer>().material.color = Color.green;
	}

	public override void StopUsing()
	{
		base.StopUsing();
		Debug.Log("Stopped using");
		GetComponent<Renderer>().material.color = Color.red;
	}

	public override void Pickup()
	{
		base.Pickup(); // Plays pickup sound, etc
	}

	public override void Drop()
	{
		base.Drop(); // Plays drop sound, etc
	}
	protected override void ActivateItem()
	{
		Debug.Log("DemoItem ACTIVATED!");
		GetComponent<Renderer>().material.color = Color.yellow;
	}
}
