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
    [Tooltip("1. günün süresi (saniye)")]
    [SerializeField] private float baseDayTimeLimit = 60f;
    [Tooltip("Her yeni güne eklenen birikimli süre (2. gün +10, 3. gün +20 ...). Marketteki Ev Geliştirmesi bundan ayrı eklenir.")]
    [SerializeField] private float extraSecondsPerDay = 10f;

    [Header("Audio")]
    [Tooltip("Deprem öncesi siren, geri sayımın son kaç saniyesinde başlasın")]
    [SerializeField] private float sirenLeadTime = 10f;
    [Range(0f, 1f)] [SerializeField] private float sirenVolume = 0.7f;
    [Range(0f, 1f)] [SerializeField] private float earthquakeVolume = 1f;

    [Header("Final Day")]
    [Tooltip("Son gün; başarılı geçilirse market yerine oyun bitiş (tebrik) ekranı açılır")]
    [SerializeField] private int finalDay = 4;
    [Tooltip("Son günün normal süresine eklenen ekstra süre (saniye)")]
    [SerializeField] private float finalDayBonusSeconds = 15f;
    [Tooltip("Son günde aktif olacak görev sayısı")]
    [SerializeField] private int finalDayTaskCount = 9;

    [Header("First Day Guide")]
    [Tooltip("1. günde bütün görev eşyalarının yeri ekranda işaretlensin (yeni oyuncular evi tanısın diye)")]
    [SerializeField] private bool markAllTasksOnFirstDay = true;
    private float currentTimer;
    private bool isTimerRunning = false;

    [Header("Instant Earthquake")]
    [Tooltip("Görevler bitince oyuncu sarı alanda bu kadar kalırsa deprem geri sayımı beklemeden başlar")]
    [SerializeField] private float coverConfirmTime = 0.5f;
    private float coverTimer = 0f;

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

    [Header("Safe Zone Messages (survival phase)")]
    [SerializeField] private string takeCoverMessage = "ALL TASKS DONE! GET INTO A YELLOW SAFE ZONE!";
    [SerializeField] private string inCoverMessage = "YOU ARE IN COVER. STAY THERE!";
    private TMP_Text survivalMessageText;

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
        {
            survivalMessageText = survivalMessageUI.GetComponentInChildren<TMP_Text>(true);
            survivalMessageUI.SetActive(false);
        }

        if (stressSlider != null)
        {
            stressSlider.maxValue = maxStress;
            stressSlider.gameObject.SetActive(false);
        }

        // Yeni oyun her zaman başlangıç parasıyla ve boş marketle başlar
        if (currentDay <= 1)
        {
            ResetRunForNewGame();
        }

        // DİKKAT: Burada otomatik StartDay() ÇAĞIRMIYORUZ!
        // Günü başlatma yetkisini tamamen DayManager ve radyo akışına bıraktık.
    }

    private void Update()
    {
        if (!isTimerRunning) return;

        // Görevler bitti: oyuncu sarı alana girdiği an deprem geri sayımı beklemeden başlar
        if (isDayCompletedSuccessfully && UpdateCoverCheck())
        {
            BeginEarthquake();
            return;
        }

        // Deprem yaklaşırken siren: son saniyelerde veya görevler bitip sığınma aşaması başlayınca
        if (!GameSfx.IsLooping(GameSfx.Siren) && (currentTimer <= sirenLeadTime || isDayCompletedSuccessfully))
        {
            GameSfx.PlayLoop(GameSfx.Siren, GameSfx.Siren, sirenVolume, 1.2f);
        }

        if (currentTimer > 0f)
        {
            currentTimer -= Time.deltaTime;

            if (isStressActive)
            {
                float currentFillRate = (baseStressFillRate + (activeNoiseMakersCount * noiseMakerPenaltyRate))
                                        * ItemEffectsManager.StressRateMultiplier; // Stres İlacı: %10 yavaş
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
                BeginEarthquake();
            }
        }
    }

    /// <summary>
    /// Güvenli alan mesajını günceller; oyuncu yeterince uzun süre sarı alandaysa true döner.
    /// Sahnede SafeZoneManager yoksa görevler biter bitmez true döner.
    /// </summary>
    private bool UpdateCoverCheck()
    {
        bool inCover = SafeZoneManager.Instance == null || SafeZoneManager.Instance.IsPlayerInsideAnyZone();

        if (survivalMessageText != null && SafeZoneManager.Instance != null)
        {
            string wanted = inCover ? inCoverMessage : takeCoverMessage;
            if (survivalMessageText.text != wanted) survivalMessageText.text = wanted;
        }

        coverTimer = inCover ? coverTimer + Time.deltaTime : 0f;
        return inCover && coverTimer >= coverConfirmTime;
    }

    private void BeginEarthquake()
    {
        if (isEarthquakeActive) return;

        isTimerRunning = false;
        StartCoroutine(TriggerEarthquakeRoutine());
    }

    /// <summary>Günün süresi: temel süre + her gün birikimli ek süre + Ev Geliştirmesi bonusu.</summary>
    private float GetDayTimeLimit(int day)
    {
        float time = baseDayTimeLimit + Mathf.Max(0, day - 1) * extraSecondsPerDay + ItemEffectsManager.BonusDaySeconds;
        if (day >= finalDay) time += finalDayBonusSeconds;
        return time;
    }

    private void ResetRunForNewGame()
    {
        GameOverScreen.ResetRunProgress();
        if (CurrencyManager.Instance != null) CurrencyManager.Instance.ResetToStartingFame();
        if (ItemEffectsManager.Instance != null) ItemEffectsManager.Instance.Refresh();
    }

    // Günü resmi olarak başlatan ana metot
    public void StartNewDay()
    {
        if (isEarthquakeActive)
        {
            Debug.LogWarning("[InteractableRandomChance] Deprem sürerken yeni gün başlatılamaz.");
            return;
        }

        if (survivalMessageUI != null)
            survivalMessageUI.SetActive(false);

        if (SafeZoneManager.Instance != null)
            SafeZoneManager.Instance.HideZones();

        ItemLocatorMarker.Hide();
        GameSfx.StopAllLoops();
        coverTimer = 0f;

        Debug.Log("--- GÜN " + currentDay + " BAŞLADI ---");

        if (currentDay > finalDay)
        {
            // Normal akışta buraya gelinmez: son gün kazanılınca bitiş ekranı açılır
            ShowVictory();
            return;
        }

        // Önceki günden kalan her durumu sıfırla
        isDayCompletedSuccessfully = false;
        activeNoiseMakersCount = 0;
        currentStress = 0f;
        currentTimer = 0f;

        // Stres SADECE 2. gün ve sonrasında aktif olur (1. günden kesinlikle açılmaz)
        isStressActive = (currentDay >= 2);

        if (stressSlider != null)
        {
            stressSlider.maxValue = maxStress;
            stressSlider.value = 0f;
            stressSlider.gameObject.SetActive(isStressActive);
        }

        // Inspector listesinde boş (silinmiş) referanslar olabilir; onları ayıkla
        List<InteractableObject> shuffledList = new List<InteractableObject>();
        if (allInteractable != null)
        {
            foreach (var obj in allInteractable)
            {
                if (obj != null) shuffledList.Add(obj);
            }
        }

        if (shuffledList.Count == 0)
        {
            Debug.LogWarning(">>> HATA: allInteractable listesi boş!");
            return;
        }

        if (timerText != null)
            timerText.gameObject.SetActive(true);

        foreach (var obj in shuffledList)
        {
            obj.SetInteractableState(false);
        }

        for (int i = 0; i < shuffledList.Count; i++)
        {
            InteractableObject temp = shuffledList[i];
            int randomIndex = Random.Range(i, shuffledList.Count);
            shuffledList[i] = shuffledList[randomIndex];
            shuffledList[randomIndex] = temp;
        }

        // HATA ÇÖZÜMÜ: Eşya sayısını her gün sabit bir formülle artırıyoruz (Sürekli üstüne ekleme hatası bitti)
        int targetActiveCount = currentDay >= finalDay ? finalDayTaskCount : baseActiveCount + ((currentDay - 1) * 2);
        if (targetActiveCount > shuffledList.Count)
            Debug.LogWarning($"[InteractableRandomChance] Gün {currentDay} için {targetActiveCount} görev istendi ama sahnede {shuffledList.Count} eşya var.");
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

        TMP_FontAsset markerFont = timerText != null ? timerText.font : null;
        if (currentDay == 1 && markAllTasksOnFirstDay)
        {
            // 1. gün rehberi: yeni oyuncular evi tanımadığı için bütün görev eşyaları işaretlenir
            for (int i = 0; i < countToActivate; i++) ItemLocatorMarker.Show(shuffledList[i], markerFont);
        }
        else if (ItemEffectsManager.AlarmActive && countToActivate > 0)
        {
            // Alarm: bugünün tehlikelerinden birinin yeri ekranda işaretlenir
            ItemLocatorMarker.Show(shuffledList[Random.Range(0, countToActivate)], markerFont);
        }

        currentTimer = GetDayTimeLimit(currentDay);
        isTimerRunning = true;
    }

    /// <summary>
    /// Marketten çıkınca gün numarasını artırır. Gün burada BAŞLAMAZ: DayManager radyo yayını bittikten sonra
    /// StartNewDay'i bir kez çağırır (önceden gün radyo sırasında da başladığı için sayaç ve stres arkada akıyordu).
    /// </summary>
    public void AdvanceToNextDay()
    {
        currentDay++;
        isTimerRunning = false;
        isStressActive = false;
    }

    public bool IsFinalDay => currentDay >= finalDay;

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

        if (survivalMessageText != null)
            survivalMessageText.text = takeCoverMessage;

        // Hayat üçgeni: 2 adet sarı güvenli alan belirir
        if (SafeZoneManager.Instance != null)
            SafeZoneManager.Instance.ShowZones();
    }

    private IEnumerator StressOverloadDeathRoutine()
    {
        isEarthquakeActive = true;

        if (timerText != null)
        {
            timerText.text = "PANIC OVERLOAD!";
        }

        Debug.Log(">>> ÖLÜM: Stres seviyesi tavan yaptı!");
        if (DayManager.Instance != null) DayManager.Instance.SetPlayerControl(false);
        yield return new WaitForSeconds(1.5f);

        PlayerDeath(DeathReason.Stress);
    }

    private IEnumerator TriggerEarthquakeRoutine()
    {
        isEarthquakeActive = true;
        isStressActive = false;
        ItemLocatorMarker.Hide();
        GameSfx.StopLoop(GameSfx.Siren, 0.8f);
        GameSfx.PlayLoop(GameSfx.Earthquake, GameSfx.Earthquake, earthquakeVolume, 0.25f);

        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = "EARTHQUAKE!";
        }

        float currentMagnitude = baseEarthquakeMagnitude + (currentDay * magnitudeMultiplierPerDay);

        // Geri sayım bitti: görevler tamam VE oyuncu sarı alanın içindeyse kurtulur.
        // (Sahnede SafeZoneManager yoksa eski kural geçerli: görevler bittiyse kurtulur.)
        bool inCover = SafeZoneManager.Instance == null || SafeZoneManager.Instance.IsPlayerInsideAnyZone();
        bool survives = isDayCompletedSuccessfully && inCover;

        // Deprem anında çök-kapan-tutun: oyuncu yerinde kalır
        if (DayManager.Instance != null) DayManager.Instance.SetPlayerControl(false);
        if (survivalMessageUI != null) survivalMessageUI.SetActive(false);

        if (CameraShake.Instance != null)
        {
            yield return StartCoroutine(CameraShake.Instance.Shake(earthquakeDuration, currentMagnitude));
        }
        else
        {
            yield return new WaitForSeconds(earthquakeDuration);
        }

        GameSfx.StopLoop(GameSfx.Earthquake, 1.2f);
        SilenceAllNoise();

        if (SafeZoneManager.Instance != null)
            SafeZoneManager.Instance.HideZones();

        if (!survives)
        {
            PlayerDeath(DeathReason.NoCover);
        }
        else
        {
            Debug.Log(">>> Harika! Gün " + currentDay + " başarıyla atlatıldı.");
            isEarthquakeActive = false;
            yield return new WaitForSeconds(1.5f);

            if (IsFinalDay)
            {
                // Son gün: market yok, oyun bitiş (tebrik) ekranı
                ShowVictory();
            }
            else if (DayManager.Instance != null)
            {
                DayManager.Instance.ShowDaySummary();
            }
        }
    }

    private void ShowVictory()
    {
        Debug.Log(">>> TEBRİKLER! Bütün günler atlatıldı, oyun bitti.");
        isTimerRunning = false;
        isStressActive = false;
        ItemLocatorMarker.Hide();
        GameSfx.StopAllLoops(0.6f);
        SilenceAllNoise();

        if (survivalMessageUI != null) survivalMessageUI.SetActive(false);
        if (stressSlider != null) stressSlider.gameObject.SetActive(false);
        if (DayManager.Instance != null) DayManager.Instance.SetPlayerControl(false);

        if (GameOverScreen.Instance != null)
        {
            GameOverScreen.Instance.ShowVictory(Mathf.Min(currentDay, finalDay));
        }
        else
        {
            Debug.LogWarning("[InteractableRandomChance] Sahnede GameOverScreen yok, bitiş ekranı gösterilemedi.");
            if (timerText != null) timerText.text = "GLORY!";
        }
    }

    private void SilenceAllNoise()
    {
        if (allInteractable == null) return;
        foreach (var obj in allInteractable)
        {
            if (obj != null) obj.SilenceNoise();
        }
    }

    private void PlayerDeath(DeathReason reason)
    {
        Debug.Log(">>> GAME OVER (" + reason + "): Ölüm ekranı açılıyor.");
        isTimerRunning = false;
        isStressActive = false;
        ItemLocatorMarker.Hide();
        GameSfx.StopAllLoops(0.6f);
        SilenceAllNoise();

        if (SafeZoneManager.Instance != null)
            SafeZoneManager.Instance.HideZones();
        if (survivalMessageUI != null)
            survivalMessageUI.SetActive(false);
        if (DayManager.Instance != null)
            DayManager.Instance.SetPlayerControl(false);

        if (GameOverScreen.Instance != null)
        {
            GameOverScreen.Instance.Show(reason, !isDayCompletedSuccessfully, currentDay);
        }
        else
        {
            Debug.LogWarning("[InteractableRandomChance] Sahnede GameOverScreen yok, ölüm ekranı gösterilemedi.");
        }
    }
}