using UnityEngine;
using System.Collections.Generic;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager instance;
    [SerializeField] private AudioSource soundFXObject;

    // Track all active audio sources
    private List<AudioSource> activeSounds = new List<AudioSource>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume = 1f)
    {
        // Stop all previous sounds
        StopAllSounds();

        // spawn in gameobject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        // assign audioClip
        audioSource.clip = audioClip;

        // assign value
        audioSource.volume = volume;

        // play sound
        audioSource.Play();

        // track this sound
        activeSounds.Add(audioSource);

        // get length of sound
        float clipLength = audioSource.clip.length;

        // destroy the clip after it is played
        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlayRandomSoundFXClip(AudioClip[] audioClip, Transform spawnTransform, float volume = 1f)
    {
        // Stop all previous sounds
        StopAllSounds();

        int randomIndex = Random.Range(0, audioClip.Length);
        
        // spawn in gameobject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        // assign audioClip
        audioSource.clip = audioClip[randomIndex];

        // assign value
        audioSource.volume = volume;

        // play sound
        audioSource.Play();

        // track this sound
        activeSounds.Add(audioSource);

        // get length of sound
        float clipLength = audioSource.clip.length;

        // destroy the clip after it is played
        Destroy(audioSource.gameObject, clipLength);
    }

    // Stop all active sounds
    public void StopAllSounds()
    {
        foreach (AudioSource source in activeSounds.ToArray())
        {
            if (source != null)
            {
                source.Stop();
                Destroy(source.gameObject);
            }
        }
        activeSounds.Clear();
    }

    // Clean up destroyed objects from the list
    void Update()
    {
        activeSounds.RemoveAll(source => source == null);
    }
}