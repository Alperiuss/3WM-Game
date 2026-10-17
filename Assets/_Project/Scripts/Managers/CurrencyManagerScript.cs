using UnityEngine;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("Currency Settings")]
    [SerializeField] private int currentFame = 1000;

    [Header("UI References")]
    [SerializeField] private TMP_Text fameText;
    [SerializeField] private GameObject shopPanel;   // Kapanacak Mağaza Paneli

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
        }

        LoadFame();
    }

    private void Start()
    {
        UpdateUI();

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
        return false;
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
        // 1. Mağaza panelini kapat
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }
}