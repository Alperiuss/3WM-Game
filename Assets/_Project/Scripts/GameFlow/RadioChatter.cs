using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Radyo yayını sırasında "Papers, Please" tarzı anlaşılmayan, cızırtılı konuşma sesi.
/// WaveRadioSystem harfler yazılırken SetTalking(true), cümle bitince SetTalking(false) çağırır.
/// Yayın boyunca altta hafif bir parazit (static) döner; başta ve sonda telsiz "kşşt" sesi çalar.
/// </summary>
public class RadioChatter : MonoBehaviour
{
    public static RadioChatter Instance { get; private set; }

    [Header("Clips")]
    [SerializeField] private AudioClip chatterClip;     // RadioChatter.wav  (garbled voice, loop)
    [SerializeField] private AudioClip staticLoopClip;  // RadioStaticLoop.wav (hiss, loop)
    [SerializeField] private AudioClip squelchClip;     // RadioSquelch.wav (short burst)

    [Header("Volumes")]
    [Range(0f, 1f)] [SerializeField] private float chatterVolume = 0.75f;
    [Range(0f, 1f)] [SerializeField] private float staticVolume = 0.16f;
    [Range(0f, 1f)] [SerializeField] private float squelchVolume = 0.5f;
    [SerializeField] private Vector2 chatterPitchRange = new Vector2(0.94f, 1.08f);
    [SerializeField] private float fadeSpeed = 10f;

    [Header("Mixer (optional)")]
    [SerializeField] private AudioMixerGroup outputGroup;

    private AudioSource chatterSource;
    private AudioSource staticSource;
    private AudioSource oneShotSource;
    private bool talking;
    private bool broadcasting;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        chatterSource = CreateSource(chatterClip, true);
        staticSource = CreateSource(staticLoopClip, true);
        oneShotSource = CreateSource(null, false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private AudioSource CreateSource(AudioClip clip, bool loop)
    {
        AudioSource s = gameObject.AddComponent<AudioSource>();
        s.clip = clip;
        s.loop = loop;
        s.playOnAwake = false;
        s.spatialBlend = 0f;
        s.volume = 0f;
        s.ignoreListenerPause = true;
        if (outputGroup != null) s.outputAudioMixerGroup = outputGroup;
        return s;
    }

    public void BeginBroadcast()
    {
        broadcasting = true;
        talking = false;
        if (staticSource != null && staticSource.clip != null)
        {
            staticSource.volume = staticVolume;
            if (!staticSource.isPlaying)
            {
                staticSource.time = Random.Range(0f, Mathf.Max(0f, staticSource.clip.length - 0.1f));
                staticSource.Play();
            }
        }
        PlaySquelch();
    }

    public void SetTalking(bool isTalking)
    {
        if (!broadcasting) BeginBroadcast();
        if (isTalking == talking) return;
        talking = isTalking;

        if (chatterSource == null || chatterSource.clip == null) return;
        if (talking)
        {
            // Her cümlede farklı bir yerden ve hafif farklı tonla başlasın
            chatterSource.time = Random.Range(0f, Mathf.Max(0f, chatterSource.clip.length - 1f));
            chatterSource.pitch = Random.Range(chatterPitchRange.x, chatterPitchRange.y);
            chatterSource.volume = 0f;
            chatterSource.Play();
        }
    }

    public void EndBroadcast()
    {
        talking = false;
        broadcasting = false;
        PlaySquelch();
    }

    private void PlaySquelch()
    {
        if (oneShotSource != null && squelchClip != null)
            oneShotSource.PlayOneShot(squelchClip, squelchVolume);
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;

        if (chatterSource != null && chatterSource.clip != null)
        {
            float target = talking ? chatterVolume : 0f;
            chatterSource.volume = Mathf.MoveTowards(chatterSource.volume, target, fadeSpeed * dt);
            if (!talking && chatterSource.isPlaying && chatterSource.volume <= 0.001f) chatterSource.Stop();
        }

        if (staticSource != null && staticSource.clip != null)
        {
            float target = broadcasting ? staticVolume : 0f;
            staticSource.volume = Mathf.MoveTowards(staticSource.volume, target, fadeSpeed * 0.3f * dt);
            if (!broadcasting && staticSource.isPlaying && staticSource.volume <= 0.001f) staticSource.Stop();
        }
    }
}
