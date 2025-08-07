using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Gun Combat")]
    [SerializeField] private Transform firePosition;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private GameObject bulletPrefab;
    
    public bool isShooting;
    private Coroutine shootCoroutine;

    [Header("References")]
    private PlayerInputHandler2 playerInput;
    private PlayerInventory inventory;

    [Header("Item Type Names")]
    private string grenadeItemName = "Grenade";
    private string laserItemName = "Laser";
    // ================================================================
    // When you guys add items, ADD THEIR NAME (STRING) HERE
     // * Examples:
     // * private string trapItemName = "Trap";
     // * private string healItemName = "Heal";
     // ================================================================

    [Header("Throw Settings")]
    [SerializeField] private float throwForce = 1f;
    [SerializeField] private Vector3 throwDirection = new Vector3(0, 0.1f, 0.3f); // Slight upward arc

    private void Awake()
    {
        playerInput = GetComponent<PlayerInputHandler2>();
        if (playerInput == null)
            playerInput = FindObjectOfType<PlayerInputHandler2>();
        inventory = GetComponent<PlayerInventory>();
        
        if (playerInput != null)
        {
            // playerInput.onShootStart += StartShooting;
            // playerInput.onShootStop += StopShooting;
            // playerInput.onLaserStart += TryActivateLaser;
            // playerInput.onLaserStop += TryDeactivateLaser;
            // playerInput.onThrow += TryThrowGrenade;
            // ================================================================
            // If you guys have your own custom input keys for your items, ADD NEW INPUT EVENT SUBSCRIPTIONS HERE
            // * Examples:
            // * playerInput.onSetTrap += SettingTrap;
            // * playerInput.onDrinkHeal += UseHeal;
            // ================================================================
            
            // =============================================================================
            // DON'T FORGET TO UPDATE PLAYERINPUTHANDLER TOO!!
             // If your item needs new input keys, you'll need to:
             // 1. Add the input detection in PlayerInputHandler
             // 2. Create events for your new inputs
             // 3. Subscribe to those events in PlayerCombat (like above)
             // 4. ASSIGN those keys in Player's inspector (in PlayerInputHandler field)!
             // =============================================================================
        }
    }

    private void OnDestroy()
    {
        if (playerInput != null)
        {
            // playerInput.onShootStart -= StartShooting;
            // playerInput.onShootStop -= StopShooting;
            // playerInput.onLaserStart -= TryActivateLaser;
            // playerInput.onLaserStop -= TryDeactivateLaser;
            // playerInput.onThrow -= TryThrowGrenade;
            // ================================================================
            // UNSUBSCRIBE ANY NEW INPUT EVENTS HERE
            // ================================================================
        }
    }

    #region Gun Combat (Always Available)
    /// <summary>
    /// So, currently the gun is always available, can omit if you guys don't want.
    /// Makes the game a little more interactive and less dangerous, and you can adjust fireRate to increase difficulty
    /// Comment out if you don't want the player to have a gun. If you guys are making guns yourselves,
    /// you can just use these methods with the shoot input already set up.
    /// </summary>
    private void StartShooting()
    {
        if (!isShooting)
        {
            isShooting = true;
            shootCoroutine = StartCoroutine(ShootContinuously());
        }
    }

    private void StopShooting()
    {
        isShooting = false;
        if (shootCoroutine != null)
        {
            StopCoroutine(shootCoroutine);
            shootCoroutine = null;
        }
    }

    private IEnumerator ShootContinuously()
    {
        while (isShooting)
        {
            FireBullet();
            yield return new WaitForSeconds(fireRate);
        }
    }

    public void FireBullet()
    {
        if (bulletPrefab == null || firePosition == null) return;

        var bullet = Instantiate(bulletPrefab, firePosition.position, firePosition.rotation);
        // Debug.Log("Bullet fired!");
    }
    #endregion

    #region Item-Based Combat
    /// <summary>
    /// TEMPLATE 1: Throwable Items (like grenades, bombs, etc.)
    /// These methods will throw an item from your inventory. All items are single use.
    /// For general bombs and throwables you guys can just use this template.
    /// </summary>
    private void TryThrowGrenade()
    {
        // Check if we have an item
        if (!inventory.HasItem)
        {
            Debug.Log("No item in inventory.");
            return;
        }
        // Check if it's a grenade
        // if (!inventory.CurrentItem.Name.Equals(grenadeItemName, System.StringComparison.OrdinalIgnoreCase))
        // {
        //     Debug.Log("Current item is not a grenade.");
        //     return;
        // }

        // ================================================================
        // INSERT MORE THROWABLE ITEM NAME CHECKS HERE
        // Example:
        // if (!inventory.CurrentItem.Name.Equals(flashingBombItemName, System.StringComparison.OrdinalIgnoreCase))
        // {
        //     Debug.Log("Current item is not a grenade.");
        //     return;
        // }
        // Or check for inherited component :), like if your custom bomb is inheriting from basic grenade
        // ================================================================

        // Check if we have an instance to throw
        // if (inventory.CurrentItemInstance == null)
        // {
        //     Debug.LogWarning("No item instance to throw!");
        //     return;
        // }
        ThrowCurrentItem();
    }

    private void ThrowCurrentItem()
    {
        GameObject itemToThrow = inventory.CurrentItemInstance;

        if (itemToThrow == null)
        {
            Debug.LogWarning("No item instance to throw!");
            return;
        }

        // Move item to fire position
        itemToThrow.transform.position = firePosition.position;
        itemToThrow.transform.rotation = firePosition.rotation;

        // Unparent the item from player
        itemToThrow.transform.SetParent(null);

        // Re-enable physics
        Rigidbody rb = itemToThrow.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            // Apply throwing force
            Vector3 worldThrowDirection = firePosition.TransformDirection(throwDirection.normalized);
            rb.AddForce(worldThrowDirection * throwForce, ForceMode.VelocityChange);
        }
        else
        {
            Debug.LogWarning("Item doesn't have a Rigidbody component!");
        }

        // Re-enable colliders
        Collider[] colliders = itemToThrow.GetComponents<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = true;
        }

        // Activate the item's behavior ("use" it)
        IUsable usable = itemToThrow.GetComponent<IUsable>();
        if (usable != null)
        {
            usable.Use();
        }

        // Clear from inventory (this will NOT destroy the thrown item)
        inventory.ClearCurrentItemWithoutDestroy();

        Debug.Log("Item thrown!");
    }

    /// <summary>
    /// TEMPLATE 2: Toggle items like lasers, flashlights, swords etc.
    /// Same as before, here's a template to use.
    /// </summary>
    private void TryActivateLaser()
    {
        if (!HasItemOfType(laserItemName))
        {
            Debug.Log("No laser in inventory!");
            return;
        }

        var laserComponent = GetCurrentItemComponent<ILaserItem>();
        if (laserComponent != null)
        {
            laserComponent.ActivateLaser();
            Debug.Log("Laser activated!");
        }
    }

    private void TryDeactivateLaser()
    {
        if (!HasItemOfType(laserItemName))
        {
            return;
        }

        var laserComponent = GetCurrentItemComponent<ILaserItem>();
        if (laserComponent != null)
        {
            laserComponent.DeactivateLaser();
            Debug.Log("Laser deactivated!");
        }
    }

    /// <summary>
    /// MORE TEMPLATE FOR NEW & YOUR CUSTOM INSTANT-USE ITEMS (potions, sci-fi spells):
    /// </summary>
    /// <param name="itemName"></param>
    /// <returns></returns>
    //
    // private void TryUseYourItem()
    //  {
    //      if (!HasItemOfType(yourItemName))
    //         {
    //             Debug.Log("No [your item] in inventory!");
    //             return;
    //         }
    //
    //      var component = GetCurrentItemComponent<IYourItemInterface>();
    //      if (component != null)
    //       {
    //             component.Use(); // or Attack(), Cast(), etc.
    //            Debug.Log("[Your item] used!");
    //         }
    //  } // you get the gist of it :)
    #endregion

    #region Inventory Helpers
    // These are just some methods to help verify (any) items in player's inventory :)
    private bool HasItemOfType(string itemName)
    {
        if (inventory == null || !inventory.HasItem)
            return false;

        return false;// BUG inventory.CurrentItem.Name.Equals(itemName, System.StringComparison.OrdinalIgnoreCase);
    }
    private T GetCurrentItemComponent<T>() where T : class
    {
        if (inventory == null || !inventory.HasItem || inventory.CurrentItemInstance == null)
            return null;

        return inventory.CurrentItemInstance.GetComponent<T>();
    }
    #endregion
}
// example interface for laser items
public interface ILaserItem
{
    void ActivateLaser();
    void DeactivateLaser();
}