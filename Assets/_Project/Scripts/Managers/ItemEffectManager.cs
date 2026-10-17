using UnityEngine;

public class ItemEffectsManager : MonoBehaviour
{
    public static ItemEffectsManager Instance { get; private set; }

    [Header("Tamir & İnşaat Eşyaları")]
    [Tooltip("Hızlı Tamir Kiti alındıysa tamir hızı artar (Örn: %50 daha hızlı)")]
    public float repairSpeedMultiplier = 1f;
    public bool hasQuickRepairKit = false;
    public bool hasElectricalTape = false; // Elektrik Bandı alındı mı?
    public bool hasScrew = false;          // Vida alındı mı?
    public bool hasHouseUpgrade = false;   // Ev Geliştirmesi alındı mı?

    [Header("Hayatta Kalma & Karakter Eşyaları")]
    [Tooltip("Sakinleştirici alındıysa panik dolumu yavaşlar (Örn: 0.70x çarpan)")]
    public float panicRateMultiplier = 1f;
    public bool hasSedative = false;       // Sakinleştirici alındı mı?
    public bool hasDisasterBag = false;    // Afet Çantası alındı mı?
    public bool hasGasAlarm = false;       // Gaz Alarmı alındı mı?
    public bool hasLocker = false;         // Locker / Dolap alındı mı?

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

        // Oyun sahnesi açılır açılmaz tüm eşyaların alınıp alınmadığını kontrol et
        LoadAllItemEffects();
    }

    /// <summary>
    /// PlayerPrefs'ten "Bought_itemID" kayıtlarını okur ve oyun içi etkilere dönüştürür.
    /// </summary>
    public void LoadAllItemEffects()
    {
        // -------------------------------------------------------------
        // 1. TAMİR & İNŞAAT EŞYALARI
        // -------------------------------------------------------------
        hasQuickRepairKit = IsBought("quick_repair_kit");
        hasElectricalTape = IsBought("electrical_tape");
        hasScrew          = IsBought("screw");
        hasHouseUpgrade   = IsBought("house_upgrade");

        // Hızlı tamir kiti alındıysa tamir hızını 1.5x (%50 daha hızlı) yap, yoksa normal 1.0x
        repairSpeedMultiplier = hasQuickRepairKit ? 1.5f : 1.0f;


        // -------------------------------------------------------------
        // 2. HAYATTA KALMA & KARAKTER EŞYALARI
        // -------------------------------------------------------------
        hasSedative    = IsBought("sedative");
        hasDisasterBag = IsBought("disaster_bag");
        hasGasAlarm    = IsBought("gas_alarm");
        hasLocker      = IsBought("locker");

        // Sakinleştirici alındıysa panik dolumunu %30 yavaşlat (0.7x), yoksa normal 1.0x
        panicRateMultiplier = hasSedative ? 0.7f : 1.0f;

        Debug.Log("[ItemEffectsManager] Tüm tekil eşyalar başarıyla yüklendi!");
    }

    /// <summary>
    /// ItemCard'ın kaydettiği "Bought_" anahtarına bakar.
    /// </summary>
    private bool IsBought(string itemID)
    {
        return PlayerPrefs.GetInt("Bought_" + itemID, 0) == 1;
    }

    // =========================================================================
    // INSPECTOR SAĞ TIK TEST MENÜSÜ (Game Jam testleri için aşırı pratik)
    // =========================================================================

    [ContextMenu("Test: Alınan Eşyaları Konsola Yaz")]
    private void DebugPrintItems()
    {
        Debug.Log("==== [SATIN ALINAN EŞYALAR] ====");
        Debug.Log($"Hızlı Tamir Kiti : {hasQuickRepairKit} (Hız Çarpanı: {repairSpeedMultiplier})");
        Debug.Log($"Sakinleştirici   : {hasSedative} (Panik Çarpanı: {panicRateMultiplier})");
        Debug.Log($"Elektrik Bandı   : {hasElectricalTape}");
        Debug.Log($"Vida             : {hasScrew}");
        Debug.Log($"Ev Geliştirmesi  : {hasHouseUpgrade}");
        Debug.Log($"Afet Çantası     : {hasDisasterBag}");
        Debug.Log($"Gaz Alarmı       : {hasGasAlarm}");
        Debug.Log($"Locker (Dolap)   : {hasLocker}");
    }

    [ContextMenu("Test: Tüm Alımları Sıfırla (PlayerPrefs Reset)")]
    private void DebugResetAllPurchases()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        LoadAllItemEffects();
        Debug.LogWarning("[ItemEffectsManager] Tüm eşya alımları sıfırlandı!");
    }
}