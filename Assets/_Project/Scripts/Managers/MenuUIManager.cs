using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuUIManager : MonoBehaviour
{
    public static MenuUIManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    [Header("Settings Panel Buttons")]
    [SerializeField] private Button settingsBackButton;

    [Header("Game Flow References")]
    [Tooltip("Oyun başladığında aktif olacak Oyun İçi UI / HUD (Varsa)")]
    [SerializeField] private GameObject gameHUDCanvas;

    // Runtime States
    public bool IsGameStarted { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitializeButtons();
        ShowMainMenu();
    }

    private void InitializeButtons()
    {
        if (playButton != null) playButton.onClick.AddListener(OnPlayButtonClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitButtonClicked);
        if (settingsBackButton != null) settingsBackButton.onClick.AddListener(OnSettingsBackClicked);
    }

    #region Button Actions

    // OYUNA BAŞLAMA
    private void OnPlayButtonClicked()
    {
        IsGameStarted = true;
        Debug.Log("[MenuUI]: 1. Gün Başladı!");

        // Menü panellerini kapat
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Oyun içi arayüzü aç
        if (gameHUDCanvas != null) gameHUDCanvas.SetActive(true);
    }

    // AYARLAR
    private void OnSettingsButtonClicked()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    // AYARLARDAN GERİ DÖNÜŞ
    private void OnSettingsBackClicked()
    {
        ShowMainMenu();
    }

    // ÇIKIŞ
    private void OnExitButtonClicked()
    {
        Debug.Log("[MenuUI]: Oyundan çıkılıyor...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    #endregion

    #region Public Helper Methods

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    #endregion
}