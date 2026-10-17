using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class InteractableRandomChance : MonoBehaviour
{
    public static InteractableRandomChance Instance;

    [Header("Settings")]
    [SerializeField] private List<InteractableObject> allInteractable;
    [SerializeField] private int activeCountPerRound = 5; // Her tur kaç tanesi aktif olacak?

    [Header("Timer Settings")]
    [SerializeField] private float roundTimeLimit = 30f; // Süre (Saniye cinsinden)
    private float currentTimer;
    private bool isTimerRunning = false;

    [Header("Earthquake Settings")]
    [SerializeField] private float earthquakeDuration = 5f;          // Deprem kaç saniye sürecek?
    [SerializeField] private float baseEarthquakeMagnitude = 0.1f;   // Temel sarsıntı şiddeti
    [SerializeField] private float magnitudeMultiplierPerRound = 0.05f; // Tur başına artacak şiddet artışı
    private int currentRound = 0;

    [Header("UI Settings")]
    [SerializeField] private GameObject survivalMessageUI; // "Saklan ve Kurtul!" yazısının olduğu UI objesi
    [SerializeField] private TextMeshProUGUI timerText;    // Ekranda süreyi gösterecek TextMeshPro bileşeni

    private int remainingActiveCount = 0;
    private bool isEarthquakeActive = false;
    private bool isRoundCompletedSuccessfully = false; // Oyuncu tüm item'ları zamanında topladı mı?

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

    private void Update()
    {
        // Zamanlayıcı aktifse geriye doğru say
        if (isTimerRunning)
        {
            if (currentTimer > 0f)
            {
                currentTimer -= Time.deltaTime;

                if (timerText != null)
                {
                    int minutes = Mathf.FloorToInt(currentTimer / 60f);
                    int seconds = Mathf.FloorToInt(currentTimer % 60f);
                    timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                }

                // Süre bittiğinde (00:00 olduğunda) HER KOŞULDA depremi başlat
                if (currentTimer <= 0f)
                {
                    currentTimer = 0f;
                    isTimerRunning = false;

                    StartCoroutine(TriggerEarthquakeRoutine());
                }
            }
        }
    }

    public void StartNewRound()
    {
        if (isEarthquakeActive) return;

        currentRound++; // Her yeni turda tur sayısını artır!
        Debug.Log("--- TUR " + currentRound + " BAŞLADI ---");

        isRoundCompletedSuccessfully = false; // Yeni turda başarı durumunu sıfırla

        if (allInteractable == null || allInteractable.Count == 0)
        {
            Debug.LogWarning(">>> HATA: allInteractable listesi boş veya null!");
            return;
        }

        if (survivalMessageUI != null)
            survivalMessageUI.SetActive(false);

        if (timerText != null)
            timerText.gameObject.SetActive(true); // Sayaç tekrar görünür olsun

        foreach (var obj in allInteractable)
        {
            obj.SetInteractableState(false);
        }

        List<InteractableObject> shuffledList = new List<InteractableObject>(allInteractable);
        for (int i = 0; i < shuffledList.Count; i++)
        {
            InteractableObject temp = shuffledList[i];
            int randomIndex = Random.Range(i, shuffledList.Count);
            shuffledList[i] = shuffledList[randomIndex];
            shuffledList[randomIndex] = temp;
        }

        int countToActivate = Mathf.Min(activeCountPerRound, shuffledList.Count);
        remainingActiveCount = countToActivate;

        for (int i = 0; i < countToActivate; i++)
        {
            shuffledList[i].SetInteractableState(true);
        }

        currentTimer = roundTimeLimit;
        isTimerRunning = true;
    }

    // Aktif nesnelerden biri çözüldüğünde çağrılacak fonksiyon
    public void NotifyObjectResolved()
    {
        if (isEarthquakeActive || !isTimerRunning) return;

        remainingActiveCount--;
        Debug.Log("Kalan aktif nesne: " + remainingActiveCount);

        // Tüm aktif nesneler süre bitmeden bittiyse
        if (remainingActiveCount <= 0)
        {
            TriggerSurvivalPhase();
        }
    }

    private void TriggerSurvivalPhase()
    {
        isRoundCompletedSuccessfully = true; // Tüm item'lar başarıyla toplandı!
        Debug.Log("Bütün objeler zamanında tamamlandı: SAKLAN VE KURTUL!");

        if (survivalMessageUI != null)
            survivalMessageUI.SetActive(true);

        if (timerText != null)
            timerText.gameObject.SetActive(false); // Süre sayacını gizle (Zaten bitti veya gizlendi)
    }

    private IEnumerator TriggerEarthquakeRoutine()
    {
        isEarthquakeActive = true;
        Debug.Log("⚠️ SÜRE BİTTİ! DEPREM BAŞLADI!");

        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = "DEPREM!";
        }

        // Tur geçtikçe artan sarsıntı şiddeti
        float currentMagnitude = baseEarthquakeMagnitude + (currentRound * magnitudeMultiplierPerRound);

        // 1. Kamerayı belirlenen süre boyunca sallamaya başlatıyoruz
        if (CameraShake.Instance != null)
        {
            yield return StartCoroutine(CameraShake.Instance.Shake(earthquakeDuration, currentMagnitude));
        }
        else
        {
            yield return new WaitForSeconds(earthquakeDuration);
        }

        // 2. Deprem sallantısı tamamlandı/bitmeye yakın KONTROL yapıyoruz:
        if (!isRoundCompletedSuccessfully)
        {
            // İtem'lar toplanamadıysa depremin sonunda ölüm gerçekleşiyor!
            if (timerText != null)
            {
                timerText.text = "ÖLDÜN!";
            }

            PlayerDeath();

            // Eğer oyuncu öldüyse yeni tura geçmek istemiyorsan burada 'yield break;' yapabilirsin.
            // Şimdilik test için yeni turu başlatmaya devam ediyor.
        }
        else
        {
            Debug.Log(">>> Harika! Süresinde saklandın ve depremi atlattın.");
        }

        Debug.Log("Deprem bitti. İşlem tamamlanıyor...");
        isEarthquakeActive = false;

        StartNewRound();
    }

    // İleride için dolduracağın ölüm fonksiyonu
    private void PlayerDeath()
    {
        Debug.Log(">>> DEBUG: Karakter depremde öldü! (İleride buraya ölüm kodlarını yazabilirsin)");

        // Örnek ileride eklenebilecekler:
        // Time.timeScale = 0f; // Oyunu dondur
        // GameOverUI.SetActive(true); // Game Over ekranını aç
    }
}