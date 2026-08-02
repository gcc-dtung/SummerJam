using UnityEngine;

public class FlowManager : Singleton<FlowManager>
{
    [Header("Canvas")]
    [SerializeField] private Canvas canvasMainMenu;
    [SerializeField] private Canvas canvasGamePlay;
    [SerializeField] private CanvasTransition canvasTransition;

    [Header("Next Level Transition")]
    [Tooltip("Assign the manually configured NextLevelTransition overlay here.")]
    [SerializeField] private NextLevelTransition nextLevelTransition;

    [Header("Result Panels")]
    [SerializeField] private WinPanel winPanel;
    [SerializeField] private LosePanel losePanel;

    [Header("Always On")]
    [Tooltip("Objects that must stay active for the whole game flow, for example Canvas Animation.")]
    [SerializeField] private GameObject[] alwaysOnObjects;

    [Header("Startup UI Roots")]
    [SerializeField] private bool autoEnableKnownUiRoots = true;
    [SerializeField] private string[] mainMenuActiveRootPaths =
    {
        "Background",
        "HomePanel",
        "SettingPanel",
        "ShopPanel",
        "AlwayUp Panel"
    };

    [SerializeField] private string[] gameplayActiveRootPaths =
    {
        "HUDLayer",
        "HUDLayer/Up Panel",
        "HUDLayer/Down Panel",
        "HUDLayer/Score",
        "HUDLayer/Gold",
        "HUDLayer/Gem",
        "PopupLayer"
    };

    private bool isChangingFlow;

    protected override void Awake()
    {
        base.Awake();
        ApplyBootState();
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Start()
    {
        ApplyBootState();
    }

    public async void StartGame()
    {
        if (isChangingFlow)
            return;

        isChangingFlow = true;
        SetAlwaysOnObjectsActive();
        HideResultPanels();

        if (canvasTransition != null)
        {
            await canvasTransition.PlayAsync(() =>
            {
                ShowGameplayCanvasOnly();
                GameManager.Instance.UpdateGameState(GameState.SetUp);
            });
        }
        else
        {
            ShowGameplayCanvasOnly();
            GameManager.Instance.UpdateGameState(GameState.SetUp);
        }

        isChangingFlow = false;
        GameManager.Instance.UpdateGameState(GameState.GamePlay);
    }

    public void BackToMainMenu()
    {
        if (isChangingFlow)
            return;

        isChangingFlow = true;

        if (winPanel != null && winPanel.IsVisible)
        {
            winPanel.PlayOutro(BackToMainMenuAfterResultOutro);
            return;
        }

        BackToMainMenuAfterResultOutro();
    }

    private async void BackToMainMenuAfterResultOutro()
    {
        SoundManager.PlayBGMSound(BGMType.MainMenu); // co the xoa
        SetAlwaysOnObjectsActive();
        HideResultPanels();

        if (canvasTransition != null)
        {
            await canvasTransition.PlayAsync(ShowMainMenuCanvasOnly);
        }
        else
        {
            ShowMainMenuCanvasOnly();
        }
        
        isChangingFlow = false;
    }

    public async void NextLevel()
    {
        if (isChangingFlow)
            return;

        isChangingFlow = true;

        try
        {
            if (nextLevelTransition != null)
            {
                await nextLevelTransition.PlayAsync(LoadNextLevelWhileCovered);
            }
            else
            {
                LoadNextLevelWhileCovered();
            }

            GameManager.Instance.UpdateGameState(GameState.GamePlay);
        }
        finally
        {
            isChangingFlow = false;
        }
    }

    public async void ReplayCurrentLevel()
    {
        if (isChangingFlow)
            return;

        isChangingFlow = true;

        try
        {
            if (nextLevelTransition != null)
            {
                await nextLevelTransition.PlayAsync(ReplayCurrentLevelWhileCovered);
            }
            else
            {
                ReplayCurrentLevelWhileCovered();
            }
        }
        finally
        {
            isChangingFlow = false;
        }
    }

    public void ApplyBootState()
    {
        SetRequiredUiRootsActive();
        SetAlwaysOnObjectsActive();
        HideNextLevelTransition();
        ShowMainMenuCanvasOnly();
        HideResultPanels();
    }

    private void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Win:
                ShowWinFlow();
                break;
            case GameState.Lose:
                ShowLoseFlow();
                break;
            case GameState.GamePlay:
                ShowGameplayFlow();
                break;
        }
    }

    private void ShowWinFlow()
    {
        SetAlwaysOnObjectsActive();
        ShowGameplayCanvasOnly();
        losePanel?.HideImmediate();
        winPanel?.OnWin();
    }

    private void ShowLoseFlow()
    {
        SetAlwaysOnObjectsActive();
        ShowGameplayCanvasOnly();
        winPanel?.HideImmediate();
        losePanel?.OnLose();
    }

    private void ShowGameplayFlow()
    {
        SetAlwaysOnObjectsActive();
        ShowGameplayCanvasOnly();
        HideResultPanels();
    }

    private void ShowMainMenuCanvasOnly()
    {
        SetRequiredUiRootsActive();
        SetCanvasActive(canvasMainMenu, true);
        SetCanvasActive(canvasGamePlay, false);
    }

    private void ShowGameplayCanvasOnly()
    {
        SetRequiredUiRootsActive();
        SetCanvasActive(canvasMainMenu, false);
        SetCanvasActive(canvasGamePlay, true);
    }

    private void HideResultPanels()
    {
        winPanel?.HideImmediate();
        losePanel?.HideImmediate();
    }

    private void HideNextLevelTransition()
    {
        nextLevelTransition?.HideImmediate();
    }

    private void LoadNextLevelWhileCovered()
    {
        HideResultPanels();
        GameManager.Instance.UpdateGameState(GameState.SetUp);
    }

    private void ReplayCurrentLevelWhileCovered()
    {
        HideResultPanels();
        GameManager.Instance.UpdateGameState(GameState.Replay);
    }

    private void SetAlwaysOnObjectsActive()
    {
        if (alwaysOnObjects == null)
            return;

        foreach (GameObject target in alwaysOnObjects)
        {
            if (target != null)
                target.SetActive(true);
        }
    }

    private void SetRequiredUiRootsActive()
    {
        if (!autoEnableKnownUiRoots)
            return;

        SetChildPathsActive(canvasMainMenu, mainMenuActiveRootPaths);
        SetChildPathsActive(canvasGamePlay, gameplayActiveRootPaths);
    }

    private static void SetChildPathsActive(Canvas rootCanvas, string[] childPaths)
    {
        if (rootCanvas == null || childPaths == null)
            return;

        Transform canvasTransform = rootCanvas.transform;

        foreach (string childPath in childPaths)
        {
            if (string.IsNullOrWhiteSpace(childPath))
                continue;

            Transform target = canvasTransform.Find(childPath);
            if (target != null)
                target.gameObject.SetActive(true);
        }
    }

    private static void SetCanvasActive(Canvas targetCanvas, bool isActive)
    {
        if (targetCanvas != null)
            targetCanvas.gameObject.SetActive(isActive);
    }
}
