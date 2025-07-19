using UnityEngine;

[RequireComponent(typeof(PlayerInventory))]
public class PlayerInteract : MonoBehaviour
{
    [Header("References")]
    private PlayerInputHandler inputHandler;
    private PlayerInventory inventory;

    [Header("Level Settings")]
    [SerializeField]
    private LevelInfo currentLevelInfo;

    private void Start()
    {
        // Get required components
        inputHandler = GetComponent<PlayerInputHandler>();
        inventory = GetComponent<PlayerInventory>();

        if (inputHandler == null)
        {
            Debug.LogError("PlayerInputHandler not found on player!");
        }

        if (inventory == null)
        {
            Debug.LogError("PlayerInventory not found on player!");
        }

        // Subscribe to input events
        if (inputHandler != null)
        {
            inputHandler.onInteract += HandleInteract;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (inputHandler != null)
        {
            inputHandler.onInteract -= HandleInteract;
        }
    }

    private void HandleInteract()
    {
        // Find the closest pickup in range
        PickupItem pickup = FindClosestPickup();
        
        if (pickup != null)
        {
            TryPickupItem(pickup);
        }
        else
        {
            // Try to use current item if no pickup is available
            if (inventory.HasItem)
            {
                UseCurrentItem();
            }
        }
    }

    private PickupItem FindClosestPickup()
    {
        // Find all pickup items in the scene
        PickupItem[] pickups = FindObjectsOfType<PickupItem>();
        PickupItem closestPickup = null;
        float closestDistance = float.MaxValue;

        foreach (var pickup in pickups)
        {
            if (pickup.IsPlayerInRange(transform))
            {
                float distance = Vector3.Distance(transform.position, pickup.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPickup = pickup;
                }
            }
        }

        return closestPickup;
    }

    private void TryPickupItem(PickupItem pickup)
    {
        if (inventory.HasItem)
        {
            Debug.Log("Inventory is full! Use your current item first.");
            return;
        }

        // Get random item from current level
        if (currentLevelInfo == null)
        {
            Debug.LogWarning("No level info assigned! Cannot generate random item.");
            return;
        }

        ItemSO randomItem = currentLevelInfo.GetRandomItem();
        if (randomItem == null)
        {
            Debug.LogWarning("No items available in current level!");
            return;
        }

        // Try to pickup the item
        if (inventory.TryPickupItem(randomItem))
        {
            // Successfully picked up - destroy the pickup object
            pickup.OnPickedUp();
        }
    }

    private void UseCurrentItem()
    {
        if (inventory.HasItem)
        {
            // Get the current item instance and try to use it
            var itemInstance = inventory.transform.GetComponentInChildren<IUsableItem>();
            
            if (itemInstance != null)
            {
                itemInstance.UseItem();
            }
            
            // Clear from inventory
            inventory.UseCurrentItem();
        }
    }

    /// <summary>
    /// Set the current level info (call this when loading a new level)
    /// </summary>
    public void SetLevelInfo(LevelInfo levelInfo)
    {
        currentLevelInfo = levelInfo;
    }
}

// Interface for items that can be used
public interface IUsableItem
{
    void UseItem();
}