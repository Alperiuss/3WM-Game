using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfirmationPanelMAnager : MonoBehaviour
{
    public static ConfirmationPanelMAnager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject popupPanel;     // Pop-up panelinin kendisi (ConfirmationPanel)
    [SerializeField] private TMP_Text itemNameText;     // Tıklanan eşyanın adının yazacağı metin
    [SerializeField] private Button yesButton;          // Satın almayı onaylayan YES butonu
    [SerializeField] private Button noButton;           // Onayı iptal edip markete dönen NO butonu

    // O an tıklanmış ve onay bekleyen kart referansı
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
        }
    }

    private void Start()
    {
        // YES ve NO butonlarının tıklama olaylarını bağlıyoruz
        if (yesButton != null)
        {
            yesButton.onClick.AddListener(OnYesClicked);
        }

        if (noButton != null)
        {
            noButton.onClick.AddListener(OnNoClicked);
        }

        // Oyun başladığında pop-up varsayılan olarak kapalı kalsın
        HidePanel();
    }

    // ItemCard tarafından çağrılan açılış metodu
    public void OpenConfirmation(ItemCard card, string nameOfItem)
    {
        pendingItemCard = card;

        if (itemNameText != null)
        {
            itemNameText.text = nameOfItem;
        }

        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }
    }

    // YES BUTONUNA BASILDIĞINDA (Satın Alma İşlemi Onaylanır)
    private void OnYesClicked()
    {
        if (pendingItemCard != null)
        {
            // Kartın içindeki satın alma/yükseltme mantığını çalıştırır
            pendingItemCard.ConfirmPurchase();
        }

        // İşlem tamamlandıktan sonra paneli kapatır
        HidePanel();
    }

    // NO BUTONUNA BASILDIĞINDA (İptal Edilir ve Markete Dönülür)
    private void OnNoClicked()
    {
        // Hiçbir işlem yapmadan doğrudan paneli kapatarak markete döner
        HidePanel();
    }

    // Paneli Gizleme / Sıfırlama Metodu
    public void HidePanel()
    {
        pendingItemCard = null;

        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }
}