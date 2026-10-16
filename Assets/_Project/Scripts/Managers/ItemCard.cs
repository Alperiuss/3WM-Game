using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemCard : MonoBehaviour
{
    
    public static event Action<ItemCard> OnItemPurchased;

    [Header("Item Data")]
    [SerializeField] private string itemID = "item_sword_01";
    [SerializeField] private string itemName = "Iron Sword";
    [SerializeField] private int itemPrice = 150;
    [SerializeField] private bool isPurchased = false;

    [Header("UI References")]
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text priceText;

    // Public properties to allow external scripts to read item info safely
    public string ItemID => itemID;
    public string ItemName => itemName;
    public int ItemPrice => itemPrice;
    public bool IsPurchased => isPurchased;

    private void Start()
    {
        UpdateCardUI();

        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }
    }

    private void OnBuyButtonClicked()
    {
        if (isPurchased) return;

        // Attempt transaction using CurrencyManager Instance
        if (CurrencyManager.Instance.TrySpendFame(itemPrice))
        {
            isPurchased = true;
            UpdateCardUI();

            // Notify all listening external systems (Inventory, DownPanel, Save System, etc.)
            OnItemPurchased?.Invoke(this);
            
            Debug.Log($"Successfully purchased: {itemName}");
        }
        else
        {
            Debug.LogWarning($"Insufficient Fame to purchase {itemName}!");
        }
    }

    public void UpdateCardUI()
    {
        if (priceText == null || buyButton == null) return;

        if (isPurchased)
        {
            priceText.text = "Purchased";
            buyButton.interactable = false;
        }
        else
        {
            priceText.text = $"{itemPrice} Fame";
            buyButton.interactable = true;
        }
    }
}