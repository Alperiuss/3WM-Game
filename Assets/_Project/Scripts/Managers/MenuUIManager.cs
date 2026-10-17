using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // SAHNE GEÇİŞLERİ İÇİN GEREKLİ KÜTÜPHANE

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

    [Header("Scene Settings")]
    [Tooltip("Play butonuna basıldığında açılacak sahnenin tam adı")]
    [SerializeField] private string targetSceneName = "MainScene"; 

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

    // PLAY BUTONUNA BASILDIĞINDA SAHNE DEĞİŞTİRİR
    private void OnPlayButtonClicked()
    {
        IsGameStarted = true;
        Debug.Log($"[MenuUI]: {targetSceneName} sahnesine geçiliyor...");

        // Belirtilen sahneyi yükler
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError("[MenuUI]: Hedef sahne adı (Target Scene Name) boş bırakılmış!");
        }
    }

    private void OnSettingsButtonClicked()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    private void OnSettingsBackClicked()
    {
        ShowMainMenu();
    }

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