using UnityEngine;

public class FlowManager : Singleton<FlowManager>
{
    [Header("Canvas")]
    [SerializeField] private Canvas canvasMainMenu;
    [SerializeField] private Canvas canvasGamePlay;
    [SerializeField] private CanvasTransition canvasTransition;

    [Header("Result Panels")]
    [SerializeField] private WinPanel winPanel;
    [SerializeField] private LosePanel losePanel;

    [Header("Always On")]
    [Tooltip("Objects that must stay active for the whole game flow, for example Canvas Animation.")]
    [SerializeField] private GameObject[] alwaysOnObjects;

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

    public async void BackToMainMenu()
    {
        if (isChangingFlow)
            return;

        isChangingFlow = true;
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

    public void ApplyBootState()
    {
        SetAlwaysOnObjectsActive();
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
        SetCanvasActive(canvasMainMenu, true);
        SetCanvasActive(canvasGamePlay, false);
    }

    private void ShowGameplayCanvasOnly()
    {
        SetCanvasActive(canvasMainMenu, false);
        SetCanvasActive(canvasGamePlay, true);
    }

    private void HideResultPanels()
    {
        winPanel?.HideImmediate();
        losePanel?.HideImmediate();
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

    private static void SetCanvasActive(Canvas targetCanvas, bool isActive)
    {
        if (targetCanvas != null)
            targetCanvas.gameObject.SetActive(isActive);
    }
}
