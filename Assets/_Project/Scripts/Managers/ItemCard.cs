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
    [SerializeField] private string itemDescription = "Bu eşya mutfağınızın verimliliğini artırır.";

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
    private bool listenersBound;

    private void OnEnable()
    {
        BindListeners();
        RefreshFromSave();
    }

    private void Start()
    {
        BindListeners();
        RefreshFromSave();
    }

    private void BindListeners()
    {
        if (listenersBound || buyButton == null)
            return;

        buyButton.onClick.RemoveListener(OpenConfirmationPopup);
        buyButton.onClick.AddListener(OpenConfirmationPopup);
        listenersBound = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OpenConfirmationPopup();
    }

    private void OpenConfirmationPopup()
    {
        if (IsCompleted())
            return;

        if (ConfirmationPanelMAnager.Instance != null)
            ConfirmationPanelMAnager.Instance.OpenConfirmation(this, itemName, itemDescription);
    }

    public bool ConfirmPurchase()
    {
        if (IsCompleted())
            return false;

        if (CurrencyManager.Instance == null)
            return false;

        int price = isUpgradable ? currentPrice : basePrice;
        if (!CurrencyManager.Instance.TrySpendFame(price))
            return false;

        if (isUpgradable)
            currentLevel++;
        else
            isSinglePurchased = true;

        SaveData();
        CalculatePrice();
        UpdateCardUI();

        if (ItemEffectsManager.Instance != null)
            ItemEffectsManager.Instance.LoadAllItemEffects();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("Coins");

        return true;
    }

    private bool IsCompleted()
    {
        return isUpgradable ? currentLevel >= maxLevel : isSinglePurchased;
    }

    private void RefreshFromSave()
    {
        LoadData();
        CalculatePrice();
        UpdateCardUI();
    }

    private void CalculatePrice()
    {
        if (isUpgradable)
            currentPrice = Mathf.RoundToInt(basePrice * Mathf.Pow(priceMultiplier, currentLevel));
        else
            currentPrice = basePrice;
    }

    private void UpdateCardUI()
    {
        bool isCompleted = IsCompleted();

        if (buyButton != null)
        {
            buyButton.interactable = !isCompleted;
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
                priceText.text = (currentLevel >= maxLevel) ? "Completed" : $"{currentPrice} XP";
        }
        else
        {
            if (levelText != null)
                levelText.gameObject.SetActive(false);

            if (priceText != null)
                priceText.text = isSinglePurchased ? "Purchased" : $"{basePrice} XP";
        }
    }

    private void SaveData()
    {
        if (isUpgradable)
            PlayerPrefs.SetInt("Level_" + itemID, currentLevel);
        else
            PlayerPrefs.SetInt("Bought_" + itemID, isSinglePurchased ? 1 : 0);

        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        if (isUpgradable)
            currentLevel = PlayerPrefs.GetInt("Level_" + itemID, 0);
        else
            isSinglePurchased = PlayerPrefs.GetInt("Bought_" + itemID, 0) == 1;
    }

    public string ItemName => itemName;
    public string ItemDescription => itemDescription;
}
