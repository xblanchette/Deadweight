using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{

    public static SoundManager instance;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
   public void PlaySound(AudioSource audio, float volume = 0, AudioClip clip = null)
    {
        audio.volume = volume;
        audio.PlayOneShot(clip);

    }
   public void PlayRadomSound(AudioSource audio, float volume,List<AudioClip> clips)
    {
        audio.volume=volume;
        audio.PlayOneShot(clips[Random.Range(0, clips.Count)]);
    }
   public void PlayRadomPitch(AudioSource audio,float minPitch, float maxPitch, float volume, AudioClip clip)
    {
        audio.volume = volume;
        audio.pitch = Random.Range(minPitch,maxPitch);
        audio.PlayOneShot(clip);
    }

}
