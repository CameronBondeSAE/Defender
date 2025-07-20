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
    private PlayerInputHandler playerInput;
    private PlayerInventory inventory;

    [Header("Item Type Names")]
    private string grenadeItemName = "Grenade";
    private string laserItemName = "Laser";

    [Header("Throw Settings")]
    [SerializeField] private float throwForce = 1f;
    [SerializeField] private Vector3 throwDirection = new Vector3(0, 0.1f, 0.3f); // Slight upward arc

    private void Awake()
    {
        playerInput = GetComponent<PlayerInputHandler>();
        if (playerInput == null)
            playerInput = FindObjectOfType<PlayerInputHandler>();
        inventory = GetComponent<PlayerInventory>();
        
        if (playerInput != null)
        {
            playerInput.onShootStart += StartShooting;
            playerInput.onShootStop += StopShooting;
            playerInput.onLaserStart += TryActivateLaser;
            playerInput.onLaserStop += TryDeactivateLaser;
            playerInput.onThrow += TryThrowGrenade;
        }
    }

    private void OnDestroy()
    {
        if (playerInput != null)
        {
            playerInput.onShootStart -= StartShooting;
            playerInput.onShootStop -= StopShooting;
            playerInput.onLaserStart -= TryActivateLaser;
            playerInput.onLaserStop -= TryDeactivateLaser;
            playerInput.onThrow -= TryThrowGrenade;
        }
    }

    #region Gun Combat (Always Available)
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
        Debug.Log("Bullet fired!");
    }
    #endregion

    #region Item-Based Combat
    private void TryThrowGrenade()
    {
        // Check if we have an item
        if (!inventory.HasItem)
        {
            Debug.Log("No item in inventory.");
            return;
        }
        // Check if it's a grenade
        if (!inventory.CurrentItem.Name.Equals(grenadeItemName, System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("Current item is not a grenade.");
            return;
        }
        // Check if we have an instance to throw
        if (inventory.CurrentItemInstance == null)
        {
            Debug.LogWarning("No item instance to throw!");
            return;
        }
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
        IUsableItem usableItem = itemToThrow.GetComponent<IUsableItem>();
        if (usableItem != null)
        {
            usableItem.UseItem();
        }

        // Clear from inventory (this will NOT destroy the thrown item)
        inventory.ClearCurrentItemWithoutDestroy();

        Debug.Log("Item thrown!");
    }

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
    #endregion

    #region Inventory Helpers
    private bool HasItemOfType(string itemName)
    {
        if (inventory == null || !inventory.HasItem) 
            return false;
        
        return inventory.CurrentItem.Name.Equals(itemName, System.StringComparison.OrdinalIgnoreCase);
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