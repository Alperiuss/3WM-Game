using UnityEngine;
using System.Collections.Generic;

public class InteractableRandomChance : MonoBehaviour
{
    public static InteractableRandomChance Instance;

    [Header("Settings")]
    [SerializeField] private List<InteractableObject> allInteractable;
    [SerializeField] private int activeCountPerRound = 5; // Her tur kaç tanesi aktif olacak? (5 tane)

    [Header("UI Settings")]
    [SerializeField] private GameObject survivalMessageUI; // "Saklan ve Kurtul!" yazısının olduğu UI objesi

    private int remainingActiveCount = 0;

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
        if (survivalMessageUI != null)
            survivalMessageUI.SetActive(false); // Başlangıçta gizle

        StartNewRound();
    }

    public void StartNewRound()
    {
        if (allInteractable == null || allInteractable.Count == 0)
        {
            Debug.LogWarning(">>> HATA: allInteractable listesi boş veya null!");
            return;
        }

        // Yeni tur başladığında mesajı tekrar kapat
        if (survivalMessageUI != null)
            survivalMessageUI.SetActive(false);

        // 1. Önce bütün objeleri pasif (gri) yapalım
        foreach (var obj in allInteractable)
        {
            obj.SetInteractableState(false);
        }

        // 2. Listeyi rastgele karıştırmak için kopyalayalım
        List<InteractableObject> shuffledList = new List<InteractableObject>(allInteractable);
        for (int i = 0; i < shuffledList.Count; i++)
        {
            InteractableObject temp = shuffledList[i];
            int randomIndex = Random.Range(i, shuffledList.Count);
            shuffledList[i] = shuffledList[randomIndex];
            shuffledList[randomIndex] = temp;
        }

        // 3. İstediğimiz sayı kadarını (örn. 5 tanesini) rastgele aktif yapalım
        int countToActivate = Mathf.Min(activeCountPerRound, shuffledList.Count);
        remainingActiveCount = countToActivate; // Sayaç güncellendi

        for (int i = 0; i < countToActivate; i++)
        {
            shuffledList[i].SetInteractableState(true);
        }

        Debug.Log("Yeni tur başladı: " + countToActivate + " nesne aktif edildi.");
    }

    // Aktif nesnelerden biri çözüldüğünde çağrılacak fonksiyon
    public void NotifyObjectResolved()
    {
        remainingActiveCount--;
        Debug.Log("Kalan aktif nesne: " + remainingActiveCount);

        // Tüm aktif nesneler bittiyse
        if (remainingActiveCount <= 0)
        {
            TriggerSurvivalPhase();
        }
    }

    private void TriggerSurvivalPhase()
    {
        Debug.Log("Bütün objeler tamamlandı: SAKLAN VE KURTUL!");

        if (survivalMessageUI != null)
        {
            survivalMessageUI.SetActive(true); // Ekranda yazıyı göster
        }
    }
}