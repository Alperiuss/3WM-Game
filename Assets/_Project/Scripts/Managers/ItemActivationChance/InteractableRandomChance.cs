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
        // Zamanlayıcı aktifse geriye doğru say (Bu sayaç deprem gelene kadarki süredir)
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

                // Süre bittiğinde (00:00 olduğunda) depremi tetikle
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
        isTimerRunning = false; // Süreyi durdur
        Debug.Log("Bütün objeler tamamlandı: SAKLAN VE KURTUL!");

        if (survivalMessageUI != null)
            survivalMessageUI.SetActive(true);

        if (timerText != null)
            timerText.gameObject.SetActive(false); // Süre sayacını gizle

        // İstersen burada başarıyla tamamlandığı için birkaç saniye bekleyip yeni tura geçebilirsin:
        // Invoke("StartNewRound", 4f);
    }

    private IEnumerator TriggerEarthquakeRoutine()
    {
        isEarthquakeActive = true;
        Debug.Log("⚠️ SÜRE BİTTİ! DEPREM BAŞLADI! Tur: " + currentRound);

        // Deprem başladığında sayaç alanına "DEPREM!" yazabiliriz veya gizleyebiliriz
        if (timerText != null)
        {
            timerText.text = "DEPREM!";
        }

        // Tur geçtikçe artan formül: Temel şiddet + (Tur Sayısı * Artış Oranı)
        float currentMagnitude = baseEarthquakeMagnitude + (currentRound * magnitudeMultiplierPerRound);
        Debug.Log("Deprem Şiddeti: " + currentMagnitude);

        if (CameraShake.Instance != null)
        {
            // Kamerayı deprem süresi ve hesaplanan şiddet kadar sars
            yield return StartCoroutine(CameraShake.Instance.Shake(earthquakeDuration, currentMagnitude));
        }
        else
        {
            yield return new WaitForSeconds(earthquakeDuration);
        }

        Debug.Log("Deprem bitti. Yeni tur başlatılıyor...");
        isEarthquakeActive = false;

        // Deprem bittikten sonra yeni turu başlat
        StartNewRound();
    }
}