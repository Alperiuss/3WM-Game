using UnityEngine;

public  class ItemSaveManager :MonoBehaviour
{
    private const string ITEM_PREFIX = "PurchasedItem_";

    // Eşyanın satın alınıp alınmadığını kontrol eder
    public static bool IsItemPurchased(string itemId)
    {
        return PlayerPrefs.GetInt(ITEM_PREFIX + itemId, 0) == 1;
    }

    // Eşyayı satın alındı olarak işaretler
    public static void SetItemPurchased(string itemId, bool purchased = true)
    {
        PlayerPrefs.SetInt(ITEM_PREFIX + itemId, purchased ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Seviyeli / birden fazla alınabilen itemlar için miktar/seviye kaydı (opsiyonel)
    public static int GetItemLevel(string itemId)
    {
        return PlayerPrefs.GetInt(ITEM_PREFIX + itemId + "_Level", 0);
    }

    public static void IncrementItemLevel(string itemId)
    {
        int currentLevel = GetItemLevel(itemId);
        PlayerPrefs.SetInt(ITEM_PREFIX + itemId + "_Level", currentLevel + 1);
        PlayerPrefs.Save();
    }

    // Game jam testlerinde PlayerPrefs'i hızlıca sıfırlamak için
    public static void ResetAllPurchases()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[ItemSaveManager] Tüm satın alımlar sıfırlandı!");
    }
}