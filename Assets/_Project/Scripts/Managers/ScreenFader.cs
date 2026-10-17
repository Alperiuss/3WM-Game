using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [Header("UI Reference")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float defaultFadeDuration = 0.6f;

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

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Ekranı karartır, ortada onHalfWay'i çalıştırır ve ekran açıldığında onComplete'i tetikler.
    /// </summary>
    public void FadeTransition(System.Action onHalfWay, System.Action onComplete = null, float duration = -1f)
    {
        float dur = duration > 0 ? duration : defaultFadeDuration;
        StartCoroutine(TransitionRoutine(onHalfWay, onComplete, dur));
    }

    private IEnumerator TransitionRoutine(System.Action onHalfWay, System.Action onComplete, float duration)
    {
        // 1. Adım: Fade Out (Ekran kararıyor)
        yield return FadeRoutine(0f, 1f, duration);

        // 2. Adım: Ekran tamamen siyahken arka plan işlemleri (Kamera değişimi vb.)
        onHalfWay?.Invoke();

        yield return new WaitForSecondsRealtime(0.1f);

        // 3. Adım: Fade In (Ekran açılıyor)
        yield return FadeRoutine(1f, 0f, duration);

        // 4. Adım: Ekran tamamen açıldıktan sonra tetiklenen olay (Radyonun başlaması vb.)
        onComplete?.Invoke();
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;

        if (canvasGroup != null)
        {
            canvasGroup.gameObject.SetActive(true);
            canvasGroup.alpha = startAlpha;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
                yield return null;
            }

            canvasGroup.alpha = endAlpha;

            if (endAlpha == 0f)
            {
                canvasGroup.gameObject.SetActive(false);
            }
        }
    }
}