using UnityEngine;
using Unity.Netcode;

public class CivilianSoundController : NetworkBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip abductedScreamClip;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void PlayAbductedScream()
    {
        if (!IsServer)
            return;

        PlayScreamClient_Rpc();
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void PlayScreamClient_Rpc()
    {
        if (audioSource == null || abductedScreamClip == null)
            return;

        audioSource.PlayOneShot(abductedScreamClip);
    }
}