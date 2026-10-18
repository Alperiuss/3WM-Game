using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.Cinemachine;
using StarterAssets;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    [Header("Cameras")]
    [SerializeField] private CinemachineCamera radioCamera;
    [SerializeField] private CinemachineCamera playerCamera;

    [Header("UI Canvases")]
    [SerializeField] private GameObject radioCanvas;
    [SerializeField] private GameObject playerCanvas;
    [SerializeField] private GameObject summaryPanel;
    [SerializeField] private GameObject marketPanel;

    [Header("Player Spawn & Movement")]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private MonoBehaviour playerMovementScript;

    private GameObject playerObject;
    private CharacterController playerCharacterController;
    private PlayerInput playerInput;
    private StarterAssetsInputs playerInputs;
    private FirstPersonController firstPersonController;
    private bool isRadioFinished = false;
    private bool isFirstGameLaunch = true;
    private bool playerMotorInitialized = false;
    private bool isTransitioningToMarket;
    private Coroutine gameSequenceRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetupRadioPresentation();
        gameSequenceRoutine = StartCoroutine(GameSequenceRoutine(true));
    }

    private void SetupRadioPresentation()
    {
        if (radioCamera != null) radioCamera.Priority = 20;
        if (playerCamera != null) playerCamera.Priority = 10;

        if (radioCanvas != null) radioCanvas.SetActive(true);
        if (playerCanvas != null)
        {
            playerCanvas.SetActive(true);
            CanvasGroup group = playerCanvas.GetComponent<CanvasGroup>();
            if (group != null) group.alpha = 0f;
        }
        if (summaryPanel != null) summaryPanel.SetActive(false);
        if (marketPanel != null) marketPanel.SetActive(false);

        SetCursorState(false);
    }

    private IEnumerator GameSequenceRoutine(bool isStartingGame = false)
    {
        yield return StartCoroutine(InitializePlayerMotorThenLock());

        isRadioFinished = false;

        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.FadeTransition(
                onHalfWay: () =>
                {
                    Time.timeScale = 1f;
                },
                onComplete: StartRadioBroadcast
            );
        }
        else
        {
            StartRadioBroadcast();
        }

        yield return new WaitUntil(() => isRadioFinished);

        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.FadeTransition(
                onHalfWay: SwitchToPlayerAndSpawn,
                onComplete: () => CompletePlayerHandoff(isStartingGame)
            );
        }
        else
        {
            SwitchToPlayerAndSpawn();
            CompletePlayerHandoff(isStartingGame);
        }
    }

    private IEnumerator InitializePlayerMotorThenLock()
    {
        CachePlayerReferences();

        if (playerMovementScript != null && !playerMovementScript.enabled)
            playerMovementScript.enabled = true;

        if (playerCharacterController != null)
            playerCharacterController.enabled = true;

        yield return null;

        playerMotorInitialized = true;
        SetPlayerMovement(false);
    }

    private void StartRadioBroadcast()
    {
        int currentDayToPlay = 1;
        if (InteractableRandomChance.Instance != null)
            currentDayToPlay = Mathf.Max(1, InteractableRandomChance.Instance.CurrentDay);

        if (WaveRadioSystem.Instance != null)
            WaveRadioSystem.Instance.OnWaveCompleted(currentDayToPlay);
        else
            OnRadioFinished();
    }

    private void SwitchToPlayerAndSpawn()
    {
        SetCursorState(false);

        if (radioCamera != null) radioCamera.Priority = 10;
        if (playerCamera != null) playerCamera.Priority = 20;

        if (radioCanvas != null) radioCanvas.SetActive(false);

        if (playerCanvas != null)
        {
            playerCanvas.SetActive(true);
            CanvasGroup group = playerCanvas.GetComponent<CanvasGroup>();
            if (group != null) group.alpha = 0f;
        }

        ResetPlayerPosition();
    }

    private void CompletePlayerHandoff(bool isStartingGame)
    {
        Time.timeScale = 1f;
        StartCoroutine(FadeInPlayerCanvasRoutine());
        SetPlayerMovement(true);

        if (InteractableRandomChance.Instance == null)
            return;

        if (isStartingGame && isFirstGameLaunch)
        {
            InteractableRandomChance.Instance.StartNewDay();
            isFirstGameLaunch = false;
        }
        else if (!isStartingGame)
        {
            InteractableRandomChance.Instance.StartNewDay();
        }
    }

    private void CachePlayerReferences()
    {
        if (playerObject != null)
            return;

        if (playerMovementScript != null)
            playerObject = playerMovementScript.gameObject;
        else
            playerObject = GameObject.FindWithTag("Player");

        if (playerObject == null)
            return;

        playerCharacterController = playerObject.GetComponent<CharacterController>();
        playerInput = playerObject.GetComponent<PlayerInput>();
        playerInputs = playerObject.GetComponent<StarterAssetsInputs>();
        firstPersonController = playerObject.GetComponent<FirstPersonController>();

        if (playerMovementScript == null && firstPersonController != null)
            playerMovementScript = firstPersonController;
    }

    private void ResetPlayerPosition()
    {
        CachePlayerReferences();

        if (playerSpawnPoint == null || playerObject == null)
            return;

        if (firstPersonController != null)
        {
            firstPersonController.SnapToSpawn(playerSpawnPoint.position, playerSpawnPoint.rotation);
            return;
        }

        bool wasEnabled = playerCharacterController != null && playerCharacterController.enabled;
        if (playerCharacterController != null)
            playerCharacterController.enabled = false;

        playerObject.transform.SetPositionAndRotation(playerSpawnPoint.position, playerSpawnPoint.rotation);
        Physics.SyncTransforms();

        if (playerCharacterController != null)
            playerCharacterController.enabled = wasEnabled;
    }

    private void SetPlayerMovement(bool canMove)
    {
        CachePlayerReferences();

        if (playerInputs != null)
        {
            playerInputs.MoveInput(Vector2.zero);
            playerInputs.LookInput(Vector2.zero);
            playerInputs.JumpInput(false);
            playerInputs.SprintInput(false);
            playerInputs.cursorInputForLook = canMove;
            playerInputs.cursorLocked = canMove;
        }

        if (playerInput != null)
        {
            playerInput.enabled = canMove;
            if (canMove)
                playerInput.ActivateInput();
            else
                playerInput.DeactivateInput();
        }

        if (playerMovementScript != null && playerMotorInitialized)
            playerMovementScript.enabled = canMove;

        if (canMove && playerCharacterController != null)
            playerCharacterController.enabled = true;
    }

    // Player Canvas'ın gecikmeli ve yumuşak bir şekilde netleşmesini sağlayan rutin
    private IEnumerator FadeInPlayerCanvasRoutine()
    {
        if (playerCanvas == null) yield break;

        CanvasGroup group = playerCanvas.GetComponent<CanvasGroup>();
        if (group == null) yield break;

        // Ekran açılışının oturması için çok kısa bir gecikme (örneğin 0.3 saniye)
        yield return new WaitForSeconds(0.3f);

        float duration = 0.6f; // Netleşme süresi
        float elapsed = 0f;
        group.alpha = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }

        group.alpha = 1f;
    }

    public void OnRadioFinished()
    {
        isRadioFinished = true;
    }

        public void ShowDaySummary()
    {
        SetPlayerMovement(false);

        if (ScreenFader.Instance == null)
        {
            OpenSummaryScreen();
            return;
        }

        ScreenFader.Instance.FadeTransition(
            onHalfWay: OpenSummaryScreen
        );
    }

    private void OpenSummaryScreen()
    {
        if (playerCanvas != null)
        {
            CanvasGroup group = playerCanvas.GetComponent<CanvasGroup>();
            if (group != null) group.alpha = 0f;
            playerCanvas.SetActive(false);
        }

        if (radioCanvas != null)
            radioCanvas.SetActive(false);

        if (marketPanel != null)
            marketPanel.SetActive(false);

        if (summaryPanel != null)
            summaryPanel.SetActive(true);

        if (SummaryPanelManager.Instance != null)
        {
            if (InteractableRandomChance.Instance != null)
                SummaryPanelManager.Instance.currentRound = InteractableRandomChance.Instance.CurrentDay;

            SummaryPanelManager.Instance.ShowSummary();
        }

        Time.timeScale = 0f;
        SetCursorState(true);
    }

    public void OnSummaryContinuePressed()
    {
        if (isTransitioningToMarket)
            return;

        isTransitioningToMarket = true;

        if (ScreenFader.Instance == null)
        {
            OpenMarketScreen();
            return;
        }

        ScreenFader.Instance.FadeTransition(
            onHalfWay: OpenMarketScreen,
            onComplete: () => isTransitioningToMarket = false
        );
    }

    private void OpenMarketScreen()
    {
        if (summaryPanel != null)
            summaryPanel.SetActive(false);

        if (SummaryPanelManager.Instance != null)
            SummaryPanelManager.Instance.ClosePanels(false);

        if (marketPanel != null)
            marketPanel.SetActive(true);

        Time.timeScale = 0f;
        SetCursorState(true);
    }

    public void OnMarketContinuePressed()
    {
        isTransitioningToMarket = false;
        SetupRadioPresentation();
        SetPlayerMovement(false);

        if (marketPanel != null)
            marketPanel.SetActive(false);

        if (summaryPanel != null)
            summaryPanel.SetActive(false);

        if (SummaryPanelManager.Instance != null)
        {
            SummaryPanelManager.Instance.ClosePanels(false);
            SummaryPanelManager.Instance.PrepareForNewRound();
        }

        if (InteractableRandomChance.Instance != null)
            InteractableRandomChance.Instance.AdvanceToNextDay();

        if (ItemEffectsManager.Instance != null)
            ItemEffectsManager.Instance.LoadAllItemEffects();

        Time.timeScale = 1f;

        if (gameSequenceRoutine != null)
            StopCoroutine(gameSequenceRoutine);

        gameSequenceRoutine = StartCoroutine(GameSequenceRoutine(false));
    }

    private void SetCursorState(bool isVisible)
    {
        if (isVisible)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}