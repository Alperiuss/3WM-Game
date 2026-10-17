using UnityEngine;
using System.Collections;
using Unity.Cinemachine; // Unity 6 Cinemachine 3 için

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    [Header("Cameras")]
    [SerializeField] private CinemachineCamera radioCamera;   // Radyo kamerası
    [SerializeField] private CinemachineCamera playerCamera;  // Oyuncu kamerası

    [Header("UI Canvases")]
    [SerializeField] private GameObject radioCanvas;          // Radyo Canvas'ı
    [SerializeField] private GameObject playerCanvas;         // Oyuncu Canvas'ı
    [SerializeField] private GameObject summaryPanel;         // Görev Özeti ve XP Paneli
    [SerializeField] private GameObject marketPanel;          // Market Paneli
    // Radyonun bitip bitmediğini takip eden bayrak (flag)
    private bool isRadioFinished = false;

    private void Awake()
    {
        // Singleton pattern
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
        // Oyunu bu özel sıralı başlatma fonksiyonu ile başlatıyoruz
        StartCoroutine(GameSequenceRoutine());
    }

    private IEnumerator GameSequenceRoutine()
    {
        SetCursorState(false);

        // Fade geçişini başlatıyoruz
        ScreenFader.Instance.FadeTransition(
            onHalfWay: () =>
            {
                // 1. AŞAMA: Ekran Siyahken (Kameralar ve Canvas'lar arkada ayarlanır)
                if (radioCamera != null) radioCamera.Priority = 20;
                if (playerCamera != null) playerCamera.Priority = 10;

                if (radioCanvas != null) radioCanvas.SetActive(true);
                if (playerCanvas != null) playerCanvas.SetActive(false);
                if (summaryPanel != null) summaryPanel.SetActive(false);
                if (marketPanel != null) marketPanel.SetActive(false);

                Time.timeScale = 1f;
            },
            onComplete: () =>
            {
                // 2. AŞAMA: Ekran Tamamen Açıldıktan SONRA Radyo Güvenle Başlar!
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

        // Radyo sahnesinin bitmesini bekle (Enter ile geçilene kadar veya süre bitene kadar kilitli kalır)
        isRadioFinished = false;
        yield return new WaitUntil(() => isRadioFinished);

        // --- RADYODAN OYUNCU KAMERASINA GEÇİŞ ---
        ScreenFader.Instance.FadeTransition(() =>
        {
            SetCursorState(false);

            if (radioCamera != null) radioCamera.Priority = 10;
            if (playerCamera != null) playerCamera.Priority = 20;

            if (radioCanvas != null) radioCanvas.SetActive(false);
            if (playerCanvas != null) playerCanvas.SetActive(true);
        });

        yield return new WaitForSecondsRealtime(1.0f);
        Time.timeScale = 1f;

        if (InteractableRandomChance.Instance != null)
        {
            InteractableRandomChance.Instance.StartNewDay();
        }
    }

    // Radyo bittiğinde çağrılır
    public void OnRadioFinished()
    {
        isRadioFinished = true;
    }

    // ==========================================
    // DEPREM BİTTİĞİNDE ÇAĞRILACAK AKIŞ (Özet -> Market -> Yeni Gün)
    // ==========================================

    public void ShowDaySummary()
    {
        Time.timeScale = 0f; // Oyunu durdur

        ScreenFader.Instance.FadeTransition(() =>
         {
             if (playerCanvas != null) playerCanvas.SetActive(false); // Oyun durunca player UI gizlensin

             if (summaryPanel != null)
             {
                 summaryPanel.SetActive(true); // Görev özeti ve XP panelini göster
             }

             SetCursorState(true); // Fareyi göster
         });
    }

    // Özet panelindeki "Continue" butonuna bu bağlanacak
    public void OnSummaryContinuePressed()
    {
        ScreenFader.Instance.FadeTransition(() =>
        {
            if (summaryPanel != null) summaryPanel.SetActive(false); // Özet panelini kapat
            if (marketPanel != null) marketPanel.SetActive(true);    // Market panelini aç

            SetCursorState(true); // Fareyi göster
        });
    }

    // Market panelindeki "Güne Başla / Devam Et" butonuna bu fonksiyonu bağlayacaksın
    public void OnMarketContinuePressed()
    {
        ScreenFader.Instance.FadeTransition(() =>
         {
             if (marketPanel != null) marketPanel.SetActive(false); // Market panelini kapat
             if (radioCanvas != null) radioCanvas.SetActive(true); // Oyuncu UI'ını aç
         });

        Time.timeScale = 1f; // Oyunu tekrar başlat

        StartCoroutine(GameSequenceRoutine()); // Yeni günü başlat
    }

    // Yardımcı Metot: İmlecin görünürlüğünü ve kilitlenme durumunu yönetir
    private void SetCursorState(bool isVisible)
    {
        if (isVisible)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None; // İmleç serbestçe hareket eder
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked; // İmleç ekranın ortasında kilitlenir/gizlenir
        }
    }
}
