using Unity.Netcode;
using UnityEngine;

namespace NicholasScripts
{
    /// <summary>
    /// Generator MVC view: handles playing audio, particle effects, and visual range for the generator.
    /// </summary>
    [System.Serializable]
    public class View_Generator : NetworkBehaviour
    {
        public AudioSource audioSource;
        public AudioClip generatorStarting;
        public AudioClip generatorRunning;

        public ParticleSystem sparkParticles;

        // ====== SERVER CALLS ======
        public void PlayStartupSoundServer()
        {
            if (!IsServer) return;
            PlayStartupSound();
            PlayStartupSoundClientRpc();
        }

        public void PlayRunningLoopServer()
        {
            if (!IsServer) return;
            PlayRunningLoop();
            PlayRunningLoopClientRpc();
        }

        public void StopAudioServer()
        {
            if (!IsServer) return;
            StopAudio();
            StopAudioClientRpc();
        }

        public void PlaySparksServer()
        {
            if (!IsServer) return;
            PlaySparks();
            PlaySparksClientRpc();
        }

        public void StopSparksServer()
        {
            if (!IsServer) return;
            StopSparks();
            StopSparksClientRpc();
        }

        // ====== CLIENT RPCs ======
        [Rpc(SendTo.ClientsAndHost)]
        private void PlayStartupSoundClientRpc() => PlayStartupSound();

        [Rpc(SendTo.ClientsAndHost)]
        private void PlayRunningLoopClientRpc() => PlayRunningLoop();

        [Rpc(SendTo.ClientsAndHost)]
        private void StopAudioClientRpc() => StopAudio();

        [Rpc(SendTo.ClientsAndHost)]
        private void PlaySparksClientRpc() => PlaySparks();

        [Rpc(SendTo.ClientsAndHost)]
        private void StopSparksClientRpc() => StopSparks();

        // ====== LOCAL METHODS (unchanged) ======
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
