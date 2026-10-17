using UnityEngine;

public class ItemEffectManager : MonoBehaviour
{
    public static ItemEffectManager Instance { get; private set; }

    [Header("Tamir & İnşaat Etkileri")]
    [Tooltip("Hızlı tamir kiti çarpanı. Normal = 1.0, Seviye arttıkça yükselir.")]
    public float repairSpeedMultiplier = 1f;
    public int electricalTapeCount = 0;   // Elektrik bandı adedi
    public int screwCount = 0;            // Vida adedi
    public int houseUpgradeLevel = 0;     // Ev geliştirmesi seviyesi

    [Header("Hayatta Kalma & Karakter Etkileri")]
    [Tooltip("Sakinleştirici çarpanı. Normal = 1.0, Panik dolumunu yavaşlatmak için 1'den küçük olur (örn: 0.70).")]
    public float panicRateMultiplier = 1f;
    public bool hasDisasterBag = false;   // Afet çantası var mı?
    public bool hasGasAlarm = false;      // Gaz alarmı var mı?
    public bool hasLocker = false;        // Locker / Dolap var mı?

    private void Awake()
    {
        // Singleton Yapısı: Diğer scriptlerden tek satırda erişebilmek için
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Sahne açılır açılmaz PlayerPrefs kayıtlarını oku ve değerleri hesapla
        LoadAllItemEffects();
    }

    /// <summary>
    /// ItemCard'ın PlayerPrefs'e kaydettiği tüm verileri okur ve oyun içi değerlere yansıtır.
    /// </summary>
    public void LoadAllItemEffects()
    {
        // -------------------------------------------------------------
        // 1. SEVİYELİ (UPGRADABLE) EŞYALAR: "Level_" + itemID
        // -------------------------------------------------------------

        // Hızlı Tamir Kiti (Her seviye tamir hızını %25 artırır)
        int repairLevel = PlayerPrefs.GetInt("Level_quick_repair_kit", 0);
        repairSpeedMultiplier = 1f + (repairLevel * 0.25f);

        // Sakinleştirici (Her seviye panik dolmasını %15 yavaşlatır, min 0.20 sınırı)
        int sedativeLevel = PlayerPrefs.GetInt("Level_sedative", 0);
        panicRateMultiplier = Mathf.Clamp(1f - (sedativeLevel * 0.15f), 0.2f, 1f);

        // Ev Geliştirmesi Seviyesi
        houseUpgradeLevel = PlayerPrefs.GetInt("Level_house_upgrade", 0);

        // Elektrik Bandı & Vida (Eğer seviyeli/adetli satın alınıyorsa)
        electricalTapeCount = PlayerPrefs.GetInt("Level_electrical_tape", 0);
        screwCount = PlayerPrefs.GetInt("Level_screw", 0);


        // -------------------------------------------------------------
        // 2. TEK SEFERLİK (SINGLE PURCHASE) EŞYALAR: "Bought_" + itemID
        // -------------------------------------------------------------

        hasDisasterBag = PlayerPrefs.GetInt("Bought_disaster_bag", 0) == 1;
        hasGasAlarm = PlayerPrefs.GetInt("Bought_gas_alarm", 0) == 1;
        hasLocker = PlayerPrefs.GetInt("Bought_locker", 0) == 1;

        // Eğer elektrik bandı ve vida tek seferlik eşya yapıldıysa:
        if (PlayerPrefs.GetInt("Bought_electrical_tape", 0) == 1 && electricalTapeCount == 0)
        {
            electricalTapeCount = 1;
        }
        if (PlayerPrefs.GetInt("Bought_screw", 0) == 1 && screwCount == 0)
        {
            screwCount = 1;
        }

        Debug.Log("[ItemEffectsManager] Eşya etkileri PlayerPrefs'ten başarıyla yüklendi!");
    }

    // =========================================================================
    // TEST & DEBUG KISAYOLLARI (Inspector'da script başlığına sağ tıklayarak açılır)
    // =========================================================================

    [ContextMenu("Test: Kayıtları Konsola Yazdır")]
    private void DebugPrintSavedItems()
    {
        Debug.Log("--- PLAYERPREFS KAYIT DURUMU ---");
        Debug.Log($"quick_repair_kit Lvl: {PlayerPrefs.GetInt("Level_quick_repair_kit", 0)}");
        Debug.Log($"sedative Lvl: {PlayerPrefs.GetInt("Level_sedative", 0)}");
        Debug.Log($"house_upgrade Lvl: {PlayerPrefs.GetInt("Level_house_upgrade", 0)}");
        Debug.Log($"gas_alarm Alındı mı: {PlayerPrefs.GetInt("Bought_gas_alarm", 0) == 1}");
        Debug.Log($"locker Alındı mı: {PlayerPrefs.GetInt("Bought_locker", 0) == 1}");
        Debug.Log($"disaster_bag Alındı mı: {PlayerPrefs.GetInt("Bought_disaster_bag", 0) == 1}");
    }

    [ContextMenu("Test: Tüm PlayerPrefs Kayıtlarını Sil")]
    private void DebugClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.LogWarning("[ItemEffectsManager] Tüm satın alma kayıtları sıfırlandı!");
    }
}