using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UseAudioExamples : MonoBehaviour
{
    // One shot sounds
    public void PlayExplosionAudio()
    {
        // you can pass in a desired volume at the end
        AudioManager.Instance.PlayAudio(AudioManager.AudioType.Explosion, 0.7f);
    }
    
    public void PlayAlienDeathAudio()
    {
        AudioManager.Instance.PlayAudio(AudioManager.AudioType.AlienDeath, 0.8f);
    }
    // Lopping sounds
    public void PlayWalkAudio()
    {
        StartCoroutine(Walkaudio());
    }
    IEnumerator Walkaudio() // Coroutine just to demonstrate. When use just copy what's inside of it.
    {
        // you need to store this sound when you call it because these sounds needs to be manually stopped.
        AudioSource loopingWalkSound = AudioManager.Instance.PlayLoopingAudio(AudioManager.AudioType.Walk);
        yield return new WaitForSeconds(5f);
        AudioManager.Instance.StopLoopingAudio(loopingWalkSound);
    }

    public void PlayBackgroundMusic()
    {
        // plays background music at 50% volume
        AudioManager.Instance.PlayMusic(0.5f);
    }

    public void StopBackgroundMusic()
    {
        AudioManager.Instance.StopMusic();
    }

    public void AdjustGlobalVolume(float volume)
    {
        AudioManager.Instance.SetGlobalVolume(0.6f);
        // AudioManager.Instance.SetGlobalVolume(volume);
    }

    public void GetCurrentVolume()
    {
        float currentVolume = AudioManager.Instance.GetCurrentVolume();
        Debug.Log("Current Volume: " + currentVolume);
    }
    
}
