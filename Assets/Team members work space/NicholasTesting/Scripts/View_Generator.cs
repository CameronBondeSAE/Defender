using UnityEngine;

namespace NicholasScripts
{
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class View_Generator : MonoBehaviour
    {
        public AudioSource audioSource;
        public AudioClip generatorStarting;
        public AudioClip generatorRunning;
        
        public ParticleSystem sparkParticles;


        public void PlayStartupSound()
        {
            if (audioSource != null && generatorStarting != null)
            {
                audioSource.clip = generatorStarting;
                audioSource.loop = false;
                audioSource.Play();
            }
        }

        public void PlayRunningLoop()
        {
            if (audioSource != null && generatorRunning != null)
            {
                audioSource.clip = generatorRunning;
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        public void StopAudio()
        {
            if (audioSource != null)
                audioSource.Stop();
        }
        
        public void PlaySparks()
        {
            if (sparkParticles != null && !sparkParticles.isPlaying)
                sparkParticles.Play();
        }
        
        public void StopSparks()
        {
            if (sparkParticles != null && sparkParticles.isPlaying)
                sparkParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        public void DrawPowerRange(Vector3 position, float range)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(position, range);
        }
    }
}