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
            UseCurrentItem();
        }
    }

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
        IUsableItem usable = inventory.CurrentItemInstance?.GetComponent<IUsableItem>();

        if (usable != null)
        {
            usable.UseItem();
            // * For items like potions that are consumed immediately,
            // Add functionality here (or create a similar function) to handle its immediate destruction
            inventory.UseCurrentItem();
        }
        else
        {
            Debug.Log("Current item is not directly usable through interact. Use specific controls (throw, laser, etc.)");
        }
    }
}
