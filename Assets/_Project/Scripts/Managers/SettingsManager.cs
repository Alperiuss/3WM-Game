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
        // 1. MASTER VOLUME SLIDER
        if (masterVolumeSlider != null)
        {
            // Kayıtlı ses değerini al (Varsayılan 1f)
            float savedMasterVol = PlayerPrefs.GetFloat(AudioManager.MASTER_KEY, 1f);
            masterVolumeSlider.value = savedMasterVol;

            // Oyun ilk açıldığında kaydedilmiş sesi AudioManager'a haber ver
            AudioManager.OnVolumeChanged?.Invoke(AudioManager.MASTER_KEY, savedMasterVol);

            // Slider her kaydırıldığında event tetikle
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeSliderChanged);
        }

        // 2. FULLSCREEN TOGGLE
        if (fullscreenToggle != null)
        {
            bool isFullscreen = PlayerPrefs.GetInt("SavedFullscreen", 1) == 1;
            fullscreenToggle.isOn = isFullscreen;
            SetFullscreen(isFullscreen);

            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }
    }

    private void OnMasterVolumeSliderChanged(float value)
    {
        // Doğrudan metot çağırmak yerine Event'e (Listener) haber veriyoruz
        AudioManager.OnVolumeChanged?.Invoke(AudioManager.MASTER_KEY, value);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("SavedFullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
}