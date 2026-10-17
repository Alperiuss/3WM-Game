using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfirmationPanelMAnager : MonoBehaviour
{
    public static ConfirmationPanelMAnager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject popupPanel;

    [Header("UI Text References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText; // AÇIKLAMA METNİ İÇİN YENİ REFERANS
    [SerializeField] private TMP_Text priceText;       // İSTEĞE BAĞLI FİYAT METNİ

    [Header("UI Buttons")]
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private ItemCard pendingItemCard;

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
    }

    private void Start()
    {
        if (yesButton != null) yesButton.onClick.AddListener(OnYesClicked);
        if (noButton != null) noButton.onClick.AddListener(OnNoClicked);

        HidePanel();
    }

    // Kart tıklandığında çalışan açılış metodu
    public void OpenConfirmation(ItemCard card, string title, string description, int price = 0)
    {
        pendingItemCard = card;

        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = description;
        if (priceText != null && price > 0) priceText.text = $"{price} Fame";

        if (popupPanel != null) popupPanel.SetActive(true);
    }

    private void OnYesClicked()
    {
        if (pendingItemCard != null)
        {
            pendingItemCard.ConfirmPurchase();
        }

        HidePanel();
    }

    private void OnNoClicked()
    {
        HidePanel();
    }

    public void HidePanel()
    {
        pendingItemCard = null;
        if (popupPanel != null) popupPanel.SetActive(false);
    }
}