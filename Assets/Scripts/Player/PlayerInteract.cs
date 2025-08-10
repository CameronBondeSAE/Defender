using Defender;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerInventory))]
public class PlayerInteract : NetworkBehaviour
{
	[Header("Pickup Settings")]
	[SerializeField]
	private float interactionRange = 2f;

	public Transform interactMount;
	
	private bool playerInRange = false;

	[Header("References")]
	private PlayerInputHandler2 inputHandler;

	private PlayerInventory inventory;

	private void Start()
	{
		inputHandler = GetComponent<PlayerInputHandler2>();
		inventory    = GetComponent<PlayerInventory>();

		if (inputHandler == null)
			Debug.LogError("PlayerInputHandler not found on player");

		if (inventory == null)
			Debug.LogError("PlayerInventory not found on player");

		if (inputHandler != null)
		{
			inputHandler.onInventory += HandleInventory;
			inputHandler.onUse += InputHandlerOnonUse;
		}
	}

	private void OnDestroy()
	{
		if (inputHandler != null)
		{
			inputHandler.onInventory -= HandleInventory;
			inputHandler.onUse -= InputHandlerOnonUse;
		}
	}

	private void InputHandlerOnonUse(bool obj)
	{
		// If client, request server to try pickup item
		if(IsClient)
		{
			RequestTryUseItem_Rpc();
		}
	}

	[Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
	private void RequestTryUseItem_Rpc()
	{
		// Use the held item if holding
		if (inventory.HasItem)
		{
			inventory.UseCurrentItem();
			return;
		}
		
		// Otherwise use nearby floor item
		IUsable pickup = FindClosestUsable();

		if (pickup != null)
		{
			pickup.Use(GetComponent<CharacterBase>());
		}
	}

	private void HandleInventory()
	{
		// If client, request server to try pickup item
		if(IsClient)
		{
			RequestTryPickupItem_Rpc();
		}
	}

	[Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
	private void RequestTryPickupItem_Rpc()
	{
		if (inventory.HasItem)
		{
			// if already holding, drop it
			inventory.DropHeldItem();
		}
		else
		{
			IPickup pickup = FindClosestPickup();

			// MonoBehaviour monoBehaviour = pickup as MonoBehaviour;
			// if (monoBehaviour != null) 
			// 	inventory.TryPickupItem(monoBehaviour.GetComponent<NetworkObject>());
			if (pickup != null) 
				inventory.TryPickupItem(pickup);
		}
	}

	/// <summary>
	/// Finds nearest IUsable implementation
	/// </summary>
	/// <returns></returns>
	private IUsable FindClosestUsable()
	{
		Collider[] collidersInRange       = new Collider[10];
		Physics.OverlapSphereNonAlloc(interactMount.position, interactionRange, collidersInRange);
		
		IUsable[] pickups;
		IUsable   closest         = null;
		float     closestDistance = float.MaxValue;

		foreach (Collider c in collidersInRange)
		{
			if( c == null ) continue; // Because I preallocate a fixed array size
			
			Debug.DrawLine(transform.position, c.transform.position, Color.green, 1f);
			Debug.Log("Nearby usable = "+c.name);
			IUsable pickup = c.GetComponent<IUsable>(); // Assumes collider and IUsables are on the same GO
			if (pickup != null)
			{
				float distance = Vector3.Distance(transform.position, ((MonoBehaviour) pickup).transform.position);
				if (distance < closestDistance)
				{
					closestDistance = distance;
					closest         = pickup;
				}
			}
		}

		return closest;
	}
	/// <summary>
	/// Finds nearest IPickup implementation
	/// </summary>
	/// <returns></returns>
	private IPickup FindClosestPickup()
	{
		Collider[] collidersInRange       = new Collider[10];
		Physics.OverlapSphereNonAlloc(interactMount.position, interactionRange, collidersInRange);
		
		IPickup[] pickups;
		IPickup   closest         = null;
		float     closestDistance = float.MaxValue;

		foreach (Collider c in collidersInRange)
		{
			if( c == null ) continue; // Because I preallocate a fixed array size
			
			Debug.DrawLine(transform.position, c.transform.position, Color.green, 1f);
			Debug.Log("Nearby pickup = "+c.name);
			IPickup pickup = c.GetComponent<IPickup>(); // Assumes collider and IUsables are on the same GO
			if (pickup != null)
			{
				float distance = Vector3.Distance(transform.position, ((MonoBehaviour) pickup).transform.position);
				if (distance < closestDistance)
				{
					closestDistance = distance;
					closest         = pickup;
				}
			}
		}

		return closest;
	}

	/// <summary>
	/// Check if player is in range for pickup
	/// </summary>
	public bool IsPlayerInRange(Transform item)
	{
		float distance = Vector3.Distance(transform.position, item.position);
		return distance <= interactionRange;
	}
}