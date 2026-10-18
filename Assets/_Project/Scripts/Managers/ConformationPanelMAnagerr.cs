using System.Collections;
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
    [SerializeField] private TMP_Text descriptionText;

    [Header("UI Buttons")]
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private ItemCard pendingItemCard;
    private Coroutine warningCoroutine;

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

    // 1. NORMAL ONAY EKRANI (Satın Alınsın mı?)
    public void OpenConfirmation(ItemCard card, string title, string description)
    {
        if (warningCoroutine != null) StopCoroutine(warningCoroutine);

        pendingItemCard = card;

        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = description;

        // Butonları görünür yap
        if (yesButton != null) yesButton.gameObject.SetActive(true);
        if (noButton != null) noButton.gameObject.SetActive(true);

        if (popupPanel != null) popupPanel.SetActive(true);
    }

    // 2. OTOMATİK KAPANAN UYARI EKRANI (YOU DONT HAVE ENOUGH XP!)
    public void ShowWarning(string warningMessage, float duration = 2f)
    {
        if (warningCoroutine != null) StopCoroutine(warningCoroutine);

        pendingItemCard = null;

        if (titleText != null) titleText.text = warningMessage;
        if (descriptionText != null) descriptionText.text = "";

        // Uyarı mesajında YES / NO butonlarını gizle
        if (yesButton != null) yesButton.gameObject.SetActive(false);
        if (noButton != null) noButton.gameObject.SetActive(false);

        if (popupPanel != null) popupPanel.SetActive(true);

        // Belirtilen süre bittiğinde otomatik kapat
        warningCoroutine = StartCoroutine(HideAfterDelay(duration));
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        HidePanel();
    }

    private void OnYesClicked()
    {
        if (pendingItemCard != null)
            pendingItemCard.ConfirmPurchase();

        HidePanel();
    }

    private void OnNoClicked()
    {
        HidePanel();
    }

    public void HidePanel()
    {
        if (warningCoroutine != null) StopCoroutine(warningCoroutine);

        pendingItemCard = null;
        if (popupPanel != null) popupPanel.SetActive(false);
    }
}