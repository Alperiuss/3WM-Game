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

        // 1. Adım: Radyo kamerası önde
        if (radioCamera != null) radioCamera.Priority = 20;
        if (playerCamera != null) playerCamera.Priority = 10;

        // SADECE Radio Canvas açık olsun, diğerleri kapalı
        if (radioCanvas != null) radioCanvas.SetActive(true);
        if (playerCanvas != null) playerCanvas.SetActive(false);
        if (summaryPanel != null) summaryPanel.SetActive(false);
        if (marketPanel != null) marketPanel.SetActive(false);

        Time.timeScale = 1f;

        int currentDay = 1;
        if (InteractableRandomChance.Instance != null)
        {
            currentDay = InteractableRandomChance.Instance.CurrentDay;
        }

        if (WaveRadioSystem.Instance != null)
        {
            WaveRadioSystem.Instance.OnWaveCompleted(currentDay);
        }

        // Radyo sahnesinin süresi (Örn: Oyuncu radyoyu inceleyene kadar 3 saniye bekle veya tuşa basmasını sağla)
        isRadioFinished = false;
        yield return new WaitUntil(() => isRadioFinished);

        SetCursorState(false);

        // 2. Player kamerasını öne al
        if (radioCamera != null) radioCamera.Priority = 10;
        if (playerCamera != null) playerCamera.Priority = 20;

        // Radyo Canvas'ı kapat, Player Canvas'ı aç
        if (radioCanvas != null) radioCanvas.SetActive(false);
        if (playerCanvas != null) playerCanvas.SetActive(true);

        // Kamera geçişi için gerçek zamanlı kısa bir pay bırakıp oyunu başlat
        yield return new WaitForSecondsRealtime(1.0f);
        Time.timeScale = 1f;

        // İlk günü başlat
        if (InteractableRandomChance.Instance != null)
        {
            InteractableRandomChance.Instance.StartNewDay();
        }

        // Oyuncu markette alışverişini bitirip "Güne Başla" butonuna basana kadar 
        // bu Coroutine burada durur. Butona basıldığında aşağıda yazdığımız 
        // CloseMarketAndStartDay() fonksiyonunu çağıracağız.
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

        if (playerCanvas != null) playerCanvas.SetActive(false); // Oyun durunca player UI gizlensin

        if (summaryPanel != null)
        {
            summaryPanel.SetActive(true); // Görev özeti ve XP panelini göster
        }

        SetCursorState(true); // Fareyi göster
    }

    // Özet panelindeki "Continue" butonuna bu bağlanacak
    public void OnSummaryContinuePressed()
    {
        if (summaryPanel != null)
        {
            summaryPanel.SetActive(false); // Özet panelini kapat
        }

        // Hemen ardından Market panelini aç
        if (marketPanel != null)
        {
            marketPanel.SetActive(true); // Marketi aç
        }

        SetCursorState(true); // Fareyi göster
    }

    // Market panelindeki "Güne Başla / Devam Et" butonuna bu fonksiyonu bağlayacaksın
    public void OnMarketContinuePressed()
    {
        if (marketPanel != null)
        {
            marketPanel.SetActive(false); // Marketi kapat
        }

        if (radioCanvas != null) radioCanvas.SetActive(true);

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
