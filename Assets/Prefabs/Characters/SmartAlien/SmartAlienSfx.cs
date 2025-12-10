using UnityEngine;
/// <summary>
/// This will need to be networked by gpg222 :)
/// </summary>
public class SmartAlienSfx : MonoBehaviour
{
    public smartAlienViewer alienView;
    
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
        alienView.DestoryAlienSfx_RPC();
    }

    public void PlayEscortStart()
    {
        PlayClip(escortStartClip);
        alienView.escortStart_RPC();
    }

    public void PlayThreatDisabled()
    {
        PlayClip(threatDisabledClip);
        alienView.playThreateDisabeld_RPC();
    }

    public void PlayCivsDroppedOff()
    {
        PlayClip(civsDroppedOffClip);
        alienView.DroppedCiv_RPC();
    }
}

