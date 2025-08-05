using System.Collections.Generic;
using UnityEngine;
using System;

using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerInventory : MonoBehaviour
{
   [Header("Inventory Settings")]
    public Transform itemHolder; // Where to parent the current item

    [Header("Current State")]
    // public ItemSO CurrentItem { get; private set; }
    public IUsable CurrentItem { get; private set; }
    public GameObject CurrentItemInstance { get; private set; }
    // public bool HasItem => CurrentItem != null && CurrentItemInstance != null;
    public bool HasItem => itemHolder.parent != null;

    // [Header("Inventory Tracking")]
    // private List<ItemSO> itemsCollected = new List<ItemSO>();
    // private List<ItemSO> itemsUsed = new List<ItemSO>();
    // private List<ItemSO> availableItems = new List<ItemSO>();
    // public IReadOnlyList<ItemSO> AvailableItems => availableItems;

    // Events for UI updates if needed
    // public event Action<ItemSO> OnItemPickedUp;
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
    /// Make item kinematic and disable colliders while in inventory so it doesn't fall forever...
    /// </summary>
    private void SetupItemForInventory(GameObject item)
    {
        // no physics
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        // nio colliders
        Collider[] colliders = item.GetComponents<Collider>();
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
            Debug.Log("No item to use!");
            return;
        }
        
        CurrentItem.Use();
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
