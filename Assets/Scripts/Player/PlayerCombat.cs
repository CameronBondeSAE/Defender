using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Bullet")]
    public GameObject bulletPrefab;
    [SerializeField] float bulletSpeed;
    public Transform firePosition;
    
    [Header("Grenade")]
    public GameObject grenadePrefab;
    public Transform handPosition;
    public float throwForce = 10f;

    public void FireBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePosition.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = firePosition.right * bulletSpeed; 
        }
    }

    public void ThrowGrenade()
    {
        GameObject grenade = Instantiate(grenadePrefab, handPosition.position, Quaternion.identity);
        Rigidbody2D rb = grenade.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 throwDirection = firePosition.right + Vector3.up; 
            rb.AddForce(throwDirection * throwForce, ForceMode2D.Impulse);
        }
    }
}
