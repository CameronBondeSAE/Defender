using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class NetworkedSoundController : NetworkBehaviour
{
    [Header("Setup")]
    [SerializeField] private AudioSource audioSource;

    [Header("Sequential Voiceline Clips (played in order)")]
    [SerializeField] private List<AudioClip> audioClips = new List<AudioClip>();

    private int currentClipIndex = 0;
    private bool isPlayingOnServer = false;

    [Header("Looping Laser SFX Settings")]
    [SerializeField] private AudioClip laserLoopClip;
    [SerializeField] private float defaultFadeOutDuration = 0.4f;

    private bool isLaserLooping = false;
    private Coroutine fadeOutCoroutine;
    private float baseVolume = 1f;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            baseVolume = audioSource.volume;
        }
    }

    // ------------------ EXISTING ONE-SHOT SEQUENTIAL PLAY ------------------ //

    public void PlayVoicelinesAudioClip()
    {
        if (!IsServer)
            return;

        if (audioClips == null || audioClips.Count == 0)
            return;

        if (isPlayingOnServer)
            return;

        // Don't trigger one-shots while laser loop is active
        if (isLaserLooping)
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

        audioSource.loop = false;
        audioSource.clip = clip;
        audioSource.volume = baseVolume;
        audioSource.Play();
    }

    // ------------------ LASER LOOP + FADE-OUT ------------------ //

    /// <summary>
    /// Called by server when laser attack starts.
    /// Starts looping laser SFX on all clients.
    /// </summary>
    public void StartLaserLoop()
    {
        if (!IsServer)
            return;

        if (isLaserLooping)
            return;

        if (laserLoopClip == null || audioSource == null)
            return;

        isLaserLooping = true;
        StartLaserLoopClient_Rpc();
    }

    /// <summary>
    /// Called by server when laser attack ends.
    /// Fades out laser SFX on all clients.
    /// </summary>
    public void StopLaserLoop()
    {
        StopLaserLoop(defaultFadeOutDuration);
    }

    public void StopLaserLoop(float fadeOutDuration)
    {
        if (!IsServer)
            return;

        if (!isLaserLooping)
            return;

        isLaserLooping = false;
        StopLaserLoopClient_Rpc(fadeOutDuration);
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void StartLaserLoopClient_Rpc()
    {
        if (audioSource == null || laserLoopClip == null)
            return;

        // Stop any current fade-out
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = null;
        }

        audioSource.clip = laserLoopClip;
        audioSource.loop = true;
        audioSource.volume = baseVolume;
        audioSource.Play();
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void StopLaserLoopClient_Rpc(float fadeDuration)
    {
        if (audioSource == null)
            return;

        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
        }

        fadeOutCoroutine = StartCoroutine(FadeOutCoroutine(fadeDuration));
    }

    private IEnumerator FadeOutCoroutine(float fadeDuration)
    {
        if (!audioSource.isPlaying)
            yield break;

        if (fadeDuration <= 0f)
        {
            audioSource.Stop();
            audioSource.volume = baseVolume;
            audioSource.loop = false;
            yield break;
        }

        float startVolume = audioSource.volume;
        float time = 0f;

        while (time < fadeDuration && audioSource.isPlaying)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        audioSource.Stop();
        audioSource.loop = false;
        audioSource.volume = baseVolume;
    }
}
