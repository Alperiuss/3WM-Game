using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemCard : MonoBehaviour, IPointerClickHandler
{
    [Header("Item Configuration")]
    [SerializeField] private string itemID = "item_unique_id";
    [SerializeField] private string itemName = "Kitchen Item";
    [TextArea(2, 5)]
    [SerializeField] private string itemDescription = "Bu eşya mutfağınızın verimliliğini artırır."; // YENİ EKLENEN AÇIKLAMA ALANI

    [Header("Upgrade Settings")]
    [SerializeField] private bool isUpgradable = false; 
    [SerializeField] private int basePrice = 100;
    [SerializeField] private float priceMultiplier = 1.5f; 
    [SerializeField] private int maxLevel = 5;            

    [Header("UI References")]
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text levelText;

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
            buyButton.onClick.AddListener(OpenConfirmationPopup);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OpenConfirmationPopup();
    }

    private void OpenConfirmationPopup()
{
    bool isCompleted = isUpgradable ? (currentLevel >= maxLevel) : isSinglePurchased;
    if (isCompleted) return;

    if (ConfirmationPanelMAnager.Instance != null)
    {
        // Inspector'dan girilen itemName ve itemDescription verilerini panele aktarır
        ConfirmationPanelMAnager.Instance.OpenConfirmation(this, itemName, itemDescription, currentPrice);
    }
}

    public void ConfirmPurchase()
    {
        if (isUpgradable)
        {
            if (currentLevel >= maxLevel) return;

            if (CurrencyManager.Instance != null && CurrencyManager.Instance.TrySpendFame(currentPrice))
            {
                currentLevel++;
                SaveData();
                CalculatePrice();
                UpdateCardUI();
                Debug.Log($"[UPGRADE SUCCESS]: {itemName} Level {currentLevel} oldu!");
            }
        }
        else
        {
            if (isSinglePurchased) return;

            if (CurrencyManager.Instance != null && CurrencyManager.Instance.TrySpendFame(basePrice))
            {
                isSinglePurchased = true;
                SaveData();
                UpdateCardUI();
                Debug.Log($"[PURCHASE SUCCESS]: {itemName} satın alındı!");
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
        bool isCompleted = isUpgradable ? (currentLevel >= maxLevel) : isSinglePurchased;

        if (buyButton != null)
        {
            Image buttonImage = buyButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                Color buttonColor = buttonImage.color;
                buttonColor.a = isCompleted ? 1f : 0f; 
                buttonImage.color = buttonColor;
            }
        }

        if (isUpgradable)
        {
            if (levelText != null)
            {
                levelText.gameObject.SetActive(true);
                levelText.text = (currentLevel >= maxLevel) ? "MAX LEVEL" : $"Lvl {currentLevel}";
            }

            if (priceText != null)
            {
                priceText.text = (currentLevel >= maxLevel) ? "Completed" : $"{currentPrice} XP";
            }
        }
        else
        {
            if (levelText != null)
            {
                levelText.gameObject.SetActive(false);
            }

            if (priceText != null)
            {
                priceText.text = isSinglePurchased ? "Purchased" : $"{basePrice} XP";
            }
        }
    }

    private void SaveData()
    {
        if (isUpgradable) PlayerPrefs.SetInt("Level_" + itemID, currentLevel);
        else PlayerPrefs.SetInt("Bought_" + itemID, isSinglePurchased ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        if (isUpgradable) currentLevel = PlayerPrefs.GetInt("Level_" + itemID, 0);
        else isSinglePurchased = PlayerPrefs.GetInt("Bought_" + itemID, 0) == 1;
    }

    public string ItemName => itemName;
    public string ItemDescription => itemDescription;
}