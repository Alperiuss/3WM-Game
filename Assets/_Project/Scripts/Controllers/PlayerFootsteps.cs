using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Oyuncu evde yürürken adım sesi çalar. Katedilen mesafeye göre çalıştığı için koşunca adımlar sıklaşır,
/// hareket kilitliyken (radyo, deprem, özet) ses çıkmaz.
/// Sahnede "Player" etiketli karaktere otomatik eklenir; Inspector'da ayar gerekmez.
/// Klipler: Resources/Audio/Footsteps
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerFootsteps : MonoBehaviour
{
    [SerializeField] private float strideLength = 1.35f;
    [SerializeField] private float volume = 0.55f;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.9f, 1.1f);

    private CharacterController controller;
    private AudioSource source;
    private AudioClip[] steps;
    private Vector3 lastPosition;
    private float travelled;
    private int lastIndex = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        AttachToPlayer();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => AttachToPlayer();

    private static void AttachToPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && player.GetComponent<CharacterController>() != null && player.GetComponent<PlayerFootsteps>() == null)
        {
            player.AddComponent<PlayerFootsteps>();
        }
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        steps = Resources.LoadAll<AudioClip>("Audio/Footsteps");
        if (steps.Length == 0) Debug.LogWarning("[PlayerFootsteps] Resources/Audio/Footsteps içinde ses yok.");

        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.outputAudioMixerGroup = GameSfx.SfxGroup;
        lastPosition = transform.position;
    }

    private void Update()
    {
        Vector3 delta = transform.position - lastPosition;
        lastPosition = transform.position;
        delta.y = 0f;
        float moved = delta.magnitude;

        // Işınlanma (gün başı spawn) veya zaman durmuşsa sayma
        if (Time.deltaTime <= 0f || moved > 2f || !controller.isGrounded || steps.Length == 0)
        {
            travelled = 0f;
            return;
        }

        travelled += moved;
        if (travelled >= strideLength)
        {
            travelled = 0f;
            PlayStep();
        }
    }

    private void PlayStep()
    {
        int index = Random.Range(0, steps.Length);
        if (steps.Length > 1 && index == lastIndex) index = (index + 1) % steps.Length;
        lastIndex = index;

        if (source.outputAudioMixerGroup == null) source.outputAudioMixerGroup = GameSfx.SfxGroup;
        source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        source.PlayOneShot(steps[index], volume);
    }
}
