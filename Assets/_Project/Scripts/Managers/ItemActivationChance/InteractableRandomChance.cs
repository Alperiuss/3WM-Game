using UnityEngine;
using System.Collections.Generic;

public class InteractableRandomChance : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private List<InteractableObject> allInteractable;
    [SerializeField] private int activeCountPerRound = 5; // Her tur kaç tanesi aktif olacak? (5 tane)

    private void Start()
    {
        StartNewRound();
    }

    public void StartNewRound()
    {
        if (allInteractable == null || allInteractable.Count == 0)
        {
            Debug.LogWarning(">>> HATA: allInteractable listesi boş veya null!");
            return;
        }

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
        for (int i = 0; i < countToActivate; i++)
        {
            shuffledList[i].SetInteractableState(true);
        }

        Debug.Log("Yeni tur başladı: " + countToActivate + " nesne aktif edildi.");
    }

}
