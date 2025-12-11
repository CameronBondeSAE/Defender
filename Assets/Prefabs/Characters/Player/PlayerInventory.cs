using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine;
using System;
using Defender;
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
	// added netvar to sync held items to clients
	private NetworkVariable<NetworkObjectReference> networkedHeldItem = new NetworkVariable<NetworkObjectReference>(
		default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	[SerializeField]
	private GameObject currentItemInstance;

	public GameObject CurrentItemInstance
	{
		get => currentItemInstance;
		private set => currentItemInstance = value;
	}

	// public bool HasItem => CurrentItem != null && CurrentItemInstance != null;

	public bool HasItem => CurrentItemInstance != null;

	public PlayerInputHandler2 playerInput;
	public event Action<IPickup> OnItemPickedUp;
	private void Start()
	{
		if (itemHolder == null)
		{
			Debug.LogError("ItemHolder is not assigned! Please assign it in the inspector.");
		}
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		networkedHeldItem.OnValueChanged += OnHeldItemChanged;
		if (!IsServer && networkedHeldItem.Value.TryGet(out NetworkObject heldObj))
		{
			OnHeldItemChanged(default, networkedHeldItem.Value);
		}
	}

	public override void OnNetworkDespawn()
	{
		if (networkedHeldItem != null)
		{
			networkedHeldItem.OnValueChanged -= OnHeldItemChanged;
		}
		base.OnNetworkDespawn();
	}

	private void OnHeldItemChanged(NetworkObjectReference previous, NetworkObjectReference current)
	{
		if (previous.TryGet(out NetworkObject prevObj))
		{
			Debug.Log($"[OnHeldItemChanged] Clearing previous item: {prevObj.name}");
			ClearItemVisually(prevObj.gameObject);
		}
		if (current.TryGet(out NetworkObject newObj))
		{
			Debug.Log($"[OnHeldItemChanged] Setting up new item: {newObj.name}");
			SetupItemVisually(newObj.gameObject);
            
			CurrentItem = newObj.GetComponent<IPickup>();
			CurrentItemInstance = newObj.gameObject;
			OnItemPickedUp?.Invoke(CurrentItem);
			ShowHeldItemInfo_FromCurrent();
		}
		else
		{
			CurrentItem = null;
			CurrentItemInstance = null;
			RequestHideHeldItemInfoRpc();
		}
	}

	private void SetupItemVisually(GameObject item)
	{
		// parent to item holder
		item.transform.SetParent(itemHolder);
		item.transform.localPosition = Vector3.zero;
		item.transform.localRotation = Quaternion.identity;
        
		// make kinematic and disable colliders
		Rigidbody rb = item.GetComponent<Rigidbody>();
		if (rb != null)
		{
			rb.isKinematic = true;
			rb.useGravity = false;
		}
        
		Collider[] colliders = item.GetComponentsInChildren<Collider>();
		foreach (var col in colliders)
		{
			col.enabled = false;
		}
	}

	private void ClearItemVisually(GameObject item)
	{
		item.transform.SetParent(null);
		Collider[] colliders = item.GetComponentsInChildren<Collider>();
		foreach (var col in colliders)
		{
			col.enabled = true;
		}
	}

	void LateUpdate()
	{
		if (HasItem)
		{
			if (CurrentItemInstance != null)
				CurrentItemInstance.transform.SetPositionAndRotation(
				                                                     itemHolder.position,
				                                                     itemHolder.rotation
				                                                    );
		}
	}
	public bool TryPickupItem(IPickup item)
	{
		if (!IsServer)
		{
			return false;
		}

		if (item == null)
		{
			Debug.Log("[TryPickupItem] Item is null.");
			return false;
		}
		if (HasItem)
		{
			Debug.Log("[TryPickupItem] Inventory full, dropping held item.");
			DropHeldItem();
		}
		// Get concrete references to real GOs
		CurrentItem         = item;
		CurrentItemInstance = (CurrentItem as MonoBehaviour)?.gameObject;
		if (CurrentItemInstance == null)
		{
			Debug.LogWarning("[TryPickupItem] CurrentItemInstance is null!");
			return false;
		}
		NetworkObject itemNetObj = CurrentItemInstance.GetComponent<NetworkObject>();
		if (itemNetObj == null) return false;
		
		
		Debug.Log($"[TryPickupItem] Parenting {CurrentItemInstance.name} to itemHolder {itemHolder.name}");
		// CurrentItemInstance.transform.SetParent(itemHolder);
		SetupItemForInventory(CurrentItemInstance);
		CurrentItemInstance.transform.localPosition = Vector3.zero;
		CurrentItemInstance.transform.localRotation = Quaternion.identity;
		CurrentItem.Pickup(GetComponent<CharacterBase>());
		CurrentItemInstance.GetComponent<UsableItem_Base>().CurrentCarrier = transform;
		// Update the networked variable (this will trigger OnHeldItemChanged on all clients)
		// update the held netvar
		networkedHeldItem.Value = itemNetObj;
		OnItemPickedUp?.Invoke(item);
		Debug.Log($"[TryPickupItem] Picked up: {CurrentItemInstance?.name}");
		return true;
	}

	public bool DropHeldItem()
	{
		if (!IsServer)
		{
			return false;
		}
		if (!HasItem)
		{
			return false;
		}

		// Move item to fire position
		if (CurrentItemInstance != null)
		{
			Vector3 dropPosition = itemHolder.position + transform.forward * 1.5f;
			// call drop on interface (for sfx)
			CurrentItem?.Drop();

			currentItemInstance.transform.rotation = transform.rotation;
			
			// Use the item base to Drop
			UsableItem_Base usableItem = CurrentItemInstance.GetComponent<UsableItem_Base>();
			if (usableItem != null)
			{
				usableItem.Drop(dropPosition);
			}
			else
			{	
				// for cases if it's not using my UsableItemBase
				CurrentItemInstance.transform.position = dropPosition;
				currentItemInstance.transform.rotation = Quaternion.Euler(0f, 0f, itemHolder.transform.rotation.z); // does this fix the item rotation when placed on the ground
				CurrentItemInstance.transform.SetParent(null, true);
				Rigidbody rb = CurrentItemInstance.GetComponent<Rigidbody>();
				if (rb != null)
				{
					rb.isKinematic = false;
					rb.useGravity = true;
					rb.AddForce(transform.forward * smallDropForce, ForceMode.VelocityChange);
				}
				Collider[] colliders = CurrentItemInstance.GetComponentsInChildren<Collider>();
				foreach (var col in colliders)
				{
					col.enabled = true;
				}
			}
		}
		networkedHeldItem.Value = default;
		CurrentItem = null;
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
		currentUsable?.Use(GetComponent<CharacterBase>());
		// TODO: Need the item to tell us if it's been destroy. Event we sub to on collecting? Yes
	}

	/// <summary>
	/// Clears the current item from inventory
	/// </summary>
	public void ClearCurrentItemWithoutDestroy()
	{
		if (!IsServer)
		{
			Debug.LogWarning("Should only be called on server!");
			return;
		}
		if (!HasItem)
		{
			Debug.Log("No item to clear!");
			return;
		}
		Debug.Log("[PlayerInventory] Clearing item");
		networkedHeldItem.Value = default;
		CurrentItem = null;
		CurrentItemInstance = null;
	}

	#region Item Description UI Rpcs

	[Rpc(SendTo.Server)]
	public void RequestShowHeldItemInfoRpc(NetworkObjectReference itemRef)
	{
		string nameStr = string.Empty, descStr = string.Empty;

		if (itemRef.TryGet(out NetworkObject itemNO) &&
		    itemNO && itemNO.TryGetComponent<IDescribable>(out var describable))
		{
			nameStr = describable.ItemName ?? string.Empty;
			descStr = describable.Description ?? string.Empty;
		}

		ShowHeldItemInfoRpc(nameStr, descStr); 
	}
	public void ShowHeldItemInfo_FromCurrent()
	{
		NetworkObject networkObject = null;
		if (currentItemInstance) networkObject = currentItemInstance.GetComponent<NetworkObject>();
		var itemRef = (networkObject != null) ? new NetworkObjectReference(networkObject) : default;
		RequestShowHeldItemInfoRpc(itemRef);
	}

	[Rpc(SendTo.Server)]
	public void RequestHideHeldItemInfoRpc()
	{
		HideHeldItemInfoRpc();
	}

	[Rpc(SendTo.Owner)]
	private void ShowHeldItemInfoRpc(string nameStr, string descStr)
	{
		var ui = FindFirstObjectByType<DanniLi.UIManager>();
		if (ui != null) ui.ShowItemPanel(nameStr, descStr);
	}

	[Rpc(SendTo.Owner)]
	private void HideHeldItemInfoRpc()
	{
		var ui = FindFirstObjectByType<DanniLi.UIManager>();
		if (ui != null) ui.HideItemPanel();
	}

	#endregion
}