using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class MusicManager : MonoBehaviour
{
    [SerializeField] AudioClip introMusic, gameplayMusic;
    AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        if (source == null)
        {
            Debug.LogError("script not in audio source");
        }
    }

    public IEnumerator SwitchMusic(int music, float fadeOut = 0, float fadeIn = 0)
    {
        AudioClip clip = null;

        switch(music)
        {
            case 0: clip = introMusic; break;
            case 1: clip = gameplayMusic; break;
            case 2: clip = null; break;
        }

        if (source.clip == clip && source.isPlaying) yield break;

        float startVolume = source.volume;

        if (fadeOut > 0 && source.isPlaying)
        {
            while (source.volume > 0)
            {
                source.volume -= startVolume * Time.deltaTime / fadeOut;
                yield return null;
            }
        }

        source.Stop();
        source.volume = 0;

        source.clip = clip;
        if (clip == null)
        {
            source.volume = startVolume;
            yield break;
        }
        source.Play();

        if (fadeIn > 0 && startVolume > 0)
        {
            while (source.volume < startVolume)
            {
                source.volume += startVolume * Time.deltaTime / fadeIn;
                yield return null;
            }
            Debug.Log("e");
        }

        source.volume = startVolume;
    }
}
