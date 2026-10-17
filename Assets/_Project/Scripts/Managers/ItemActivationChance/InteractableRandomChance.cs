using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class InteractableRandomChance : MonoBehaviour
{
    public static InteractableRandomChance Instance;

    [Header("Settings")]
    [SerializeField] private List<InteractableObject> allInteractable;
    [SerializeField] private int baseActiveCount = 3; // 1. gün başlangıç eşya sayısı

    [Header("Timer Settings")]
    [SerializeField] private float dayTimeLimit = 35f;
    private float currentTimer;
    private bool isTimerRunning = false;

    [Header("Earthquake Settings")]
    [SerializeField] private float earthquakeDuration = 5f;
    [SerializeField] private float baseEarthquakeMagnitude = 0.1f;
    [SerializeField] private float magnitudeMultiplierPerDay = 0.08f;

    [SerializeField] private int currentDay = 1; // 1. gün olarak net başlatıyoruz

    [Header("Stress Mechanics (Day 2+)")]
    [SerializeField] private Slider stressSlider;
    [SerializeField] private float maxStress = 100f;
    [SerializeField] private float baseStressFillRate = 3f;
    [SerializeField] private float noiseMakerPenaltyRate = 2f;

    private float currentStress = 0f;
    private bool isStressActive = false;
    private int activeNoiseMakersCount = 0;

    [Header("UI Settings")]
    [SerializeField] private GameObject survivalMessageUI;
    [SerializeField] private TextMeshProUGUI timerText;

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

        if (stressSlider != null)
        {
            stressSlider.maxValue = maxStress;
            stressSlider.gameObject.SetActive(false);
        }

        // DİKKAT: Burada otomatik StartDay() ÇAĞIRMIYORUZ! 
        // Günü başlatma yetkisini tamamen DayManager ve radyo akışına bıraktık.
    }

    private void Update()
    {
        if (!isTimerRunning) return;

        if (currentTimer > 0f)
        {
            currentTimer -= Time.deltaTime;

            if (isStressActive)
            {
                float currentFillRate = baseStressFillRate + (activeNoiseMakersCount * noiseMakerPenaltyRate);
                currentStress += currentFillRate * Time.deltaTime;
                currentStress = Mathf.Clamp(currentStress, 0f, maxStress);

                if (stressSlider != null)
                    stressSlider.value = currentStress;

                if (currentStress >= maxStress)
                {
                    currentStress = maxStress;
                    isTimerRunning = false;
                    StartCoroutine(StressOverloadDeathRoutine());
                    return;
                }
            }

            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(currentTimer / 60f);
                int seconds = Mathf.FloorToInt(currentTimer % 60f);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }

            if (currentTimer <= 0f)
            {
                currentTimer = 0f;
                isTimerRunning = false;
                StartCoroutine(TriggerEarthquakeRoutine());
            }
        }
    }

    // Günü resmi olarak başlatan ana metot
    public void StartNewDay()
    {
        if (isEarthquakeActive) return;

        if (survivalMessageUI != null)
            survivalMessageUI.SetActive(false);

        Debug.Log("--- GÜN " + currentDay + " BAŞLADI ---");

        if (currentDay > 4)
        {
            Debug.Log("TEBRİKLER! 4 GÜNÜ DE ATLATIP OYUNU KAZANDIN!");
            if (timerText != null) timerText.text = "GLORY!";
            return;
        }

        isDayCompletedSuccessfully = false;
        activeNoiseMakersCount = 0;
        currentStress = 0f;

        // Stres SADECE 2. gün ve sonrasında aktif olur (1. günden kesinlikle açılmaz)
        isStressActive = (currentDay >= 2);

        if (stressSlider != null)
        {
            stressSlider.gameObject.SetActive(isStressActive);
            stressSlider.value = 0f;
        }

        if (allInteractable == null || allInteractable.Count == 0)
        {
            Debug.LogWarning(">>> HATA: allInteractable listesi boş!");
            return;
        }

        if (timerText != null)
            timerText.gameObject.SetActive(true);

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

        // HATA ÇÖZÜMÜ: Eşya sayısını her gün sabit bir formülle artırıyoruz (Sürekli üstüne ekleme hatası bitti)
        int targetActiveCount = baseActiveCount + ((currentDay - 1) * 2);
        int countToActivate = Mathf.Min(targetActiveCount, shuffledList.Count);

        remainingActiveCount = countToActivate;

        List<string> currentDayTaskDescriptions = new List<string>();

        for (int i = 0; i < countToActivate; i++)
        {
            shuffledList[i].SetInteractableState(true);

            if (shuffledList[i].Data != null)
            {
                currentDayTaskDescriptions.Add(shuffledList[i].Data.taskDescription);

                if (shuffledList[i].Data.isNoiseMaker)
                {
                    activeNoiseMakersCount++;
                }
            }
        }

        if (TaskUIManager.Instance != null)
        {
            TaskUIManager.Instance.PopulateTasks(currentDayTaskDescriptions);
        }

        currentTimer = dayTimeLimit;
        isTimerRunning = true;
    }

    // Gün atlatma fonksiyonu (DayManager burayı çağıracak)
    public void AdvanceToNextDay()
    {
        currentDay++;
        StartNewDay();
    }

    public void NotifyObjectResolved(string taskDescription, InteractableData resolvedData)
    {
        if (isEarthquakeActive || !isTimerRunning) return;

        remainingActiveCount--;
        Debug.Log("Kalan aktif nesne: " + remainingActiveCount);

        if (resolvedData != null && resolvedData.isNoiseMaker)
        {
            activeNoiseMakersCount = Mathf.Max(0, activeNoiseMakersCount - 1);
            Debug.Log("Gürültülü eşya kapatıldı! Stres artış hızı yavaşlatıldı.");
        }

        if (TaskUIManager.Instance != null)
        {
            TaskUIManager.Instance.CompleteTask(taskDescription);
        }

        if (remainingActiveCount <= 0)
        {
            TriggerSurvivalPhase();
        }
    }

    private void TriggerSurvivalPhase()
    {
        isDayCompletedSuccessfully = true;
        Debug.Log("Bütün objeler zamanında tamamlandı: SAKLAN VE KURTUL! (Stres artmaya devam ediyor)");

        if (survivalMessageUI != null)
            survivalMessageUI.SetActive(true);
    }

    private IEnumerator StressOverloadDeathRoutine()
    {
        isEarthquakeActive = true;

        if (timerText != null)
        {
            timerText.text = "PANIC OVERLOAD!";
        }

        Debug.Log(">>> ÖLÜM: Stres seviyesi tavan yaptı!");
        yield return new WaitForSeconds(1.5f);

        PlayerDeath();
    }

    private IEnumerator TriggerEarthquakeRoutine()
    {
        isEarthquakeActive = true;
        isStressActive = false;

        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = "EARTHQUAKE!";
        }

        float currentMagnitude = baseEarthquakeMagnitude + (currentDay * magnitudeMultiplierPerDay);

        if (CameraShake.Instance != null)
        {
            yield return StartCoroutine(CameraShake.Instance.Shake(earthquakeDuration, currentMagnitude));
        }
        else
        {
            yield return new WaitForSeconds(earthquakeDuration);
        }

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

            if (DayManager.Instance != null)
            {
                DayManager.Instance.ShowDaySummary();
            }
        }
    }

    private void PlayerDeath()
    {
        Debug.Log(">>> GAME OVER: Ölüm ekranı açılıyor.");
    }
}