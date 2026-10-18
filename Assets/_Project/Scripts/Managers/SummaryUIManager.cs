using UnityEngine;
using TMPro;

public class SummaryPanelManager : MonoBehaviour
{
    public static SummaryPanelManager Instance { get; private set; }

    [Header("Tur Ayarları (1 - 4)")]
    [Tooltip("Şu anki aktif tur (1, 2, 3 veya 4)")]
    [Range(1, 4)]
    public int currentRound = 1;

    [Tooltip("Her tur için kazanmak adına gereken MİNİMUM tamamlanmış görev sayısı")]
    [SerializeField] private int[] requiredTasksPerRound = new int[4] { 2, 3, 3, 4 };

    [Header("XP Ödül Ayarları")]
    [Tooltip("Tamamlanan her bir görev başına verilecek XP")]
    [SerializeField] private int xpPerCompletedTask = 50;

    [Tooltip("Tur başarıyla kazanıldığında verilecek ekstra zafer bonusu XP")]
    [SerializeField] private int victoryBonusXP = 100;

    [Tooltip("Kazanılan XP direkt CurrencyManager'daki paraya/XP'ye eklensin mi?")]
    [SerializeField] private bool autoAddToCurrencyManager = true;

    [Header("Paneller")]
    [SerializeField] private GameObject successfulPanel;
    [SerializeField] private GameObject unsuccessfulPanel;

    [Header("Successful Panel Metinleri")]
    [Tooltip("Örn: '3 mission succesful'")]
    [SerializeField] private TextMeshProUGUI successPanel_SuccessText;
    [Tooltip("Örn: '2 mission unsuccesful'")]
    [SerializeField] private TextMeshProUGUI successPanel_FailText;
    [Tooltip("Açık mavi kutudaki XP yazısı (Başarılı panel)")]
    [SerializeField] private TextMeshProUGUI successPanel_XpText;

    [Header("Unsuccessful Panel Metinleri")]
    [Tooltip("Örn: '3 Unsuccesful'")]
    [SerializeField] private TextMeshProUGUI unsuccessPanel_FailText;
    [Tooltip("Örn: '2 Succesful'")]
    [SerializeField] private TextMeshProUGUI unsuccessPanel_SuccessText;
    [Tooltip("Açık mavi kutudaki XP yazısı (Başarısız panel)")]
    [SerializeField] private TextMeshProUGUI unsuccessPanel_XpText;

    [Header("Zaman Ayarı")]
    [SerializeField] private bool pauseTimeOnSummary = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Başlangıçta paneller kapalı olsun
        if (successfulPanel != null) successfulPanel.SetActive(false);
        if (unsuccessfulPanel != null) unsuccessfulPanel.SetActive(false);
    }

    /// <summary>
    /// Tur / Afet bittiğinde bu metodu çağırın.
    /// </summary>
    [ContextMenu("Test Summary")]
    public void ShowSummary()
    {
        if (TaskUIManager.Instance == null)
        {
            Debug.LogError("[SummaryPanelManager] TaskUIManager bulunamadı!");
            return;
        }

        // Görev sayılarını al
        TaskUIManager.Instance.GetTaskCounts(out int completedCount, out int uncompletedCount);

        // Bu tur için gereken görev sayısını bul
        int roundIndex = Mathf.Clamp(currentRound - 1, 0, requiredTasksPerRound.Length - 1);
        int targetRequired = requiredTasksPerRound[roundIndex];

        // Başarı Kontrolü
        bool isSuccess = completedCount >= targetRequired;

        // -------------------------------------------------------------
        // XP HESAPLAMA MANTIĞI
        // -------------------------------------------------------------
        int earnedXP = completedCount * xpPerCompletedTask;

        if (isSuccess)
        {
            earnedXP += victoryBonusXP; // Başarılıysa zafer bonusu ekle
        }

        // Kazanılan XP'yi market parasına (CurrencyManager) aktar
        if (autoAddToCurrencyManager && CurrencyManager.Instance != null && earnedXP > 0)
        {
            CurrencyManager.Instance.AddFame(earnedXP);
            Debug.Log($"[SummaryPanelManager] {earnedXP} XP CurrencyManager'a eklendi!");
        }

        // -------------------------------------------------------------
        // UI PANELLERİNİ AÇMA VE METİNLERİ YAZDIRMA
        // -------------------------------------------------------------
        if (successfulPanel != null) successfulPanel.SetActive(false);
        if (unsuccessfulPanel != null) unsuccessfulPanel.SetActive(false);

        if (isSuccess)
        {
            // --- SUCCESFUL PANELİ ---
            if (successfulPanel != null)
            {
                successfulPanel.SetActive(true);
                successfulPanel.transform.SetAsLastSibling();
            }

            if (successPanel_SuccessText != null)
                successPanel_SuccessText.text = $"{completedCount} mission succesful";

            if (successPanel_FailText != null)
                successPanel_FailText.text = $"{uncompletedCount} mission unsuccesful";

            // Mavi barda yazacak XP metni
            if (successPanel_XpText != null)
                successPanel_XpText.text = $"+{earnedXP} XP EARNED";
        }
        else
        {
            // --- UNSUCCESFUL PANELİ ---
            if (unsuccessfulPanel != null)
            {
                unsuccessfulPanel.SetActive(true);
                unsuccessfulPanel.transform.SetAsLastSibling();
            }

            if (unsuccessPanel_FailText != null)
                unsuccessPanel_FailText.text = $"{uncompletedCount} Unsuccesful";

            if (unsuccessPanel_SuccessText != null)
                unsuccessPanel_SuccessText.text = $"{completedCount} Succesful";

            // Mavi barda yazacak XP metni (Başarısız olsa bile tamamladığı görevlerin XP'sini alır)
            if (unsuccessPanel_XpText != null)
                unsuccessPanel_XpText.text = earnedXP > 0 ? $"+{earnedXP} XP EARNED" : "0 XP EARNED";
        }

        if (pauseTimeOnSummary)
        {
            Time.timeScale = 0f;
        }
    }

    /// <summary>
    /// Bir sonraki tura geçmek için çağırabilirsiniz.
    /// </summary>
    public void AdvanceToNextRound()
    {
        currentRound++;
        ClosePanels();
    }

    /// <summary>
    /// Panelleri kapatıp zamanı normale döndürür.
    /// </summary>
    public void ClosePanels()
    {
        if (successfulPanel != null) successfulPanel.SetActive(false);
        if (unsuccessfulPanel != null) unsuccessfulPanel.SetActive(false);

        Time.timeScale = 1f;
    }
}