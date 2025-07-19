using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Gun Combat")]
    [SerializeField]
    private Transform firePosition;
    
    [SerializeField]
    private float fireRate = 0.2f;
    
    [SerializeField]
    private GameObject bulletPrefab;
    
    public bool isShooting;
    private Coroutine shootCoroutine;

    [Header("References")]
    private PlayerInputHandler playerInput;
    private PlayerInventory inventory;

    [Header("Item Type Names")]
    [SerializeField]
    private string grenadeItemName = "Grenade";
    
    [SerializeField]
    private string laserItemName = "Laser";

    private void Awake()
    {
        playerInput = GetComponent<PlayerInputHandler>();
        if (playerInput == null)
            playerInput = FindObjectOfType<PlayerInputHandler>();
        inventory = GetComponent<PlayerInventory>();
        if (playerInput != null)
        {
            // gun shooting is always available, just to make the game a bit more accessible 
            // can adjust shoot interval/amount for increased difficulty
            playerInput.onShootStart += StartShooting;
            playerInput.onShootStop += StopShooting;
            // Item-based actions now (only work if player has the item)
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
        if (!HasItemOfType(grenadeItemName))
        {
            Debug.Log("No grenade in inventory!");
            return;
        }
        // Get the grenade from inventory and use it
        var grenadeComponent = GetCurrentItemComponent<BasicGrenade>();
        if (grenadeComponent != null)
        {
            grenadeComponent.UseItem();
            inventory.UseCurrentItem(); // Remove from inventory
            Debug.Log("Grenade thrown!");
        }
    }

    private void TryActivateLaser()
    {
        if (!HasItemOfType(laserItemName))
        {
            Debug.Log("No laser in inventory!");
            return;
        }

        // Get the laser component and activate it
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
            return; // No laser to deactivate
        }

        // Get the laser component and deactivate it
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
        if (inventory == null || !inventory.HasItem) 
            return null;

        // Look for the component in the current item instance
        var itemTransform = inventory.transform.GetComponentInChildren<Transform>();
        if (itemTransform != null)
        {
            return itemTransform.GetComponent<T>();
        }

        return null;
    }
    #endregion
}

// example interface for laser items
public interface ILaserItem
{
    void ActivateLaser();
    void DeactivateLaser();
}