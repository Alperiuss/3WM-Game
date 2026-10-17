using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Controls")]
    [SerializeField] private Slider masterVolumeSlider;

    [Header("Display Controls")]
    [SerializeField] private Toggle fullscreenToggle;

    private void Start()
    {
        // 1. Ses Ayarını Yükle ve Dinleyici Bağla
        if (masterVolumeSlider != null && AudioManager.Instance != null)
        {
            float savedMasterVol = AudioManager.Instance.GetGroupVolume(AudioManager.MASTER_KEY, 1f);
            masterVolumeSlider.value = savedMasterVol;
            
            // Başlangıç değerini Mixer'a uygula
            AudioManager.Instance.SetGroupVolume(AudioManager.MASTER_KEY, savedMasterVol);

            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        // 2. Tam Ekran Ayarını Yükle ve Dinleyici Bağla
        if (fullscreenToggle != null)
        {
            bool isFullscreen = PlayerPrefs.GetInt("SavedFullscreen", 1) == 1;
            fullscreenToggle.isOn = isFullscreen;
            SetFullscreen(isFullscreen);

            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }
    }

    private void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetGroupVolume(AudioManager.MASTER_KEY, value);
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("SavedFullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
}