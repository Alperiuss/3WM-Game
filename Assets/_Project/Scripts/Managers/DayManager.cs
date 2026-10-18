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

    [Header("Player Spawn & Movement")]
    [SerializeField] private Transform playerSpawnPoint;     // Karakterin her gün başlayacağı nokta
    [SerializeField] private MonoBehaviour playerMovementScript; // Karakterin hareket scripti (örn: FirstPersonController)

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

        // Radyo aşamasında hareket kesinlikle kapalı olmalı!
        SetPlayerMovement(false);
        SetCursorState(false);
    }

    private IEnumerator GameSequenceRoutine(bool isStartingGame = false)
    {
        SetPlayerMovement(false);

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

                // KARAKTERİ HER GÜN AYNI YERE IŞINLA (Spawn Point)
                ResetPlayerPosition();
            },
            onComplete: () =>
            {
                Time.timeScale = 1f;

                // Ekran açıldıktan sonra Player Canvas'ı yumuşak bir fade-in ile açıyoruz
                StartCoroutine(FadeInPlayerCanvasRoutine());

                // Kilitleri aç ve karakterin hareket etmesine izin ver!
                SetPlayerMovement(true);

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

    // Karakterin pozisyonunu ve rotasyonunu Spawn Point'e sabitleyen güvenli metot
    private void ResetPlayerPosition()
    {
        if (playerSpawnPoint == null) return;

        // Sahnedeki player objesini buluyoruz (Etiketi "Player" olmalı veya tag ile bulabiliriz)
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            // Eğer karakterde CharacterController varsa doğrudan transform değiştirmek hata verebilir, önce kapatalım
            CharacterController cc = playerObj.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            playerObj.transform.position = playerSpawnPoint.position;
            playerObj.transform.rotation = playerSpawnPoint.rotation;

            if (cc != null) cc.enabled = true;
        }
    }

    // Karakterin hareketini açıp kapatan yardımcı metot
    private void SetPlayerMovement(bool canMove)
    {
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = canMove;
        }
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
        SetPlayerMovement(false); // Özet ekranında da hareket kilitlensin

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

                // İŞTE EKSİK OLAN VE HER ŞEYİ DÜZELTEN SİHİRLİ SATIR:
                if (SummaryPanelManager.Instance != null)
                {
                    // Eğer gün sayısını InteractableRandomChance'den alıyorsan turu da eşitler:
                    if (InteractableRandomChance.Instance != null)
                    {
                        SummaryPanelManager.Instance.currentRound = InteractableRandomChance.Instance.CurrentDay;
                    }

                    SummaryPanelManager.Instance.ShowSummary();
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

                // Summary panellerini de sıfırla/kapat:
                if (SummaryPanelManager.Instance != null)
                {
                    SummaryPanelManager.Instance.ClosePanels();
                }

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