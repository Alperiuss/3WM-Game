using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Item Configuration")]
    [SerializeField] private string itemID = "item_unique_id";
    [SerializeField] private string itemName = "Kitchen Item";
    [TextArea(2, 5)]
    [SerializeField] private string itemDescription = "Bu eşya mutfağınızın verimliliğini artırır."; // Kartın üzerine gelince ve onay penceresinde gösterilir

    [Header("Upgrade Settings")]
    [Tooltip("Kapalıysa eşya sadece 1 kez satın alınabilir")]
    [SerializeField] private bool isUpgradable = false;
    [SerializeField] private int basePrice = 100;
    [SerializeField] private float priceMultiplier = 1.5f;
    [SerializeField] private int maxLevel = 5;

    [Header("UI References")]
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text levelText;

    public string ItemID => itemID;

    private int currentLevel = 0;
    private bool isSinglePurchased = false;
    private int currentPrice;
    private bool isHovered = false;

    private bool IsCompleted => isUpgradable ? (currentLevel >= maxLevel) : isSinglePurchased;

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

    private void OnEnable()
    {
        // Yeni oyunda kayıtlar sıfırlanmış olabilir; market her açıldığında kartı tazele
        LoadData();
        CalculatePrice();
        UpdateCardUI();
    }

    private void OnDisable()
    {
        if (isHovered) MarketTooltip.Hide();
        isHovered = false;
    }

    private void OnDestroy()
    {
        if (buyButton != null) buyButton.onClick.RemoveListener(OpenConfirmationPopup);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OpenConfirmationPopup();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        MarketTooltip.Show(this, itemName, itemDescription, GetStatusText(), priceText != null ? priceText.font : null);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        MarketTooltip.Hide();
    }

    private void OpenConfirmationPopup()
    {
        if (IsCompleted) return;

        MarketTooltip.Hide();

        if (ConfirmationPanelMAnager.Instance != null)
        {
            // Inspector'dan girilen itemName ve itemDescription verilerini panele aktarır
            ConfirmationPanelMAnager.Instance.OpenConfirmation(this, itemName, $"{itemDescription}\n<b>{GetStatusText()}</b>");
        }
        else
        {
            Debug.LogWarning("[ItemCard] Sahnede ConfirmationPanelMAnager yok, satın alma penceresi açılamadı.", this);
        }
    }

    public void ConfirmPurchase()
    {
        if (IsCompleted) return;
        if (CurrencyManager.Instance == null)
        {
            Debug.LogWarning("[ItemCard] Sahnede CurrencyManager yok, satın alma yapılamadı.", this);
            return;
        }

        int price = isUpgradable ? currentPrice : basePrice;
        if (!CurrencyManager.Instance.TrySpendFame(price)) return;

        if (isUpgradable) currentLevel++;
        else isSinglePurchased = true;

        SaveData();
        CalculatePrice();
        UpdateCardUI();
        Debug.Log($"[PURCHASE SUCCESS]: {itemName} satın alındı! (Seviye: {currentLevel})");

        // Pasif etkiler hemen bir sonraki günde geçerli olsun
        if (ItemEffectsManager.Instance != null) ItemEffectsManager.Instance.Refresh();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Coins");
        }
    }

    private string GetStatusText()
    {
        if (IsCompleted) return isUpgradable ? "MAX LEVEL" : "PURCHASED";
        string once = isUpgradable ? $"Level {currentLevel}/{maxLevel}" : "One-time purchase";
        return $"Price: {currentPrice} XP  -  {once}";
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
        bool isCompleted = IsCompleted;

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
