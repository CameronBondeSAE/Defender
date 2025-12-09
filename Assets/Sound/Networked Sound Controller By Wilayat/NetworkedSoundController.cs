using UnityEngine;
using Unity.Netcode;

public class NetworkedSoundController : NetworkBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void PlayAudioClip()
    {
        if (!IsServer)
            return;

        PlayAudioClipClient_Rpc();
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void PlayAudioClipClient_Rpc()
    {
        if (audioSource == null || audioClip == null)
            return;

        audioSource.PlayOneShot(audioClip);
    }
}