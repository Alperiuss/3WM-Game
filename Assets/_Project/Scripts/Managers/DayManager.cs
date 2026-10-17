using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    [Header("Cameras")]
    [SerializeField] private CinemachineCamera radioCamera;
    [SerializeField] private CinemachineCamera playerCamera;

    [Header("UI Canvases")]
    [SerializeField] private GameObject radioCanvas;
    [SerializeField] private GameObject playerCanvas;
    [SerializeField] private GameObject summaryPanel;
    [SerializeField] private GameObject marketPanel;

    private bool isRadioFinished = false;
    private bool isFirstGameLaunch = true;

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
        }
    }

    private void Start()
    {
        SetupRadioCameraInitially();
        StartCoroutine(GameSequenceRoutine(true));
    }

    private void SetupRadioCameraInitially()
    {
        if (radioCamera != null) radioCamera.Priority = 20;
        if (playerCamera != null) playerCamera.Priority = 10;

        if (radioCanvas != null) radioCanvas.SetActive(true);
        if (playerCanvas != null)
        {
            playerCanvas.SetActive(true);
            CanvasGroup group = playerCanvas.GetComponent<CanvasGroup>();
            if (group != null) group.alpha = 0f; // Başlangıçta gizli
        }
        if (summaryPanel != null) summaryPanel.SetActive(false);
        if (marketPanel != null) marketPanel.SetActive(false);

        SetCursorState(false);
    }

    private IEnumerator GameSequenceRoutine(bool isStartingGame = false)
    {
        ScreenFader.Instance.FadeTransition(
            onHalfWay: () =>
            {
                Time.timeScale = 1f;
            },
            onComplete: () =>
            {
                int currentDayToPlay = 1;
                if (InteractableRandomChance.Instance != null)
                {
                    currentDayToPlay = Mathf.Max(1, InteractableRandomChance.Instance.CurrentDay);
                }

                if (WaveRadioSystem.Instance != null)
                {
                    WaveRadioSystem.Instance.OnWaveCompleted(currentDayToPlay);
                }
            }
        );

        isRadioFinished = false;
        yield return new WaitUntil(() => isRadioFinished);

        // --- RADYODAN OYUNCU KAMERASINA GEÇİŞ (Kamera değişimi kesinlikle siyah ekranda yapılır) ---
        ScreenFader.Instance.FadeTransition(
            onHalfWay: () =>
            {
                SetCursorState(false);

                // Ekran tamamen siyahken kamera ve aktif canvas ayarlanır (Zıplama önlenir)
                if (radioCamera != null) radioCamera.Priority = 10;
                if (playerCamera != null) playerCamera.Priority = 20;

                if (radioCanvas != null) radioCanvas.SetActive(false);

                if (playerCanvas != null)
                {
                    playerCanvas.SetActive(true);
                    CanvasGroup group = playerCanvas.GetComponent<CanvasGroup>();
                    if (group != null) group.alpha = 0f; // Ekran açılırken görünmez başlar
                }
            },
            onComplete: () =>
            {
                Time.timeScale = 1f;

                // Ekran açıldıktan sonra Player Canvas'ı yumuşak bir fade-in ile açıyoruz
                StartCoroutine(FadeInPlayerCanvasRoutine());

                if (InteractableRandomChance.Instance != null)
                {
                    if (isStartingGame && isFirstGameLaunch)
                    {
                        InteractableRandomChance.Instance.StartNewDay();
                        isFirstGameLaunch = false;
                    }
                    else if (!isStartingGame)
                    {
                        InteractableRandomChance.Instance.StartNewDay();
                    }
                }
            }
        );
    }

    // Player Canvas'ın gecikmeli ve yumuşak bir şekilde netleşmesini sağlayan rutin
    private IEnumerator FadeInPlayerCanvasRoutine()
    {
        if (playerCanvas == null) yield break;

        CanvasGroup group = playerCanvas.GetComponent<CanvasGroup>();
        if (group == null) yield break;

        // Ekran açılışının oturması için çok kısa bir gecikme (örneğin 0.3 saniye)
        yield return new WaitForSeconds(0.3f);

        float duration = 0.6f; // Netleşme süresi
        float elapsed = 0f;
        group.alpha = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }

        group.alpha = 1f;
    }

    public void OnRadioFinished()
    {
        isRadioFinished = true;
    }

    public void ShowDaySummary()
    {
        Time.timeScale = 0f;

        ScreenFader.Instance.FadeTransition(
            onHalfWay: () =>
            {
                if (playerCanvas != null)
                {
                    CanvasGroup group = playerCanvas.GetComponent<CanvasGroup>();
                    if (group != null) group.alpha = 0f;
                    playerCanvas.SetActive(false);
                }

                if (summaryPanel != null)
                {
                    summaryPanel.SetActive(true);
                }

                SetCursorState(true);
            }
        );
    }

    public void OnSummaryContinuePressed()
    {
        ScreenFader.Instance.FadeTransition(
            onHalfWay: () =>
            {
                if (summaryPanel != null) summaryPanel.SetActive(false);
                if (marketPanel != null) marketPanel.SetActive(true);

                SetCursorState(true);
            }
        );
    }

    public void OnMarketContinuePressed()
    {
        SetupRadioCameraInitially();

        if (marketPanel != null) marketPanel.SetActive(false);

        if (InteractableRandomChance.Instance != null)
        {
            InteractableRandomChance.Instance.AdvanceToNextDay();
        }

        Time.timeScale = 1f;
        StartCoroutine(GameSequenceRoutine(false));
    }

    private void SetCursorState(bool isVisible)
    {
        if (isVisible)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}