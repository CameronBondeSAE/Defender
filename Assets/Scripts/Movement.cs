using UnityEngine;
using System.Collections;

namespace mothershipScripts
{
    /// <summary>
    /// Movement used for the mothership
    /// </summary>

    public class Movement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed;

        [SerializeField] protected AudioClip[] engineSounds;
        private AudioSource audioSource;

        void Start()
        {
            audioSource = transform.GetComponent<AudioSource>();
        }

        public IEnumerator MoveToADestination(Vector3 destination)
        {
            PlayRandomEngineSound();
            while ((destination - transform.position).magnitude > 0.01f)
            {
                transform.position = Vector3.Lerp(transform.position, destination, moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = destination; //Snap position destination position
            audioSource.Stop();
        }

        protected void PlayRandomEngineSound()
        {
            int randomIndex = Random.Range(0, engineSounds.Length - 1);
            audioSource.clip = engineSounds[randomIndex];
            audioSource.Play();
        }
    }
}
