using UnityEngine;
using System.Collections; // Coroutine için gerekli
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("Currency Settings")]
    [SerializeField] private int currentFame = 1000;

    [Header("UI References")]
    [SerializeField] private TMP_Text fameText;
    [SerializeField] private GameObject shopPanel;   // Kapanacak Mağaza Paneli

    [Header("Warning Hierarchy References")]
    [SerializeField] private GameObject warningPanel;      // En üstteki panel
    [SerializeField] private GameObject warningImage;      // Panel içindeki görsel/arka plan
    [SerializeField] private TMP_Text warningText;          // Yazı (TextMeshPro)
    [SerializeField] private float warningDuration = 2f;    // Ekranda kalma süresi

    private Coroutine warningCoroutine;
    private const string FAME_KEY = "Player_Fame";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadFame();
    }

    private void Start()
    {
        UpdateUI();

        // Oyun başladığında uyarı hiyerarşisini tamamen gizle
        SetWarningActive(false);
    }

    // --- CURRENCY METOTLARI ---

    public void AddFame(int amount)
    {
        currentFame += amount;
        SaveFame();
        UpdateUI();
    }

    public bool TrySpendFame(int amount)
    {
        if (currentFame >= amount)
        {
            currentFame -= amount;
            SaveFame();
            UpdateUI();
            return true;
        }
        else
        {
            // Para yetmediğinde hiyerarşiyi sırayla açıp uyarıyı göster
            ShowWarning();
            return false;
        }
    }

    /// <summary>
    /// Uyarı hiyerarşisini açar ve belirlenen süre sonra kapatır.
    /// </summary>
    public void ShowWarning()
    {
        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
        }

        warningCoroutine = StartCoroutine(WarningRoutine());
    }

    private IEnumerator WarningRoutine()
    {
        // Panel, Image ve Text'i aktif et
        SetWarningActive(true);

        // Belirlenen süre kadar ekranda tut
        yield return new WaitForSecondsRealtime(warningDuration);

        // Süre dolunca tekrar kapat
        SetWarningActive(false);
        warningCoroutine = null;
    }

    /// <summary>
    /// Hiyerarşideki tüm parçaları güvenli şekilde açar veya kapatır.
    /// </summary>
    private void SetWarningActive(bool isActive)
    {
        if (warningPanel != null) warningPanel.SetActive(isActive);
        if (warningImage != null) warningImage.SetActive(isActive);
        if (warningText != null) warningText.gameObject.SetActive(isActive);
    }

    private void SaveFame()
    {
        PlayerPrefs.SetInt(FAME_KEY, currentFame);
        PlayerPrefs.Save();
    }

    private void LoadFame()
    {
        currentFame = PlayerPrefs.GetInt(FAME_KEY, 1000);
    }

    private void UpdateUI()
    {
        if (fameText != null)
        {
            fameText.text = currentFame.ToString("N0");
        }
    }

    // --- CONTINUE BUTONU METODU ---

    public void OnContinuePressed()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnMarketContinuePressed();
            return;
        }

        if (shopPanel != null)
            shopPanel.SetActive(false);
    }
}