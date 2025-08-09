using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;

public class PlayerInventory : NetworkBehaviour
{
	[SerializeField]
	float smallDropForce = 10f;

	[Header("Inventory Settings")]
	public Transform itemHolder; // Where to parent the current item

	[Header("Current State")]
	// public ItemSO CurrentItem { get; private set; }
	public IPickup CurrentItem { get; private set; }
	

	[SerializeField]
	private GameObject currentItemInstance;

	public GameObject CurrentItemInstance
	{
		get => currentItemInstance;
		private set => currentItemInstance = value;
	}

	// public bool HasItem => CurrentItem != null && CurrentItemInstance != null;

	public bool HasItem => CurrentItem != null; // TODO: Redundant now

	public PlayerInputHandler2 playerInput;

	// [Header("Inventory Tracking")]
	// private List<ItemSO> itemsCollected = new List<ItemSO>();
	// private List<ItemSO> itemsUsed = new List<ItemSO>();
	// private List<ItemSO> availableItems = new List<ItemSO>();
	// public IReadOnlyList<ItemSO> AvailableItems => availableItems;

	// Events for UI updates if needed
	public event Action<IPickup> OnItemPickedUp;
	// public event Action<ItemSO> OnItemUsed;
	// public event Action OnItemSlotCleared;

	// public List<ItemSO> ItemsCollected => new List<ItemSO>(itemsCollected);
	// public List<ItemSO> ItemsUsed => new List<ItemSO>(itemsUsed);

	private void Start()
	{
		if (itemHolder == null)
		{
			Debug.LogError("ItemHolder is not assigned! Please assign it in the inspector.");
		}
	}


	void LateUpdate()
	{
		if (HasItem)
		{
			CurrentItemInstance.transform.SetPositionAndRotation(
			                                 itemHolder.position,
			                                 itemHolder.rotation
			                                );
		}
	}

	/// <summary>
	/// Registers list of available items on this level from game manager
	/// </summary>
	// public void RegisterAvailableItem(ItemSO item)
	// {
	//     if (!availableItems.Contains(item))
	//     {
	//         availableItems.Add(item);
	//         Debug.Log($"Registered item: {item.name}");
	//     }
	// }

	/// <summary>
	/// Tries to pick up an item. Returns true if successful.
	/// </summary>
	// public bool TryPickupItem(IPickup item)
	// public bool TryPickupItem(NetworkObjectReference _item)
	public bool TryPickupItem(IPickup item)
	{
		// NetworkObject itemObject;
		// _item.TryGet(out itemObject);
		//
		// if (itemObject == null)
		// {
		// 	Debug.LogWarning("Item object is null!");
		// 	return false;
		// }
		// IPickup item = itemObject.GetComponent<IPickup>();
		
		// if (HasItem)
		// {
		// 	Debug.Log("Cannot pick up item - inventory is full! - Dropping instead");
		// 	DropHeldItem();
		// 	return false;
		// }
		//
		// if(item == null)
		// 	return false;
		//
		// // Set current item
		// CurrentItem         = item;
		// CurrentItemInstance = (CurrentItem as MonoBehaviour)?.gameObject;
		// if (CurrentItemInstance != null)
		// {
		// 	CurrentItemInstance.transform.SetParent(itemHolder);
		// 	CurrentItemInstance.transform.localPosition = Vector3.zero;
		// 	CurrentItemInstance.transform.localRotation = Quaternion.identity;
		//
		// 	// Sets up this item to be held in inventory (kinematic)
		// 	SetupItemForInventory(CurrentItemInstance);
		// }
		//
		// OnItemPickedUp?.Invoke(item);
		// Debug.Log($"Picked up: {CurrentItemInstance.name}");
		//
		// return true;

		Debug.Log($"[TryPickupItem] Called for: {(item as MonoBehaviour)?.name}");
		if (HasItem)
		{
			Debug.Log("[TryPickupItem] Inventory full, dropping held item.");
			DropHeldItem();
			return false;
		}

		if (item == null)
		{
			Debug.Log("[TryPickupItem] Item is null.");
			return false;
		}

		CurrentItem         = item;
		CurrentItemInstance = (CurrentItem as MonoBehaviour)?.gameObject;
		if (CurrentItemInstance != null)
		{
			Debug.Log($"[TryPickupItem] Parenting {CurrentItemInstance.name} to itemHolder {itemHolder.name}");
			// CurrentItemInstance.transform.SetParent(itemHolder);
			SetupItemForInventory(CurrentItemInstance);
			
			CurrentItemInstance.transform.localPosition = Vector3.zero;
			CurrentItemInstance.transform.localRotation = Quaternion.identity;
			CurrentItem.Pickup();
			CurrentItemInstance.GetComponent<UsableItem_Base>().CurrentCarrier = transform;
		}
		else
		{
			Debug.LogWarning("[TryPickupItem] CurrentItemInstance is null!");
		}

		OnItemPickedUp?.Invoke(item);
		Debug.Log($"[TryPickupItem] Picked up: {CurrentItemInstance?.name}");
		return true;
	}

	public bool DropHeldItem()
	{
		if (!HasItem)
		{
			return false;
		}

		// Move item to fire position
		CurrentItemInstance.transform.position = itemHolder.position + transform.forward * 1.5f;
		CurrentItemInstance.transform.rotation = Quaternion.identity;

		// Unparent the item from player
		// CurrentItemInstance.transform.SetParent(null);

		// TODO: This has moved to UsableItem_Base
		// Re-enable physics
		// Rigidbody rb = CurrentItemInstance.GetComponent<Rigidbody>();
		// if (rb != null)
		// {
		// 	rb.isKinematic = false;
		// 	rb.useGravity  = true;
		//
		// 	// Apply throwing force
		// 	Vector3 worldThrowDirection = transform.forward;
		// 	
		// 	// TODO might be better to leave up to items themselves
		// 	rb.AddForce(worldThrowDirection * smallDropForce, ForceMode.VelocityChange);
		// }
		// else
		// {
		// 	Debug.LogWarning("Item doesn't have a Rigidbody component!");
		// }

		// Re-enable colliders
		Collider[] colliders = CurrentItemInstance.GetComponentsInChildren<Collider>();
		foreach (var col in colliders)
		{
			col.enabled = true;
		}

		CurrentItem         = null;
		CurrentItemInstance = null;
		
		return true;
	}

	/// <summary>
	/// Make item kinematic and disable colliders while in inventory so it doesn't fall forever...
	/// </summary>
	private void SetupItemForInventory(GameObject item)
	{
		// no physics
		Rigidbody rb = item.GetComponent<Rigidbody>();
		if (rb != null)
		{
			rb.isKinematic = true;
			rb.useGravity  = false;
		}

		// no colliders
		Collider[] colliders = item.GetComponentsInChildren<Collider>();
		foreach (var col in colliders)
		{
			col.enabled = false;
		}
	}

	/// <summary>
	/// Uses the current item and clears the inventory slot
	/// </summary>
	public void UseCurrentItem()
	{
		if (!HasItem)
		{
			Debug.Log("No item to use! Will try and use anything in front of me");
			return;
		}

		IUsable currentUsable = CurrentItem as IUsable;
		currentUsable?.Use();
		// TODO: Need the item to tell us if it's been destroy. Event we sub to on collecting? Yes


		// // Add to used items list
		// itemsUsed.Add(CurrentItem);
		//
		// // Fire event before clearing
		// OnItemUsed?.Invoke(CurrentItem);
		//
		// Debug.Log($"Used: {CurrentItem.Name}");
		//
		// // Destroy the item instance and clear references
		// if (CurrentItemInstance != null)
		// {
		//     Destroy(CurrentItemInstance);
		// }

		// ClearInventorySlot();
	}

	/// <summary>
	/// Clears the current item from inventory
	/// </summary>
	public void ClearCurrentItemWithoutDestroy()
	{
		if (!HasItem)
		{
			Debug.Log("No item to clear!");
			return;
		}

		// Add to used items list for tracking
		// itemsUsed.Add(CurrentItem);
		// Fire event before clearing
		// OnItemUsed?.Invoke(CurrentItem);
		// Debug.Log($"Used: {CurrentItem.Name}");
		// Clear references without destroying the instance (the latter is handled in PlayerCombat)
		ClearInventorySlot();
	}

	/// <summary>
	/// Clears the inventory slot references
	/// </summary>
	private void ClearInventorySlot()
	{
		// CurrentItem = null;
		// CurrentItemInstance = null;
		// OnItemSlotCleared?.Invoke();
	}

	/// Here are some helper functions if you guys want to show UI messages etc about player's item usage info :)
	/// <summary>
	/// Checks if player has used a specific item
	/// </summary>
	// public bool HasUsedItem(ItemSO item)
	// {
	//     return itemsUsed.Contains(item);
	// }
	/// <summary>
	/// Checks if player has collected a specific item
	/// </summary>
	// public bool HasCollectedItem(ItemSO item)
	// {
	//     return itemsCollected.Contains(item);
	// }
	/// <summary>
	/// Gets the count of how many times an item has been used
	/// e.g. At end of game, UI shows "you have used X amount of grenade! You love it."
	/// </summary>
	// public int GetItemUsedCount(ItemSO item)
	// {
	//     int count = 0;
	//     foreach (var usedItem in itemsUsed)
	//     {
	//         if (usedItem == item) count++;
	//     }
	//     return count;
	// }
}