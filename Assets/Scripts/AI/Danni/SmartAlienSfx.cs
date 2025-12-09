using UnityEngine;
/// <summary>
/// This will need to be networked by gpg222 :)
/// </summary>
public class SmartAlienSfx : MonoBehaviour
{
    public AudioSource audioSource;

    [Header("Clips")]
    public AudioClip destroyItemClip;
    public AudioClip escortStartClip;
    public AudioClip threatDisabledClip;
    public AudioClip civsDroppedOffClip;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void PlayClip(AudioClip clip)
    {
        if (audioSource == null) return;
        if (clip == null) return;

        audioSource.PlayOneShot(clip);
    }

    public void PlayDestroyItem()
    {
        PlayClip(destroyItemClip);
    }

    public void PlayEscortStart()
    {
        PlayClip(escortStartClip);
    }

    public void PlayThreatDisabled()
    {
        PlayClip(threatDisabledClip);
    }

    public void PlayCivsDroppedOff()
    {
        PlayClip(civsDroppedOffClip);
    }
}

