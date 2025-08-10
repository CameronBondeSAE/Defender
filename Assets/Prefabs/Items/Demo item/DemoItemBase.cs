using System;
using Defender;
using NaughtyAttributes;
using Unity.Netcode;
using UnityEngine;

public class DemoItemBase : UsableItem_Base
{
	protected override void Awake()
	{
		base.Awake();
		GetComponent<Renderer>().material.color = Color.white;
	}

	public override void Use(CharacterBase characterTryingToUse)
	{
		base.Use(characterTryingToUse);
		if (activationCountdown > 0)
			StartActivationCountdown_LocalUI(Mathf.CeilToInt(activationCountdown));

		Use_Rpc();
	}

	[Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
	private void Use_Rpc()
	{
		Debug.Log("DemoItem Used");
		GetComponent<Renderer>().material.color = Color.green;
	}

	// TODO: Networking
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
		ActivateItem_Rpc();
	}

	[Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
	private void ActivateItem_Rpc()
	{
		Debug.Log("DemoItem ACTIVATED!");
		GetComponent<Renderer>().material.color = Color.yellow;
	}
}
