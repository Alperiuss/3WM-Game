using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Wave State")]
    [SerializeField] private int currentWave = 1; // Başlangıç dalgası
    [SerializeField] private int maxWave = 5;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Sahne değişse de GameManager silinmez
        }
        else
        {
            Destroy(gameObject);
        }

        // İstersen mevcut wave durumunu kaydedip yükleyebilirsin
        LoadGameState();
    }

    // Continue butonuna basıldığında çağrılır
    public void OnShopCompleted()
    {
        Debug.Log($"[GameManager]: Mağaza kapatıldı. Wave {currentWave} radyo anonsu başlatılıyor...");

        // 1. Aktif wave'in radyo anonsunu çalıştır
        if (WaveRadioSystem.Instance != null)
        {
            WaveRadioSystem.Instance.OnWaveCompleted(currentWave);
        }

        // 2. İleride buraya yeni Wave'i başlatacak olan oyun içi mekanikleri ekleyebilirsin
        // StartNextWaveGameplay();
    }

    // Dalga/Bölüm tamamlandığında çağıracağın metot
    public void CompleteCurrentWave()
    {
        if (currentWave < maxWave)
        {
            currentWave++;
            SaveGameState();
            Debug.Log($"[GameManager]: Wave tamamlandı! Yeni Wave: {currentWave}");
        }
        else
        {
            Debug.Log("[GameManager]: Tüm wave'ler tamamlandı! Oyun bitti.");
        }
    }

    // Dışarıdan mevcut wave sayısını okumak istersen (Getter)
    public int GetCurrentWave() => currentWave;

    private void SaveGameState()
    {
        PlayerPrefs.SetInt("Current_Wave", currentWave);
        PlayerPrefs.Save();
    }

    private void LoadGameState()
    {
        currentWave = PlayerPrefs.GetInt("Current_Wave", 1);
    }
}