using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMainMenu : MonoBehaviour
{
    [Header("Sahne Ayarı")]
    [Tooltip("Ana menü sahnenizin tam adı (Build Settings'teki isimle aynı olmalı)")]
    [SerializeField] private string mainMenuSceneName = "MainMenu"; 

    /// <summary>
    /// Butonun OnClick event'ine bu metodu bağlayın.
    /// </summary>
    public void GoToMainMenu()
    {
        Debug.Log("[ReturnToMainMenu] PlayerPrefs sıfırlanıyor ve Ana Menüye dönülüyor...");

        // 1. Zamanı normale döndür (Summary'de dondurulmuştu)
        Time.timeScale = 1f;

        // 2. Fare imlecini serbest bırak
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 3. DayManager'ı temizle (Yeni oyun temiz başlasın)
        if (DayManager.Instance != null)
        {
            Destroy(DayManager.Instance.gameObject);
        }

        // =========================================================================
        // 4. PLAYERPREFS SIFIRLAMA (SES AYARLARINI KORUYARAK TÜM EŞYALARI SİLER)
        // =========================================================================
        ResetGameDataKeepAudio();

        // 5. Ana menü sahnesini yükle
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Ses ayarları (Master, Music, SFX) hariç tüm satın alımları ve oyun kayıtlarını siler.
    /// </summary>
    private void ResetGameDataKeepAudio()
    {
        // Ses ayarlarını geçici olarak hafızaya al
        float masterVol = PlayerPrefs.GetFloat(AudioManager.MASTER_KEY, 1f);
        float musicVol  = PlayerPrefs.GetFloat(AudioManager.MUSIC_KEY, 1f);
        float sfxVol    = PlayerPrefs.GetFloat(AudioManager.SFX_KEY, 1f);

        // Tüm PlayerPrefs'i sil
        PlayerPrefs.DeleteAll();

        // Ses ayarlarını geri yükle ve kaydet
        PlayerPrefs.SetFloat(AudioManager.MASTER_KEY, masterVol);
        PlayerPrefs.SetFloat(AudioManager.MUSIC_KEY, musicVol);
        PlayerPrefs.SetFloat(AudioManager.SFX_KEY, sfxVol);
        PlayerPrefs.Save();

        Debug.Log("<color=green>[PlayerPrefs]: Ses ayarları hariç tüm oyun ve eşya kayıtları temizlendi!</color>");
    }
}