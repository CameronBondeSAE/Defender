using Unity.Netcode;
using UnityEngine;

namespace NicholasScripts
{
    /// <summary>
    /// Turret MVC view: spawns bullets and plays audio/particle effects when firing.
    /// </summary>
    public class View_Turret : NetworkBehaviour
    {
        /// <summary>
        /// Nicholas's old Monobehaviour code
        /// </summary>
        //     [SerializeField] private Transform firePoint;
        //     [SerializeField] private GameObject bulletPrefab;
        //     [SerializeField] private AudioSource fireAudio;
        //     [SerializeField] private ParticleSystem muzzleFlash;
        //
        //     public void Fire()
        //     {
        //         if (bulletPrefab != null && firePoint != null)
        //         {
        //             Debug.DrawRay(firePoint.position, firePoint.forward * 2f, Color.red, 0.1f);
        //             Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        //             if (fireAudio != null) fireAudio.Play();
        //             if (muzzleFlash != null) muzzleFlash.Play();
        //         }
        //     }
        //
        //     public void FireEffect()
        //     {
        //         if (muzzleFlash != null)
        //             muzzleFlash.Play();
        //     }
        //
        //     public Transform GetFirePoint() => firePoint;
        // }

        [SerializeField] private Transform firePoint;

        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private AudioSource fireAudio;
        [SerializeField] private ParticleSystem muzzleFlash;

        public void FireServer()
        {
            if (!IsServer || bulletPrefab == null || firePoint == null) return;
            Vector3 firePosition = firePoint.position + firePoint.forward * 0.15f;
            Quaternion fireRotation = firePoint.rotation;

            var gameObject = Instantiate(bulletPrefab, firePosition, fireRotation);
            var netObj = gameObject.GetComponent<NetworkObject>();
            if (netObj != null) netObj.Spawn(true);
            FireEffectClientRpc(firePosition, fireRotation);
        }

        public void Fire()
        {
            if (bulletPrefab == null || firePoint == null) return;
            var pos = firePoint.position + firePoint.forward * 0.15f;
            var rot = firePoint.rotation;
            Instantiate(bulletPrefab, pos, rot);
            FireEffect(pos, rot);
        }

        public void FireEffect(Vector3 position, Quaternion rotation)
        {
            if (muzzleFlash != null)
            {
                muzzleFlash.transform.SetPositionAndRotation(position, rotation);
                muzzleFlash.Play();
            }

            if (fireAudio != null) fireAudio.Play();
        }

        public void FireEffect()
        {
            if (fireAudio) fireAudio.Play(); 
            if (muzzleFlash) muzzleFlash.Play();
        }

        [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Unreliable)]
        private void FireEffectClientRpc(Vector3 position, Quaternion rotation) => FireEffect(position, rotation);

        public Transform GetFirePoint() => firePoint;
    }
}