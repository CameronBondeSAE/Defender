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

    // Which clip to play next (0..Count-1, then loops)
    private int currentClipIndex = 0;

    // Server-side flag so we don't trigger a new sound while one is "in progress"
    private bool isPlayingOnServer = false;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    /// <summary>
    /// Call this from your AI / gameplay code.
    /// Only the server is allowed to start networked audio.
    /// </summary>
    public void PlayAudioClip()
    {
        if (!IsServer)
            return;

        if (audioClips == null || audioClips.Count == 0)
            return;

        // If we're still in the "cooldown" for the last clip, ignore this call.
        if (isPlayingOnServer)
            return;

        // Decide which clip to play this time.
        int clipIndexToPlay = currentClipIndex;

        // Advance the index for next time (looping).
        currentClipIndex++;
        if (currentClipIndex >= audioClips.Count)
        {
            currentClipIndex = 0;
        }

        AudioClip clipToPlay = audioClips[clipIndexToPlay];
        if (clipToPlay == null)
            return;

        isPlayingOnServer = true;

        // Tell all clients (and host) to play this specific clip.
        PlayAudioClipClient_Rpc(clipIndexToPlay);

        // Start a server-side coroutine that waits for this clip's duration
        // before allowing another sound.
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
