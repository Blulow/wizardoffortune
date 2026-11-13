using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum AudioChannel { Music, Sfx}

public class AudioManager : MonoBehaviour
{
    static AudioManager instance;

    [SerializeField] AudioMixer mixer;
    [SerializeField] AudioMixerGroup music, sfx;

    AudioSource[] sources;
    [SerializeField] Vector2 pitchRange = new(0.92f, 1.08f);
    [SerializeField] int pitchedPoolSize = 8;

    AudioSource[] pitchedPool;
    int pitchedNext;
    [SerializeField] float minRepeatInterval = 0.05f; // seconds
    readonly Dictionary<AudioClip, float> lastPlayed = new();

    bool CanPlay(AudioClip clip)
    {
        float now = Time.unscaledTime;
        if (lastPlayed.TryGetValue(clip, out float t) && now - t < minRepeatInterval)
            return false;
        lastPlayed[clip] = now;
        return true;
    }


    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);

        var groups = new[] { music, sfx};
        sources = new AudioSource[groups.Length];
        for (int i = 0; i < groups.Length; i++)
        {
            sources[i] = gameObject.AddComponent<AudioSource>();
            sources[i].playOnAwake = false;
            sources[i].outputAudioMixerGroup = groups[i]; // set once
        }

        pitchedPool = new AudioSource[pitchedPoolSize];
        for (int i = 0; i < pitchedPoolSize; i++)
        {
            pitchedPool[i] = gameObject.AddComponent<AudioSource>();
            pitchedPool[i].playOnAwake = false;
        }
    }

    void Start()
    {
        foreach (AudioChannel c in System.Enum.GetValues(typeof(AudioChannel)))
            SetVolume(c, PlayerPrefs.GetFloat("vol_" + c, 1f));

        SetMasterVolume(PlayerPrefs.GetFloat("vol_Master", 1f));
    }

    public static void SetVolume(AudioChannel channel, float linear01)
        => SetParam(channel.ToString(), linear01);

    public static void SetMasterVolume(float linear01)
        => SetParam("Master", linear01);

    static void SetParam(string name, float linear01)
    {
        if (instance == null) return;

        float db = Mathf.Log10(Mathf.Clamp(linear01, 0.0001f, 1f)) * 20f;
        if (!instance.mixer.SetFloat(name + "Vol", db))
            Debug.LogWarning($"Mixer param '{name}Vol' is not exposed.");

        PlayerPrefs.SetFloat("vol_" + name, linear01);
    }

    public static void Play(AudioClip clip, AudioChannel channel, bool randomPitch = false,
                         float delay = 0f, float volume = 1f)
    {
        if (clip == null || instance == null) return;

        if (delay > 0f)
        {
            instance.StartCoroutine(instance.PlayAfter(clip, channel, randomPitch, delay, volume));
            return;
        }

        if (!instance.CanPlay(clip)) return;

        if (!randomPitch)
            instance.sources[(int)channel].PlayOneShot(clip, volume);
        else
            instance.PlayPitched(clip, channel, volume);
    }

    IEnumerator PlayAfter(AudioClip clip, AudioChannel channel,  bool randomPitch, float delay, float volume)
    {
        yield return new WaitForSeconds(delay);
        Play(clip, channel,  randomPitch, 0f, volume); // re-enters with delay = 0
    }
    void PlayPitched(AudioClip clip, AudioChannel channel, float volume)
    {
        // Prefer a free source, otherwise steal round-robin
        AudioSource src = null;
        foreach (var s in pitchedPool)
            if (!s.isPlaying) { src = s; break; }
        if (src == null)
        {
            src = pitchedPool[pitchedNext];
            pitchedNext = (pitchedNext + 1) % pitchedPool.Length;
        }

        src.clip = clip;
        src.volume = volume;
        src.pitch = Random.Range(pitchRange.x, pitchRange.y);
        src.outputAudioMixerGroup = sources[(int)channel].outputAudioMixerGroup;
        src.Play();
    }

}