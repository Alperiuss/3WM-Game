using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer Settings")]
    [SerializeField] private AudioMixer mainAudioMixer;
    
    // AudioMixer'daki Exposed Parameter İsimleri
    public const string MASTER_KEY = "MasterVolume";
    public const string MUSIC_KEY = "MusicVolume";
    public const string SFX_KEY = "SFXVolume";

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips List")]
    [SerializeField] private Sound[] sounds;

    private void Awake()
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

        InitializeSoundDictionary();
    }

    private void InitializeSoundDictionary()
    {
        foreach (Sound s in sounds)
        {
            if (s.source == null)
            {
                s.source = gameObject.AddComponent<AudioSource>();
            }
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    #region Volume Control Methods

    // Slider'dan gelen 0.0001 - 1 değerini desibel birimine çevirip AudioMixer'a aktarır
    public void SetGroupVolume(string parameterName, float sliderValue)
    {
        if (mainAudioMixer == null)
        {
            Debug.LogWarning("[AudioManager]: AudioMixer atanmadı!");
            return;
        }

        float dB = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;
        mainAudioMixer.SetFloat(parameterName, dB);

        PlayerPrefs.SetFloat(parameterName, sliderValue);
        PlayerPrefs.Save();
    }

    public float GetGroupVolume(string parameterName, float defaultValue = 1f)
    {
        return PlayerPrefs.GetFloat(parameterName, defaultValue);
    }

    #endregion

    #region Play Audio Methods

    // İsim ile ses efekti çalma: AudioManager.Instance.PlaySFX("ButtonClick");
    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning($"[AudioManager]: '{name}' adında ses efekti bulunamadı!");
            return;
        }
        s.source.Play();
    }

    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicSource == null || musicClip == null) return;

        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    #endregion
}

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    
    [Range(0f, 1f)]
    public float volume = 1f;
    
    [Range(0.1f, 3f)]
    public float pitch = 1f;
    
    public bool loop = false;

    [HideInInspector]
    public AudioSource source;
}