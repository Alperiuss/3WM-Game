using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Oynanış sesleri (deprem, siren, telefon, çalar saat...) için sahneden bağımsız ses çalar.
/// Klipler Assets/_Project/Resources/Audio altından yüklenir; Inspector'da liste ayarlamak gerekmez.
/// AudioManager'ın SFX mixer grubuna bağlanır, böylece ayarlardaki ses seviyesine uyar.
/// </summary>
public class GameSfx : MonoBehaviour
{
    public const string Earthquake = "earthquake";
    public const string Siren = "siren";
    public const string PhoneBuzz = "phone_buzz";
    public const string AlarmClock = "alarm_clock";
    public const string TvStatic = "tv_static";

    private static GameSfx instance;
    private static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
    private readonly Dictionary<string, AudioSource> loops = new Dictionary<string, AudioSource>();
    private readonly Dictionary<AudioSource, Coroutine> fades = new Dictionary<AudioSource, Coroutine>();

    private static GameSfx Get()
    {
        if (instance == null)
        {
            GameObject go = new GameObject("GameSfx");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<GameSfx>();
        }
        return instance;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public static AudioClip Clip(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (!clips.TryGetValue(name, out AudioClip clip))
        {
            clip = Resources.Load<AudioClip>("Audio/" + name);
            if (clip == null) Debug.LogWarning($"[GameSfx] Resources/Audio/{name} bulunamadı.");
            clips[name] = clip;
        }
        return clip;
    }

    public static AudioMixerGroup SfxGroup => AudioManager.Instance != null ? AudioManager.Instance.SfxGroup : null;

    /// <summary>Kimliğe bağlı döngüsel 2D ses başlatır (zaten çalıyorsa sesi günceller).</summary>
    public static void PlayLoop(string id, string clipName, float volume, float fadeIn = 0f)
    {
        AudioClip clip = Clip(clipName);
        if (clip == null) return;

        GameSfx sfx = Get();
        if (!sfx.loops.TryGetValue(id, out AudioSource src) || src == null)
        {
            src = sfx.gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            src.ignoreListenerPause = true;
            sfx.loops[id] = src;
        }

        src.outputAudioMixerGroup = SfxGroup;
        src.loop = true;
        if (src.clip != clip || !src.isPlaying)
        {
            src.clip = clip;
            src.volume = fadeIn > 0f ? 0f : volume;
            src.Play();
        }
        sfx.FadeTo(src, volume, fadeIn, false);
    }

    public static void StopLoop(string id, float fadeOut = 0.5f)
    {
        if (instance == null || !instance.loops.TryGetValue(id, out AudioSource src) || src == null) return;
        instance.FadeTo(src, 0f, fadeOut, true);
    }

    public static void StopAllLoops(float fadeOut = 0.3f)
    {
        if (instance == null) return;
        foreach (string id in new List<string>(instance.loops.Keys)) StopLoop(id, fadeOut);
    }

    public static bool IsLooping(string id)
    {
        return instance != null && instance.loops.TryGetValue(id, out AudioSource src) && src != null && src.isPlaying && src.volume > 0f;
    }

    /// <summary>Bir dünya objesinin üstünde 3D döngüsel ses kaynağı hazırlar (telefon, saat...).</summary>
    public static AudioSource CreateWorldLoop(GameObject host, string clipName, float volume)
    {
        AudioClip clip = Clip(clipName);
        if (host == null || clip == null) return null;

        AudioSource src = host.AddComponent<AudioSource>();
        src.clip = clip;
        src.loop = true;
        src.playOnAwake = false;
        src.volume = volume;
        src.spatialBlend = 0.85f;          // yönü duyulur ama evin her yerinden işitilir
        src.rolloffMode = AudioRolloffMode.Linear;
        src.minDistance = 2f;
        src.maxDistance = 22f;
        src.dopplerLevel = 0f;
        src.outputAudioMixerGroup = SfxGroup;
        return src;
    }

    private void FadeTo(AudioSource src, float target, float duration, bool stopAtEnd)
    {
        if (fades.TryGetValue(src, out Coroutine running) && running != null) StopCoroutine(running);
        if (duration <= 0f)
        {
            src.volume = target;
            if (stopAtEnd) src.Stop();
            fades.Remove(src);
            return;
        }
        fades[src] = StartCoroutine(FadeRoutine(src, target, duration, stopAtEnd));
    }

    private IEnumerator FadeRoutine(AudioSource src, float target, float duration, bool stopAtEnd)
    {
        float start = src.volume, t = 0f;
        while (t < duration && src != null)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        if (src == null) yield break;
        src.volume = target;
        if (stopAtEnd) src.Stop();
        fades.Remove(src);
    }
}
