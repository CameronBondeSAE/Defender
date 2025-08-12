using Defender;
using Unity.Netcode;
using UnityEngine;

public class DemoItemBase : UsableItem_Base
{
	protected override void Awake()
	{
		base.Awake();
		
		GetComponent<Renderer>().material.color = Color.white;
	}

	// Run Server side only
	public override void Use(CharacterBase characterTryingToUse)
	{
		base.Use(characterTryingToUse);

		if (activationCountdown > 0)
			StartActivationCountdown_LocalUI(Mathf.CeilToInt(activationCountdown));

		
		UseClient_Rpc();
	}

	[Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
	private void UseClient_Rpc()
	{
		Debug.Log("DemoItem Used");

		GetComponent<Renderer>().material.color = Color.green;
	}

	protected override void ActivateItem()
	{
		base.ActivateItem();
		
		ActivateItemClient_Rpc();
	}

	[Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
	private void ActivateItemClient_Rpc()
	{
		Debug.Log("DemoItem ACTIVATED!");
		GetComponent<Renderer>().material.color = Color.yellow;
	}
	
	
	
	// TODO: Networking
	public override void StopUsing()
	{
		base.StopUsing();
		
		Debug.Log("Stopped using");
		GetComponent<Renderer>().material.color = Color.red;
	}

	// TODO: Networking
	public override void Pickup()
	{
		base.Pickup(); // Plays pickup sound, etc
	}

	// TODO: Networking

	public override void Drop()
	{
		base.Drop(); // Plays drop sound, etc
	}
}
