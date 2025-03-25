using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    // [Header("Ammo Data")]
    // public BulletData bulletData;
    // public GrenadeData grenadeData;
    
    [Header("Ammo Paras")]
    public Transform throwPosition;
    public Transform firePosition;
    
    [Header("Prefabs")]
    public GameObject bulletPrefab;
    public GameObject grenadePrefab;

    public void FireBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePosition.position, firePosition.rotation);
        //Bullet bulletScript = bullet.GetComponent<Bullet>();
    }

    public void ThrowGrenade()
    {
        GameObject grenade = Instantiate(grenadePrefab, throwPosition.position, throwPosition.rotation);
        Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();
        //Grenade grenadeScript = grenade.GetComponent<Grenade>();
        Vector3 launchDirection = throwPosition.forward + throwPosition.up * 0.2f;
        //grenadeRb.AddForce(launchDirection * grenadeScript.launchSpeed, ForceMode.VelocityChange);
    }
}
