using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemCard : MonoBehaviour
{
    [Header("Item Configuration")]
    [SerializeField] private string itemID = "item_unique_id";
    [SerializeField] private string itemName = "Kitchen Item";

    [Header("Upgrade Settings")]
    [Tooltip("Check this if the item can be upgraded multiple times (e.g. Home upgrades). Uncheck for single-purchase items.")]
    [SerializeField] private bool isUpgradable = false; 
    
    [SerializeField] private int basePrice = 100;
    [SerializeField] private float priceMultiplier = 1.5f; // Only used if isUpgradable = true
    [SerializeField] private int maxLevel = 5;            // Only used if isUpgradable = true

    [Header("UI References")]
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text levelText; // Level'ı gösteren küçük metin

    // Runtime state
    private int currentLevel = 0;
    private bool isSinglePurchased = false;
    private int currentPrice;

    private void Start()
    {
        LoadData();
        CalculatePrice();
        UpdateCardUI();

        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }
    }

    private void OnBuyButtonClicked()
    {
        // 1. GELİŞTİRİLEBİLİR (UPGRADABLE) EŞYA MANTIĞI
        if (isUpgradable)
        {
            if (currentLevel >= maxLevel) return;

            if (CurrencyManager.Instance.TrySpendFame(currentPrice))
            {
                currentLevel++;
                SaveData();
                CalculatePrice();
                UpdateCardUI();

                Debug.Log($"[UPGRADE]: {itemName} upgraded to Level {currentLevel}");
            }
        }
        // 2. TEK SEFERLİK (SINGLE-PURCHASE) EŞYA MANTIĞI
        else
        {
            if (isSinglePurchased) return;

            if (CurrencyManager.Instance.TrySpendFame(basePrice))
            {
                isSinglePurchased = true;
                SaveData();
                UpdateCardUI();

                Debug.Log($"[PURCHASE]: {itemName} bought successfully!");
            }
        }
    }

    private void CalculatePrice()
    {
        if (isUpgradable)
        {
            currentPrice = Mathf.RoundToInt(basePrice * Mathf.Pow(priceMultiplier, currentLevel));
        }
        else
        {
            currentPrice = basePrice;
        }
    }

    private void UpdateCardUI()
    {
        // GELİŞTİRİLEBİLİR EŞYA İÇİN UI
        if (isUpgradable)
        {
            if (levelText != null)
            {
                levelText.gameObject.SetActive(true); // Level yazısını göster
                levelText.text = (currentLevel >= maxLevel) ? "MAX LEVEL" : $"Lvl {currentLevel}";
            }

            if (priceText != null && buyButton != null)
            {
                if (currentLevel >= maxLevel)
                {
                    priceText.text = "Completed";
                    buyButton.interactable = false;
                }
                else
                {
                    priceText.text = $"{currentPrice} Fame";
                    buyButton.interactable = true;
                }
            }
        }
        // TEK SEFERLİK EŞYA İÇİN UI
        else
        {
            if (levelText != null)
            {
                levelText.gameObject.SetActive(false); // Tek seferlik eşyada Level yazısını gizle
            }

            if (priceText != null && buyButton != null)
            {
                if (isSinglePurchased)
                {
                    priceText.text = "Purchased";
                    buyButton.interactable = false;
                }
                else
                {
                    priceText.text = $"{basePrice} Fame";
                    buyButton.interactable = true;
                }
            }
        }
    }

    // KAYIT VE YÜKLEME MANTIĞI
    private void SaveData()
    {
        if (isUpgradable)
        {
            PlayerPrefs.SetInt("Level_" + itemID, currentLevel);
        }
        else
        {
            PlayerPrefs.SetInt("Bought_" + itemID, isSinglePurchased ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        if (isUpgradable)
        {
            currentLevel = PlayerPrefs.GetInt("Level_" + itemID, 0);
        }
        else
        {
            isSinglePurchased = PlayerPrefs.GetInt("Bought_" + itemID, 0) == 1;
        }
    }

    // Diğer script'lerin eşya durumunu okuyabilmesi için public metotlar
    public bool IsUpgradable => isUpgradable;
    public int GetCurrentLevel() => currentLevel;
    public bool IsItemPurchased() => isUpgradable ? (currentLevel > 0) : isSinglePurchased;
}