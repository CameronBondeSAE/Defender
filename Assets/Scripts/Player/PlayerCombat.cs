using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Bullet")]
    public Ammo bullet;
    public Transform firePosition;
    
    [Header("Grenade")]
    public Ammo grenade;
    public Transform handPosition;
    public float throwForce = 10f;

    public void FireBullet()
    {
        // GameObject bullet = Instantiate(bulletPrefab, firePosition.position, Quaternion.identity);
        // Rigidbody rb = bullet.GetComponent<Rigidbody>();
        // if (rb != null)
        // {
        //     rb.linearVelocity = firePosition.right * bulletSpeed; 
        // }
        if (bullet != null)
        {
            bullet.Hit(null);
        }
    }

    public void ThrowGrenade()
    {
        // GameObject grenade = Instantiate(grenadePrefab, handPosition.position, Quaternion.identity);
        // Rigidbody rb = grenade.GetComponent<Rigidbody>();
        // if (rb != null)
        // {
        //     Vector2 throwDirection = firePosition.right + Vector3.up; 
        //     rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        // }
        if (grenade != null)
        {
            grenade.Hit(null);
        }
    }
}
