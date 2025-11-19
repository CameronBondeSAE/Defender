using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class Light_Model : NetworkBehaviour
{
	[Header("Server only")]
	[SerializeField]
	private float toggleSpeed = 2f;

	[Header("Light Settings")]
	[Tooltip("The light to turn on and off")]
	[SerializeField]
	private Light light;

	private float timer;
	private bool  lightState;

	private void Update()
	{
		timer += Time.deltaTime;
		if (timer > toggleSpeed)
		{
			timer = 0f;
			ChangeColour_Rpc(); // TODO: WHAT colour??
		}
	}


	[Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
	public void ChangeColour_Rpc()
	{
		
	}
}