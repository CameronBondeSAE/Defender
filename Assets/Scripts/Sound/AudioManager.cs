using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    // enum for audio types
    public enum AudioType
    {
        //Shared
        Walk,
        Run,
        Idle,
        Shoot,
        LoadAmmo,
        
        // Player's
        PlayerHit,
        PlayerDeath,
        
        // Alien's
        AlienGrowl,
        AlienAlert,
        AlienHit,
        AlienDeath,
        
        // Weapon effects
        BulletHit,
        Explosion,
        
        // Music
        BackgroundMusic,
        BattleMusic,
    }

    [Header("Movement Sounds")]
    public AudioClip walk;
    public AudioClip run;
    public AudioClip idle;
    public AudioClip shoot;
    public AudioClip loadAmmo;
    
    [Header("Player Sounds")]
    public AudioClip playerHit;
    public AudioClip playerDeath;
    
    [Header("Alien Sounds")]
    public AudioClip alienGrowl;
    public AudioClip alienAlert;
    public AudioClip alienHit;
    public AudioClip alienDeath;
    
    [Header("Weapon sounds")]
    public AudioClip bulletHit;
    public AudioClip explosion;
    
    [Header("Music")]
    public AudioClip backgroundMusic;
    public AudioClip battleMusic;
    
    private AudioSource musicSource;
    private List<AudioSource> audioSources = new List<AudioSource>();
    private float globalVolume = 0.5f; // Default volume

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        musicSource = gameObject.AddComponent<AudioSource>();
    }

    // Play a one-shot sound effect
    public void PlayAudio(AudioType type, float volume = -1f)
    {
        AudioClip clip = GetAudioClip(type);
        if (clip == null) return;

        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.clip = clip;
        newSource.volume = (volume >= 0) ? volume : globalVolume;
        newSource.Play();

        audioSources.Add(newSource);
        Destroy(newSource, clip.length); // Auto-destroy source after playing
    }
    
    // Plays a looping one-shot sound effect, for things like walk sound
    public AudioSource PlayLoopingAudio(AudioType type, float volume = -1f)
    {
        AudioClip clip = GetAudioClip(type);
        if (clip == null) return null;

        AudioSource loopingSource = gameObject.AddComponent<AudioSource>();
        loopingSource.clip = clip;
        loopingSource.volume = (volume >= 0) ? volume : globalVolume;
        loopingSource.loop = true;
        loopingSource.Play();

        audioSources.Add(loopingSource);
        return loopingSource; // Return reference for manual stopping
    }

    // Stops a specific looping audio source
    public void StopLoopingAudio(AudioSource loopingSource)
    {
        if (loopingSource != null)
        {
            loopingSource.Stop();
            audioSources.Remove(loopingSource);
            Destroy(loopingSource);
        }
    }
    
    // plays looping background music (when called once)
    public void PlayMusic(float volume = -1f)
    {
        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.volume = (volume >= 0) ? volume : globalVolume;
        musicSource.Play();
    }
    
    // Stops the currently playing music
    public void StopMusic()
    {
        musicSource.Stop();
    }
     // Set a new global volume level for all sounds
    public void SetGlobalVolume(float newVolume)
    {
        globalVolume = newVolume;
        foreach (AudioSource source in audioSources)
        {
            if (source != null) source.volume = globalVolume;
        }
        if (musicSource != null) musicSource.volume = globalVolume;
    }

    public float GetCurrentVolume()
    {
        return globalVolume;
    }

    // Private helper method to get the correct AudioClip
    private AudioClip GetAudioClip(AudioType type)
    {
        switch (type)
        {
            case AudioType.Walk: return walk;
            case AudioType.Run: return run;
            case AudioType.Idle: return idle;
            case AudioType.Shoot: return shoot;
            case AudioType.LoadAmmo: return loadAmmo;
            
            case AudioType.PlayerHit: return playerHit;
            case AudioType.PlayerDeath: return playerDeath;
            
            case AudioType.AlienGrowl: return alienGrowl;
            case AudioType.AlienHit: return alienHit;
            case AudioType.AlienDeath: return alienDeath;
            case AudioType.AlienAlert: return alienAlert;
            
            case AudioType.BulletHit: return bulletHit;
            case AudioType.Explosion: return explosion;
            
            case AudioType.BackgroundMusic: return backgroundMusic;
            case AudioType.BattleMusic: return battleMusic;
            default:
                Debug.LogError("Audio type not found: " + type);
                return null;
        }
    }
}
