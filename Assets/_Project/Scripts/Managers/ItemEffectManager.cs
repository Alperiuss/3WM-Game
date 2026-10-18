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
        hasQuickRepairKit = IsOwned("item_repairkit_01", "quick_repair_kit");
        hasElectricalTape = IsOwned("item_electricaltape_01", "electrical_tape");
        hasScrew          = IsOwned("item_screw_01", "screw");
        hasHouseUpgrade   = IsOwned("item_houseImprove_01", "house_upgrade");

        // Hızlı tamir kiti alındıysa tamir hızını 1.5x (%50 daha hızlı) yap, yoksa normal 1.0x
        repairSpeedMultiplier = hasQuickRepairKit ? 1.5f : 1.0f;


        // -------------------------------------------------------------
        // 2. HAYATTA KALMA & KARAKTER EŞYALARI
        // -------------------------------------------------------------
        hasSedative    = IsOwned("item_medication_01", "sedative");
        hasDisasterBag = IsOwned("item_firstAid_01", "disaster_bag");
        hasGasAlarm    = IsOwned("item_alarm_01", "gas_alarm");
        hasLocker      = IsOwned("item_Lock_01", "locker");

        // Sakinleştirici alındıysa panik dolumunu %30 yavaşlat (0.7x), yoksa normal 1.0x
        panicRateMultiplier = hasSedative ? 0.7f : 1.0f;

        Debug.Log("[ItemEffectsManager] Tüm tekil eşyalar başarıyla yüklendi!");
    }

    /// <summary>
    /// ItemCard'ın kaydettiği "Bought_" anahtarına bakar.
    /// </summary>
    private bool IsOwned(params string[] itemIDs)
    {
        for (int i = 0; i < itemIDs.Length; i++)
        {
            string id = itemIDs[i];
            if (PlayerPrefs.GetInt("Bought_" + id, 0) == 1)
                return true;
            if (PlayerPrefs.GetInt("Level_" + id, 0) > 0)
                return true;
        }

        return false;
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