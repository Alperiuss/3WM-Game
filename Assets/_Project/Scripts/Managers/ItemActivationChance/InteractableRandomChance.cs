using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class InteractableRandomChance : MonoBehaviour
{
    public static InteractableRandomChance Instance;

    [Header("Settings")]
    [SerializeField] private List<InteractableObject> allInteractable;
    [SerializeField] private int baseActiveCount = 3; // 1. gün kaç eşya aktif olsun?

    [Header("Timer Settings")]
    [SerializeField] private float dayTimeLimit = 35f; // Günlük süre sınırı
    private float currentTimer;
    private bool isTimerRunning = false;

    [Header("Earthquake Settings")]
    [SerializeField] private float earthquakeDuration = 5f;          // Deprem süresi
    [SerializeField] private float baseEarthquakeMagnitude = 0.1f;   // Temel sarsıntı şiddeti
    [SerializeField] private float magnitudeMultiplierPerDay = 0.08f; // Gün başına artan şiddet çarpanı
    private int currentDay = 0;

    [Header("UI Settings")]
    [SerializeField] private GameObject survivalMessageUI; // "Saklan ve Kurtul!" yazısı
    [SerializeField] private TextMeshProUGUI timerText;    // Sayaç ve durum metni

    private int remainingActiveCount = 0;
    private bool isEarthquakeActive = false;
    private bool isDayCompletedSuccessfully = false;
    public int CurrentDay => currentDay;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (survivalMessageUI != null)
            survivalMessageUI.SetActive(false);

        StartNewDay();
    }

    private void Update()
    {
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

                // Süre bittiğinde HER KOŞULDA deprem başlar
                if (currentTimer <= 0f)
                {
                    currentTimer = 0f;
                    isTimerRunning = false;

                    StartCoroutine(TriggerEarthquakeRoutine());
                }
            }
        }
    }

    public void StartNewDay()
    {
        if (isEarthquakeActive) return;

        if (survivalMessageUI != null)
            survivalMessageUI.SetActive(false);

        currentDay++;
        Debug.Log("--- GÜN " + currentDay + " BAŞLADI ---");

        if (currentDay > 4)
        {
            Debug.Log("TEBRİKLER! 4 GÜNÜ DE ATLATIP OYUNU KAZANDIN!");
            if (timerText != null) timerText.text = "GLORY!";
            return;
        }

        isDayCompletedSuccessfully = false;

        if (allInteractable == null || allInteractable.Count == 0)
        {
            Debug.LogWarning(">>> HATA: allInteractable listesi boş!");
            return;
        }

        if (survivalMessageUI != null)
            survivalMessageUI.SetActive(false);

        if (timerText != null)
            timerText.gameObject.SetActive(true);

        // 1. ÖNEMLİ: Yeni gün başlarken önce TÜM eşyaları pasif yap (Hepsini sıfırla)
        foreach (var obj in allInteractable)
        {
            obj.SetInteractableState(false);
        }

        // 2. Eşyaları karıştır ve bu gün için sadece belirlenen sayıda eşyayı aktif et
        List<InteractableObject> shuffledList = new List<InteractableObject>(allInteractable);
        for (int i = 0; i < shuffledList.Count; i++)
        {
            InteractableObject temp = shuffledList[i];
            int randomIndex = Random.Range(i, shuffledList.Count);
            shuffledList[i] = shuffledList[randomIndex];
            shuffledList[randomIndex] = temp;
        }

        // Gün ilerledikçe aktif olacak eşya sayısı artar (1. gün 3, 2. gün 4 vb.)
        int countToActivate = Mathf.Min(baseActiveCount, shuffledList.Count);
        baseActiveCount += 2;
        remainingActiveCount = countToActivate;

        for (int i = 0; i < countToActivate; i++)
        {
            shuffledList[i].SetInteractableState(true);
        }

        currentTimer = dayTimeLimit;
        isTimerRunning = true;
    }

    public void NotifyObjectResolved()
    {
        if (isEarthquakeActive || !isTimerRunning) return;

        remainingActiveCount--;
        Debug.Log("Kalan aktif nesne: " + remainingActiveCount);

        if (remainingActiveCount <= 0)
        {
            TriggerSurvivalPhase();
        }
    }

    private void TriggerSurvivalPhase()
    {
        isDayCompletedSuccessfully = true;
        Debug.Log("Bütün objeler zamanında tamamlandı: SAKLAN VE KURTUL!");

        if (survivalMessageUI != null)
            survivalMessageUI.SetActive(true);
    }

    private IEnumerator TriggerEarthquakeRoutine()
    {
        isEarthquakeActive = true;

        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = "EARTHQUAKE!";
        }

        // Gün ilerledikçe deprem şiddeti katlanarak artar
        float currentMagnitude = baseEarthquakeMagnitude + (currentDay * magnitudeMultiplierPerDay);

        // Kamerayı deprem süresi boyunca sars
        if (CameraShake.Instance != null)
        {
            yield return StartCoroutine(CameraShake.Instance.Shake(earthquakeDuration, currentMagnitude));
        }
        else
        {
            yield return new WaitForSeconds(earthquakeDuration);
        }

        // Deprem biterken / bittiğinde kontrol: Görev zamanında yapıldı mı?
        if (!isDayCompletedSuccessfully)
        {
            if (timerText != null)
            {
                timerText.text = "ÖLDÜN!";
            }
            PlayerDeath();
        }
        else
        {
            Debug.Log(">>> Harika! Gün " + currentDay + " başarıyla atlatıldı.");
            isEarthquakeActive = false;
            yield return new WaitForSeconds(1.5f);

            // Doğrudan yeni güne geçmek yerine önce Özet Paneli açılır:
            if (DayManager.Instance != null)
            {
                DayManager.Instance.ShowDaySummary();
            }
        }
    }

    private void PlayerDeath()
    {
        Debug.Log(">>> ÖLÜM: Karakter depremde eşyaları yetiştiremediği için öldü!");
        // Örnek: Time.timeScale = 0f; GameOverPanel.SetActive(true);
    }
}