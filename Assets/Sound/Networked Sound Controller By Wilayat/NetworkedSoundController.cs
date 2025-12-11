using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class NetworkedSoundController : NetworkBehaviour
{
    [Header("Setup")]
    [SerializeField] private AudioSource audioSource;

    [Header("Clips (played in order)")]
    [SerializeField] private List<AudioClip> audioClips = new List<AudioClip>();

    private int currentClipIndex = 0;

    private bool isPlayingOnServer = false;

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

        if (audioClips == null || audioClips.Count == 0)
            return;

        if (isPlayingOnServer)
            return;

        int clipIndexToPlay = currentClipIndex;

        currentClipIndex++;
        if (currentClipIndex >= audioClips.Count)
        {
            currentClipIndex = 0;
        }

        AudioClip clipToPlay = audioClips[clipIndexToPlay];
        if (clipToPlay == null)
            return;

        isPlayingOnServer = true;

        PlayAudioClipClient_Rpc(clipIndexToPlay);

        StartCoroutine(ServerCooldownCoroutine(clipToPlay.length));
    }

    private IEnumerator ServerCooldownCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        isPlayingOnServer = false;
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void PlayAudioClipClient_Rpc(int clipIndex)
    {
        if (audioSource == null)
            return;

        if (audioClips == null || audioClips.Count == 0)
            return;

        if (clipIndex < 0 || clipIndex >= audioClips.Count)
            return;

        AudioClip clip = audioClips[clipIndex];
        if (clip == null)
            return;

        audioSource.clip = clip;
        audioSource.Play();
    }
}
