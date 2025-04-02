using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    // [Header("Ammo Data")]
    // public BulletData bulletData;
    // public GrenadeData grenadeData;
    
    [Header("Ammo Params")]
    public Transform throwPosition;
    public Transform firePosition;
    public float fireRate = 0.2f; 
    public bool isShooting;
    private Coroutine shootCoroutine;
    
    [Header("Refs")]
    public GameObject bulletPrefab;
    public GameObject grenadePrefab;
    private PlayerInputHandler playerInput;
    
    void Awake()
    {
        playerInput = FindObjectOfType<PlayerInputHandler>(); // Ensure we get the input handler
        if (playerInput != null)
        {
            playerInput.onShootStart += StartShooting;
            playerInput.onShootStop += StopShooting;
            playerInput.onLaserStart += ActivateLaser;
            playerInput.onLaserStop += DeactivateLaser;
            playerInput.onThrow += ThrowGrenade;
        }
    }

    void OnDestroy()
    {
        if (playerInput != null)
        {
            playerInput.onShootStart -= StartShooting;
            playerInput.onShootStop -= StopShooting;
            playerInput.onLaserStart -= ActivateLaser;
            playerInput.onLaserStop -= DeactivateLaser;
            playerInput.onThrow -= ThrowGrenade;
        }
    }
    
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
            PlayerEventManager.instance.events.onShoot.Invoke();
            FireBullet();
            yield return new WaitForSeconds(fireRate);
        }
    }

    private void ActivateLaser()
    {
        // laser logic
    }
    
    private void DeactivateLaser()
    {
        // laser logic
    }

    public void FireBullet()
    {
        if (bulletPrefab == null || firePosition == null) return;
        GameObject bullet = Instantiate(bulletPrefab, firePosition.position, firePosition.rotation);
        //Debug.Log("Bullet fired at: " + firePosition.forward);
    }

    public void ThrowGrenade()
    {
        if (grenadePrefab == null || throwPosition == null) return;
        GameObject grenade = Instantiate(grenadePrefab, throwPosition.position, throwPosition.rotation);
        // Grenade grenadeScript = grenade.GetComponent<Grenade>();
        // grenadeScript.Launch();
    }
}
