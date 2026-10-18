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
    [SerializeField] private int xpPerCompletedTask = 50;
    [SerializeField] private int victoryBonusXP = 100;
    [SerializeField] private bool autoAddToCurrencyManager = true;

    [Header("Paneller")]
    [SerializeField] private GameObject successfulPanel;
    [SerializeField] private GameObject unsuccessfulPanel;

    [Header("Successful Panel Metinleri")]
    [SerializeField] private TextMeshProUGUI successPanel_SuccessText;
    [SerializeField] private TextMeshProUGUI successPanel_FailText;
    [SerializeField] private TextMeshProUGUI successPanel_XpText;

    [Header("Unsuccessful Panel Metinleri")]
    [SerializeField] private TextMeshProUGUI unsuccessPanel_FailText;
    [SerializeField] private TextMeshProUGUI unsuccessPanel_SuccessText;
    [SerializeField] private TextMeshProUGUI unsuccessPanel_XpText;

    [Header("Zaman Ayarı")]
    [SerializeField] private bool pauseTimeOnSummary = true;

    private bool hasGrantedXpThisRound;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (successfulPanel != null) successfulPanel.SetActive(false);
        if (unsuccessfulPanel != null) unsuccessfulPanel.SetActive(false);
    }

    public void PrepareForNewRound()
    {
        hasGrantedXpThisRound = false;
    }

    [ContextMenu("Test Summary")]
    public void ShowSummary()
    {
        int completedCount = 0;
        int uncompletedCount = 0;

        if (TaskUIManager.Instance != null)
            TaskUIManager.Instance.GetTaskCounts(out completedCount, out uncompletedCount);
        else
            Debug.LogError("[SummaryPanelManager] TaskUIManager bulunamadı!");

        int roundIndex = Mathf.Clamp(currentRound - 1, 0, requiredTasksPerRound.Length - 1);
        int targetRequired = requiredTasksPerRound[roundIndex];

        bool survivedDay = InteractableRandomChance.Instance != null
            && InteractableRandomChance.Instance.WasDayCompletedSuccessfully;
        bool isSuccess = survivedDay || completedCount >= targetRequired;

        int earnedXP = completedCount * xpPerCompletedTask;
        if (isSuccess)
            earnedXP += victoryBonusXP;

        if (autoAddToCurrencyManager && CurrencyManager.Instance != null && earnedXP > 0 && !hasGrantedXpThisRound)
        {
            CurrencyManager.Instance.AddFame(earnedXP);
            hasGrantedXpThisRound = true;
        }

        if (successfulPanel != null) successfulPanel.SetActive(false);
        if (unsuccessfulPanel != null) unsuccessfulPanel.SetActive(false);

        if (isSuccess)
        {
            if (successfulPanel != null)
            {
                successfulPanel.SetActive(true);
                successfulPanel.transform.SetAsLastSibling();
            }

            if (successPanel_SuccessText != null)
                successPanel_SuccessText.text = $"{completedCount} mission succesful";

            if (successPanel_FailText != null)
                successPanel_FailText.text = $"{uncompletedCount} mission unsuccesful";

            if (successPanel_XpText != null)
                successPanel_XpText.text = $"+{earnedXP} XP EARNED";
        }
        else
        {
            if (unsuccessfulPanel != null)
            {
                unsuccessfulPanel.SetActive(true);
                unsuccessfulPanel.transform.SetAsLastSibling();
            }

            if (unsuccessPanel_FailText != null)
                unsuccessPanel_FailText.text = $"{uncompletedCount} Unsuccesful";

            if (unsuccessPanel_SuccessText != null)
                unsuccessPanel_SuccessText.text = $"{completedCount} Succesful";

            if (unsuccessPanel_XpText != null)
                unsuccessPanel_XpText.text = earnedXP > 0 ? $"+{earnedXP} XP EARNED" : "0 XP EARNED";
        }

        if (pauseTimeOnSummary)
            Time.timeScale = 0f;
    }

    public void AdvanceToNextRound()
    {
        currentRound++;
        ClosePanels(false);
    }

    public void ClosePanels(bool restoreTimeScale = false)
    {
        if (successfulPanel != null) successfulPanel.SetActive(false);
        if (unsuccessfulPanel != null) unsuccessfulPanel.SetActive(false);

        if (restoreTimeScale)
            Time.timeScale = 1f;
    }
}
