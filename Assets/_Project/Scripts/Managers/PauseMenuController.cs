using UnityEngine;
using UnityEngine.InputSystem; // New Input System kütüphanesi

public class PauseMenuController : MonoBehaviour
{
    [Header("UI Paneli")]
    [Tooltip("Açılıp kapanacak Settings veya Pause Panel objesi")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Oyun Zamanı Ayarı")]
    [Tooltip("Panel açıldığında oyun arkada dursun mu?")]
    [SerializeField] private bool pauseTimeWhenOpen = true;

    private bool isPaused = false;

    private void Start()
    {
        // Oyun başladığında panelin kapalı olduğundan emin olalım
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // New Input System ile klavyeden ESC tuşuna basılıp basılmadığını kontrol eder
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleSettingsMenu();
        }
    }

    /// <summary>
    /// Menüyü açıksa kapatır, kapalıysa açar.
    /// </summary>
    public void ToggleSettingsMenu()
    {
        isPaused = !isPaused;

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(isPaused);
        }

        // Zamanı durdur / devam ettir
        if (pauseTimeWhenOpen)
        {
            Time.timeScale = isPaused ? 0f : 1f;
        }

        // Fare imlecini ayarla (FPS / TPS oyunlarında imleç kilitliyse açar)
        if (isPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Eğer oyununuzda mouse kilitli oynanıyorsa burayı CursorLockMode.Locked yapabilirsiniz
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// Settings panelindeki "Kapat" veya "Devam Et" butonunun OnClick event'ine bağlayabilirsiniz.
    /// </summary>
    public void ResumeGame()
    {
        if (isPaused)
        {
            ToggleSettingsMenu();
        }
    }

    private void OnDestroy()
    {
        // Sahne değişirken oyun donuk kalmasın diye zamanı normale döndür
        Time.timeScale = 1f;
    }
}