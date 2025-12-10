using UnityEngine;

// attach the required components if neeeded automatically if not assigned
[RequireComponent(typeof(AudioSource), typeof(AudioLowPassFilter))]
public class SoundOcclusion : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform listener;
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioLowPassFilter filter;

    [Header("Sound Occlusion Volume Settings")]
    [SerializeField] private float originalVolume;
    [SerializeField] private float occludedVolume = 0.3f;
    [SerializeField] private float occludedCutoff = 800f;
    [SerializeField] private float normalCutoff = 22000f;
    [SerializeField] private float fadeSpeed = 5f;

    private void Start()
    {
        // find the listener obj if needed, right now it 
        listener = FindFirstObjectByType<AudioListener>().transform;

        // uh oh, no listener was found
        if (listener == null) Debug.Log("No Audio Listener in the Scene" + Time.time);

        // get the required components
        source = GetComponent<AudioSource>();
        filter = GetComponent<AudioLowPassFilter>();

        // set the sources original volume
        originalVolume = source.volume;
    }

    private void Update()
    {
        // exit script if no listeners found
        if (listener == null) return;

        // Raycast check instead of linecast need extra variables, is it faster to do linecast?
        // is there a better way to do this then put it in the Update event?

        //Vector3 direction = listener.position - transform.position;
        //float distance = dir.magnitude;
        //bool blocked = Physics.Raycast(transform.position, direction.normalized, distance, obstructionMask);

        // set a bool based on whether the line/ray is blocked from listener to obj making sound
        bool blocked = Physics.Linecast(transform.position, listener.position, obstructionMask);

        // set occluded vol based on if blocked
        float targetVolume = blocked ? occludedVolume : originalVolume;

        // set cutoff based on occulsion
        float targetCutoff = blocked ? occludedCutoff : normalCutoff;

        // lerp the volume of the sound source
        source.volume = Mathf.Lerp(source.volume, targetVolume, Time.deltaTime * fadeSpeed);

        // set the filter frequency
        filter.cutoffFrequency = Mathf.Lerp(filter.cutoffFrequency, targetCutoff, Time.deltaTime * fadeSpeed);
    }
}