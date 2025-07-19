using System.Collections.Generic;
using UnityEngine;
using System;

using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField]
    private Transform itemHolder; // Where to parent the current item

    [Header("Current State")]
    [SerializeField]
    private ItemSO currentItem;
    
    [SerializeField]
    private GameObject currentItemInstance;

    [Header("Inventory Tracking")]
    [SerializeField]
    private List<ItemSO> itemsCollected = new List<ItemSO>();
    
    [SerializeField]
    private List<ItemSO> itemsUsed = new List<ItemSO>();

    // Events for UI updates
    public event Action<ItemSO> OnItemPickedUp;
    public event Action<ItemSO> OnItemUsed;
    public event Action OnItemSlotCleared;

    // Properties
    public bool HasItem => currentItem != null;
    public ItemSO CurrentItem => currentItem;
    public List<ItemSO> ItemsCollected => new List<ItemSO>(itemsCollected);
    public List<ItemSO> ItemsUsed => new List<ItemSO>(itemsUsed);

    private void Start()
    {
        // Create item holder if not assigned
        if (itemHolder == null)
        {
            GameObject holder = new GameObject("ItemHolder");
            holder.transform.SetParent(transform);
            holder.transform.localPosition = Vector3.zero;
            itemHolder = holder.transform;
        }
    }

    /// <summary>
    /// Attempts to pick up an item. Returns true if successful.
    /// </summary>
    public bool TryPickupItem(ItemSO item)
    {
        if (HasItem)
        {
            Debug.Log("Cannot pick up item - inventory is full!");
            return false;
        }

        if (item == null || item.ItemPrefab == null)
        {
            Debug.LogWarning("Cannot pick up item - item or prefab is null!");
            return false;
        }

        // Set current item
        currentItem = item;
        itemsCollected.Add(item);

        // Instantiate the item prefab
        currentItemInstance = Instantiate(item.ItemPrefab, itemHolder);
        currentItemInstance.transform.localPosition = Vector3.zero;
        currentItemInstance.transform.localRotation = Quaternion.identity;

        // Disable physics and colliders so it doesn't interfere or fall
        var rigidbody = currentItemInstance.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
        }

        var colliders = currentItemInstance.GetComponents<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        OnItemPickedUp?.Invoke(item);
        
        Debug.Log($"Picked up: {item.Name}");
        return true;
    }

    /// <summary>
    /// Uses the current item and clears the inventory slot
    /// </summary>
    public void UseCurrentItem()
    {
        if (!HasItem)
        {
            Debug.Log("No item to use!");
            return;
        }

        // Add to used items list
        itemsUsed.Add(currentItem);
        
        // Fire event before clearing
        OnItemUsed?.Invoke(currentItem);
        
        Debug.Log($"Used: {currentItem.Name}");

        // Clear current item
        if (currentItemInstance != null)
        {
            Destroy(currentItemInstance);
        }

        currentItem = null;
        currentItemInstance = null;

        OnItemSlotCleared?.Invoke();
    }
    /// Ok use these functions below if you guys would like to show UI popups or tell the player how they went
    /// ("you used x number of grenades! So aggressive!" etc.) at end of game
    /// <summary>
    /// checks if player has used a specific item
    /// </summary>
    public bool HasUsedItem(ItemSO item)
    {
        return itemsUsed.Contains(item);
    }
    /// <summary>
    /// checks if player has collected a specific item
    /// </summary>
    public bool HasCollectedItem(ItemSO item)
    {
        return itemsCollected.Contains(item);
    }
    /// <summary>
    /// gets the count of how many times an item has been used
    /// </summary>
    public int GetItemUsedCount(ItemSO item)
    {
        int count = 0;
        foreach (var usedItem in itemsUsed)
        {
            if (usedItem == item) count++;
        }
        return count;
    }
}
