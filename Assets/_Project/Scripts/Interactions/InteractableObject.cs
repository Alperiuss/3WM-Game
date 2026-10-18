using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A household object the player must secure/fix (TV, gas valve, plates...).
///
/// Visual states:
///  - idle      : the object is not part of today's tasks      -> idleVisual (optional)
///  - active    : today's task, still unsafe                    -> activeVisual + pulsing warm tint (+ shake for noise makers)
///  - resolved  : the player fixed it                           -> resolvedVisual + short green flash
///
/// Legacy cube items can still use targetRenderer + the three colors; leave targetRenderer empty for real models.
/// </summary>
public class InteractableObject : MonoBehaviour
{
    [SerializeField] private InteractableData data;

    [Header("Interaction Settings")]
    [SerializeField] private float requiredHoldTime = 2f; // Etkileşim için gereken süre (saniye)

    [Header("Conditions")]
    [SerializeField] private bool isInteractable = false;

    [Header("Visual & Color References")]
    [Tooltip("Sadece eski küp objeler için. Gerçek eşyalarda BOŞ bırakın.")]
    [SerializeField] private Renderer targetRenderer;
    [Tooltip("Görev aktifken görünen parça (açık vana, akan su, çalan telefon ekranı...)")]
    [SerializeField] private GameObject activeVisual;
    [Tooltip("Görev çözülünce görünen parça (kayışlar, kapalı vana, rafa dizilmiş tabaklar...)")]
    [SerializeField] private GameObject resolvedVisual;
    [Tooltip("Obje bugünün görevi değilken görünen parça (opsiyonel)")]
    [SerializeField] private GameObject idleVisual;

    [Header("Color Settings (legacy cube items)")]
    [SerializeField] private Color normalColor = Color.red;     // Aktifken (Kırmızı)
    [SerializeField] private Color disabledColor = Color.gray;  // Pasifken (Gri)
    [SerializeField] private Color interactedColor = Color.green; // Çözüldüğünde (Yeşil)

    [Header("Hazard Highlight (real objects)")]
    [Tooltip("Titreyecek / renklenecek model kökü. Boşsa bu obje kullanılır.")]
    [SerializeField] private Transform modelRoot;
    [SerializeField] private Color highlightColor = new Color(1f, 0.38f, 0.12f, 1f);
    [Range(0f, 1f)]
    [SerializeField] private float highlightStrength = 0.5f;
    [SerializeField] private float pulseSpeed = 4f;
    [SerializeField] private bool shakeWhileNoisy = true;
    [SerializeField] private float shakeAmount = 0.006f;
    [SerializeField] private Color resolvedFlashColor = new Color(0.35f, 1f, 0.45f, 1f);

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private bool isResolved = false;
    private readonly List<Renderer> tintRenderers = new List<Renderer>();
    private readonly List<Color> tintBaseColors = new List<Color>();
    private MaterialPropertyBlock propertyBlock;
    private Vector3 modelRestPosition;
    private bool tintApplied = false;
    private Coroutine flashRoutine;
    private AudioSource noiseSource;

    public bool IsInteractable => isInteractable && !isResolved;
    public float RequiredHoldTime => requiredHoldTime;
    public InteractableData Data => data; // Veriye dışarıdan güvenli erişim

    private void Awake()
    {
        if (modelRoot == null) modelRoot = transform;
        modelRestPosition = modelRoot.localPosition;
        propertyBlock = new MaterialPropertyBlock();
        CacheTintRenderers();
    }

    private void Start()
    {
        ApplyState();
    }

    private void CacheTintRenderers()
    {
        tintRenderers.Clear();
        tintBaseColors.Clear();
        Renderer[] renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            if (r == null || r == targetRenderer) continue;
            Material m = r.sharedMaterial;
            if (m == null) continue;

            Color baseColor;
            if (m.HasProperty(BaseColorId)) baseColor = m.GetColor(BaseColorId);
            else if (m.HasProperty(ColorId)) baseColor = m.GetColor(ColorId);
            else continue;

            tintRenderers.Add(r);
            tintBaseColors.Add(baseColor);
        }
    }

    public void SetInteractableState(bool state)
    {
        isInteractable = state;
        isResolved = false; // Yeni turda çözülme durumunu sıfırla
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }
        ApplyState();
    }

    public void Interact()
    {
        if (!IsInteractable) return;

        ResolveHazard();
    }

    private void ResolveHazard()
    {
        isResolved = true;

        if (data != null)
        {
            Debug.Log($"Interacted with {data.objectName}.");

            if (data.interactionSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayOneShot(data.interactionSound);
            }
        }
        else
        {
            Debug.LogWarning($"[InteractableObject] '{name}' objesine InteractableData atanmamış.", this);
        }

        // Yöneticiye hangi görevin bittiğini bildiriyoruz (Data yoksa da sayaç düşsün, gün kilitlenmesin)
        if (InteractableRandomChance.Instance != null)
        {
            InteractableRandomChance.Instance.NotifyObjectResolved(data != null ? data.taskDescription : null, data);
        }

        ApplyState();

        if (isActiveAndEnabled)
        {
            flashRoutine = StartCoroutine(ResolvedFlashRoutine());
        }
    }

    private void ApplyState()
    {
        bool active = isInteractable && !isResolved;

        // Önce hepsini kapat, sonra doğru olanı aç (aynı obje birden fazla alana atanmış olabilir)
        if (activeVisual != null) activeVisual.SetActive(false);
        if (resolvedVisual != null) resolvedVisual.SetActive(false);
        if (idleVisual != null) idleVisual.SetActive(false);

        if (isResolved)
        {
            if (resolvedVisual != null) resolvedVisual.SetActive(true);
        }
        else if (active)
        {
            if (activeVisual != null) activeVisual.SetActive(true);
        }
        else
        {
            if (idleVisual != null) idleVisual.SetActive(true);
        }

        // Eski küp sistemi
        if (targetRenderer != null)
        {
            Color c = isResolved ? interactedColor : (isInteractable ? normalColor : disabledColor);
            targetRenderer.material.color = c;
        }

        if (!active)
        {
            ClearTint();
            if (modelRoot != null) modelRoot.localPosition = modelRestPosition;
        }

        UpdateNoise(active);
    }

    // ------------------------------------------------------------------ Gürültü sesi (telefon, çalar saat, kumanda)

    /// <summary>Gürültü yapan eşyanın döngüsel sesi (veride isNoiseMaker işaretli olanlar).</summary>
    private static string GetNoiseClip(InteractableData d)
    {
        if (d == null || !d.isNoiseMaker || string.IsNullOrEmpty(d.objectName)) return null;
        string n = d.objectName.ToLowerInvariant();
        if (n.Contains("phone")) return GameSfx.PhoneBuzz;
        if (n.Contains("clock")) return GameSfx.AlarmClock;
        if (n.Contains("remote")) return GameSfx.TvStatic;
        return null;
    }

    private void UpdateNoise(bool active)
    {
        if (active)
        {
            if (noiseSource == null)
            {
                string clipName = GetNoiseClip(data);
                if (clipName == null) return;
                noiseSource = GameSfx.CreateWorldLoop(gameObject, clipName, 0.9f);
                if (noiseSource == null) return;
            }
            if (!noiseSource.isPlaying)
            {
                noiseSource.time = Random.Range(0f, noiseSource.clip.length * 0.9f);
                noiseSource.Play();
            }
        }
        else if (noiseSource != null && noiseSource.isPlaying)
        {
            noiseSource.Stop();
        }
    }

    /// <summary>Gün bittiğinde (deprem sonrası, ölüm ekranı) hâlâ çalan eşyaları susturur.</summary>
    public void SilenceNoise()
    {
        if (noiseSource != null) noiseSource.Stop();
    }

    private void Update()
    {
        if (!IsInteractable) return;

        // Uyarı rengi: objenin kendi rengi ile turuncu arasında nabız gibi atar
        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed);
        ApplyTint(highlightColor, highlightStrength * pulse);

        // Gürültü yapan eşyalar (telefon, saat, kumanda, musluk) titrer
        if (shakeWhileNoisy && data != null && data.isNoiseMaker && modelRoot != null && modelRoot != transform)
        {
            float t = Time.time * 45f;
            Vector3 offset = new Vector3(Mathf.Sin(t), 0f, Mathf.Cos(t * 1.3f)) * shakeAmount;
            modelRoot.localPosition = modelRestPosition + offset;
        }
    }

    private void ApplyTint(Color target, float amount)
    {
        if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();
        for (int i = 0; i < tintRenderers.Count; i++)
        {
            Renderer r = tintRenderers[i];
            if (r == null) continue;
            Color c = Color.Lerp(tintBaseColors[i], target, amount);
            c.a = tintBaseColors[i].a;
            r.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, c);
            propertyBlock.SetColor(ColorId, c);
            r.SetPropertyBlock(propertyBlock);
        }
        tintApplied = true;
    }

    private void ClearTint()
    {
        if (!tintApplied) return;
        for (int i = 0; i < tintRenderers.Count; i++)
        {
            if (tintRenderers[i] != null) tintRenderers[i].SetPropertyBlock(null);
        }
        tintApplied = false;
    }

    private IEnumerator ResolvedFlashRoutine()
    {
        // Renderer listesini yenile (resolvedVisual yeni açıldı)
        CacheTintRenderers();

        float duration = 0.6f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            ApplyTint(resolvedFlashColor, 0.6f * (1f - elapsed / duration));
            yield return null;
        }
        ClearTint();
        flashRoutine = null;
    }

    private void OnDisable()
    {
        ClearTint();
        SilenceNoise();
    }
}
