using UnityEngine;

/// <summary>
/// Marketten alınan tek seferlik eşyaların oyun içi pasif etkileri.
/// Satın alma durumu ItemCard'ın kaydettiği "Bought_{itemID}" anahtarından okunur.
/// Diğer scriptler statik yardımcıları kullanır; sahnede bu obje olmasa bile varsayılan (etkisiz) değerler döner.
/// </summary>
public class ItemEffectsManager : MonoBehaviour
{
    public static ItemEffectsManager Instance { get; private set; }

    // Market kartlarındaki itemID'ler (Canvas.prefab > ItemCard)
    public const string RepairKitId = "item_repairkit_01";
    public const string StressMedicineId = "item_medication_01";
    public const string ElectricalTapeId = "item_electricaltape_01";
    public const string HouseUpgradeId = "item_houseImprove_01";
    public const string AlarmId = "item_alarm_01";
    public const string FirstAidKitId = "item_firstAid_01";

    [Header("Ev Geliştirmesi")]
    [SerializeField] private float houseUpgradeBonusSeconds = 10f;

    [Header("Tamir Çantası / Elektrik Bandı (süre çarpanı, 0.5 = yarı süre)")]
    [SerializeField] private float repairKitTimeMultiplier = 0.5f;
    [SerializeField] private float electricalTapeTimeMultiplier = 0.5f;

    [Header("Sağlık Kiti")]
    [SerializeField] private int firstAidExtraSafeZones = 1;

    [Header("Stres İlacı (stres dolum çarpanı)")]
    [SerializeField] private float stressMedicineRateMultiplier = 0.9f;

    public bool HasRepairKit { get; private set; }
    public bool HasStressMedicine { get; private set; }
    public bool HasElectricalTape { get; private set; }
    public bool HasHouseUpgrade { get; private set; }
    public bool HasAlarm { get; private set; }
    public bool HasFirstAidKit { get; private set; }

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

        Refresh();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Satın alma kayıtlarını yeniden okur (market alımından veya sıfırlamadan sonra çağrılır).</summary>
    public void Refresh()
    {
        HasRepairKit = IsBought(RepairKitId);
        HasStressMedicine = IsBought(StressMedicineId);
        HasElectricalTape = IsBought(ElectricalTapeId);
        HasHouseUpgrade = IsBought(HouseUpgradeId);
        HasAlarm = IsBought(AlarmId);
        HasFirstAidKit = IsBought(FirstAidKitId);
    }

    /// <summary>Eski "seviyeli" kayıtlar da (Level_) satın alınmış sayılır.</summary>
    public static bool IsBought(string itemID)
    {
        return PlayerPrefs.GetInt("Bought_" + itemID, 0) == 1 || PlayerPrefs.GetInt("Level_" + itemID, 0) > 0;
    }

    // ------------------------------------------------------------------ Statik yardımcılar

    /// <summary>Bir eşyanın tamir süresine uygulanacak çarpan (1 = normal).</summary>
    public static float RepairTimeMultiplier(InteractableData data)
    {
        ItemEffectsManager m = Instance;
        if (m == null) return 1f;

        float multiplier = 1f;
        if (m.HasRepairKit) multiplier *= m.repairKitTimeMultiplier;
        if (m.HasElectricalTape && data != null && data.isElectrical) multiplier *= m.electricalTapeTimeMultiplier;
        return multiplier;
    }

    /// <summary>Her güne eklenen kalıcı bonus süre (saniye).</summary>
    public static float BonusDaySeconds => Instance != null && Instance.HasHouseUpgrade ? Instance.houseUpgradeBonusSeconds : 0f;

    /// <summary>Gösterilecek ilave güvenli alan sayısı.</summary>
    public static int ExtraSafeZones => Instance != null && Instance.HasFirstAidKit ? Instance.firstAidExtraSafeZones : 0;

    /// <summary>Stres barı dolum hızı çarpanı (1 = normal).</summary>
    public static float StressRateMultiplier => Instance != null && Instance.HasStressMedicine ? Instance.stressMedicineRateMultiplier : 1f;

    /// <summary>Gün başında bir tehlikenin yeri işaretlensin mi?</summary>
    public static bool AlarmActive => Instance != null && Instance.HasAlarm;

    // ------------------------------------------------------------------ Test menüsü

    [ContextMenu("Test: Alınan Eşyaları Konsola Yaz")]
    private void DebugPrintItems()
    {
        Refresh();
        Debug.Log($"[ItemEffectsManager] Tamir Çantası={HasRepairKit} | Elektrik Bandı={HasElectricalTape} | Ev Geliştirmesi={HasHouseUpgrade} | " +
                  $"Sağlık Kiti={HasFirstAidKit} | Stres İlacı={HasStressMedicine} | Alarm={HasAlarm}");
    }

    [ContextMenu("Test: Tüm Alımları Sıfırla (PlayerPrefs Reset)")]
    private void DebugResetAllPurchases()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Refresh();
        Debug.LogWarning("[ItemEffectsManager] Tüm eşya alımları sıfırlandı!");
    }
}
