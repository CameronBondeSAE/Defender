using UnityEngine;

namespace NicholasScripts
{
    public class View_Turret : MonoBehaviour
    {
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private AudioSource fireAudio;
        [SerializeField] private ParticleSystem muzzleFlash;

        public void Fire()
        {
            if (bulletPrefab != null && firePoint != null)
            {
                Debug.DrawRay(firePoint.position, firePoint.forward * 2f, Color.red, 2f);
                Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                // Debug.Log("Firing bullet...");
                if (fireAudio != null) fireAudio.Play();
                if (muzzleFlash != null) muzzleFlash.Play();
            }
        }
        
        public void FireEffect()
        {
            if (muzzleFlash != null)
                muzzleFlash.Play();
        }

        public Transform GetFirePoint() => firePoint;
    }
}