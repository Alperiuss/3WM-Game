using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    // =========================================================================
    // ASLA NULL DÖNMEYEN GÜVENLİ SINGLETON
    // =========================================================================
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            // Eğer instance herhangi bir sebeple null olursa sahnede ara ve bul
            if (instance == null)
            {
                instance = FindFirstObjectByType<AudioManager>();
            }
            return instance;
        }
        private set
        {
            instance = value;
        }
    }

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
        if (instance == null)
        {
            instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        InitializeSoundDictionary();
    }

    private void Start()
    {
        // Oyun açıldığında PlayerPrefs'e kaydedilmiş ses ayarlarını AudioMixer'a yükle
        LoadAndApplySavedVolumes();
    }

    private void OnEnable()
    {
        OnVolumeChanged += HandleVolumeChange;
    }

    private void OnDisable()
    {
        OnVolumeChanged -= HandleVolumeChange;
    }

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
            sounds[i].source.playOnAwake = false;
            sounds[i].source.spatialBlend = 0f; // 2D Ses (Her yerden eşit ve net duyulur)
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

    private void LoadAndApplySavedVolumes()
    {
        ApplyVolumeFromPrefs(MASTER_KEY);
        ApplyVolumeFromPrefs(MUSIC_KEY);
        ApplyVolumeFromPrefs(SFX_KEY);
    }

    private void ApplyVolumeFromPrefs(string key)
    {
        float savedValue = PlayerPrefs.GetFloat(key, 1f); // Varsayılan: 1.0 (Full ses)
        HandleVolumeChange(key, savedValue);
    }

    #endregion

    #region Play Audio Methods

    public void PlaySFX(string name)
    {
        if (sounds == null || string.IsNullOrEmpty(name)) return;

        // Büyük / küçük harf veya boşluk farkı gözetmeksizin ismi bulur
        Sound s = Array.Find(sounds, sound => sound != null && string.Equals(sound.name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));

        if (s == null)
        {
            Debug.LogWarning($"[AudioManager]: '{name}' adında ses efekti listede bulunamadı! Inspector'daki ismi kontrol edin.");
            return;
        }

        if (s.source != null)
        {
            s.source.Play();
        }
    }
        // AudioManager.cs içine eklenecek:

    public void StopSFX(string name)
    {
        if (sounds == null || string.IsNullOrEmpty(name)) return;
        Sound s = Array.Find(sounds, sound => sound != null && string.Equals(sound.name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));
        if (s != null && s.source != null && s.source.isPlaying)
        {
            s.source.Stop();
        }
    }

    public bool HasSFX(string name)
    {
        if (sounds == null || string.IsNullOrEmpty(name)) return false;
        return Array.Exists(sounds, sound => sound != null && sound.name != null && string.Equals(sound.name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public bool IsPlayingSFX(string name)
    {
        if (sounds == null || string.IsNullOrEmpty(name)) return false;
        Sound s = Array.Find(sounds, sound => sound != null && string.Equals(sound.name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));
        return s != null && s.source != null && s.source.isPlaying;
    }

    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.spatialBlend = 0f;
        }

        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip == null) return;

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.spatialBlend = 0f;
        }

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