using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace DanniLi
{
    /// <summary>
    /// Sorry...I know it's not my job but I HAD to network this one for it to work/be tested
    /// </summary>
    public class Egg : NetworkBehaviour
    {
        [Header("Hatching Settings")]
        [Tooltip("your AI prefab that will spawn when this egg hatches. MUST have NetworkObject!")]
        [SerializeField] private GameObject alienToHatch;

        [Header("SFX")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip hatchSfx;

        private bool hasHatched;

        /// <summary>
        /// hatch will be called by the GameManager when it is this egg's turn to hatch.
        /// </summary>
        public void Hatch()
        {
            if (!IsServer || hasHatched)
                return;

            hasHatched = true;

            // spawn the AI
            if (alienToHatch != null)
            {
                GameObject aiInstance = Instantiate(alienToHatch, transform.position, transform.rotation);
                NetworkObject netObj = aiInstance.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    netObj.Spawn();
                }
            }

            // Play sound on all clients
            PlayHatchSfxRpc();

            // despawn
            NetworkObject eggNetObj = GetComponent<NetworkObject>();
            if (eggNetObj != null)
            {
                eggNetObj.Despawn(true);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
        private void PlayHatchSfxRpc()
        {
            if (audioSource != null && hatchSfx != null)
            {
                audioSource.PlayOneShot(hatchSfx);
            }
        }
    }
}