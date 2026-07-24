using System;
using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private CanvasTransition _transition;
    public Action<GameState> OnGameStateChanged;
    public GameState currentState { get; private set; }

    private void Start()
    {
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.LoadGame();
        }
        StartCoroutine(StartGameFlow(true));
    }

    private IEnumerator StartGameFlow(bool isGameStart = false)
    {
        UpdateGameState(GameState.SetUp);
        if (isGameStart)
        {
            LevelManager.Instance.LoadCurrentLevel();
        }
        else
        {
            LevelManager.Instance.LoadNextLevel();
        }
        yield return null;
        UpdateGameState(GameState.GamePlay);
    }

    private IEnumerator RestartGameFlow()
    {
        UpdateGameState(GameState.SetUp);
        LevelManager.Instance.LoadCurrentLevel();
        yield return null;
        UpdateGameState(GameState.GamePlay);
    }

    public void UpdateGameState(GameState state)
    {
        currentState = state;
        switch (state)
        {
            case GameState.Win:
                HandleWin();
                break;
            case GameState.Lose:
                HandleLose();
                break;
            case GameState.Replay:
                HandleReplay();
                break;
            case GameState.GamePlay:
                HandleGamePlay();
                break;
        }
        OnGameStateChanged?.Invoke(currentState);
    }

    private void HandleWin()
    {
        _transition.PlayAsync(() => {StartCoroutine(StartGameFlow(false));});
        
    }

    private void HandleLose()
    {
        Debug.Log("Lose");
    }

    private void HandleReplay()
    {
        StartCoroutine(RestartGameFlow());
    }

    private void HandleSetUp()
    {
        
    }

    private void HandleGamePlay()
    {
        
    }
    
    
    
}

public enum GameState
{
    Win = 0,
    Lose = 1,
    Replay = 2,
    GamePlay = 3,
    SetUp = 4
}
