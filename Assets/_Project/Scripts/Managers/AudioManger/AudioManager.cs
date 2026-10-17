using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer Settings")]
    [SerializeField] private AudioMixer mainAudioMixer;

    public const string MASTER_KEY = "MasterVolume";
    public const string MUSIC_KEY = "MusicVolume";
    public const string SFX_KEY = "SFXVolume";

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips List")]
    [SerializeField] private Sound[] sounds;

    // Statik Dinleyici Olayı (Event listener)
    public static Action<string, float> OnVolumeChanged;

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

    private void OnEnable()
    {
        // Ses değişim olayına kendisini dinleyici olarak ekler
        OnVolumeChanged += HandleVolumeChange;
    }

    private void OnDisable()
    {
        // Temizlik: Dinleyici bağlantısını koparır
        OnVolumeChanged -= HandleVolumeChange;
    }

    // 1. DÜZELTME: for döngüsü ve null kontrolü eklendi
    private void InitializeSoundDictionary()
    {
        if (sounds == null) return;

        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i] == null) continue;

            if (sounds[i].source == null)
            {
                sounds[i].source = gameObject.AddComponent<AudioSource>();
            }

            sounds[i].source.clip = sounds[i].clip;
            sounds[i].source.volume = sounds[i].volume;
            sounds[i].source.pitch = sounds[i].pitch;
            sounds[i].source.loop = sounds[i].loop;
        }
    }

    #region Listener Handler (Event Tarafından Tetiklenir)

    private void HandleVolumeChange(string parameterName, float sliderValue)
    {
        if (mainAudioMixer == null) return;

        // Desibel dönüşümü
        float dB = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;
        mainAudioMixer.SetFloat(parameterName, dB);

        // Kaydet
        PlayerPrefs.SetFloat(parameterName, sliderValue);
        PlayerPrefs.Save();
    }

    public float GetGroupVolume(string parameterName, float defaultValue = 1f)
    {
        return PlayerPrefs.GetFloat(parameterName, defaultValue);
    }

    #endregion

    #region Play Audio Methods

    public void PlaySFX(string name)
    {
        if (sounds == null) return;

        Sound s = Array.Find(sounds, sound => sound != null && sound.name == name);
        if (s == null)
        {
            Debug.LogWarning($"[AudioManager]: '{name}' adında ses efekti bulunamadı!");
            return;
        }

        if (s.source != null)
        {
            s.source.Play();
        }
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

// 2. DÜZELTME: Eksik olan Sound sınıfı dosyanın altına eklendi
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