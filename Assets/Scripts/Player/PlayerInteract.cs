using UnityEngine;

[RequireComponent(typeof(PlayerInventory))]
public class PlayerInteract : MonoBehaviour
{
   [Header("References")]
    private PlayerInputHandler inputHandler;
    private PlayerInventory inventory;

    private void Start()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        inventory = GetComponent<PlayerInventory>();

        if (inputHandler == null)
            Debug.LogError("PlayerInputHandler not found on player");

        if (inventory == null)
            Debug.LogError("PlayerInventory not found on player");

        if (inputHandler != null)
            inputHandler.onInteract += HandleInteract;
    }

    private void OnDestroy()
    {
        if (inputHandler != null)
            inputHandler.onInteract -= HandleInteract;
    }

    private void HandleInteract()
    {
        PickupItem pickup = FindClosestPickup();

        if (pickup != null)
        {
            TryPickupItem(pickup);
        }
        else if (inventory.HasItem)
        {
            UseCurrentItem();// This is where direct-use items get handled
        }
    }

    /// <summary>
    /// This method handles item pick up, leave it as is
    /// </summary>
    /// <returns></returns>
    private PickupItem FindClosestPickup() 
    {
        PickupItem[] pickups = FindObjectsOfType<PickupItem>();
        PickupItem closest = null;
        float closestDistance = float.MaxValue;

        foreach (var pickup in pickups)
        {
            if (pickup.IsPlayerInRange(transform))
            {
                float distance = Vector3.Distance(transform.position, pickup.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = pickup;
                }
            }
        }

        return closest;
    }
    private void TryPickupItem(PickupItem pickup)
    {
        if (inventory.HasItem)
        {
            Debug.Log("Inventory is full! Use your current item first.");
            return;
        }

        var availableItems = inventory.AvailableItems;
        if (availableItems == null || availableItems.Count == 0)
        {
            Debug.LogWarning("No items available to pick up!");
            return;
        }

        ItemSO randomItem = availableItems[Random.Range(0, availableItems.Count)];

        if (randomItem == null)
        {
            Debug.LogWarning("Item from list is null!");
            return;
        }

        if (inventory.TryPickupItem(randomItem))
        {
            pickup.OnPickedUp();
        }
    }

    private void UseCurrentItem()
    {
        if (!inventory.HasItem) return;

        // Use the item through its interface
        IUsable usable = inventory.CurrentItemInstance?.GetComponent<IUsable>();

        if (usable != null)
        {
            usable.Use();
            // ================================================================
            // ADD SPECIAL HANDLING FOR CONSUMABLE ITEMS HERE
             // Example Pattern for consumable items (potions, food, etc.):
             //
             // if (HasItemOfType("Health Potion"))
             // { 
             //     // Do healing logic here or let the potion component handle it
             //    inventory.UseCurrentItem(); // This destroys the item
             // }
             // else if (HasItemOfType("Key"))
             // {
             //     // Keys might be consumed when used on doors
             //     // Let the door/key interaction handle consumption
             // }
             //  else if (HasItemOfType("Sci-FiScroll"))
             //  {
             //     // Scrolls are usually consumed when cast
             //      inventory.UseCurrentItem();
             // }
             
            // For now, only consume items that are meant to be consumed immediately
            // Items like grenades, lasers, etc. should NOT be consumed here! 
            // They get consumed when thrown/used through PlayerCombat!
            // ================================================================
            inventory.UseCurrentItem();
        }
        else
        {
            Debug.Log("Current item is not directly usable through interact. Use specific controls (throw, laser, etc.)");
        }
    }
}
