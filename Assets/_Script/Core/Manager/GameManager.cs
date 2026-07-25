using System;
using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private CanvasTransition _transition;
    [SerializeField] private LosePanel losePanel;
    private bool isGameStart;
    public Action<GameState> OnGameStateChanged;
    public GameState currentState { get; private set; }

    private void Start()
    {
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.LoadGame();
        }

        isGameStart = false;
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
            case GameState.SetUp:
                HandleSetUp();
                break;
        }
        OnGameStateChanged?.Invoke(currentState);
    }

    private void HandleWin()
    {
        
        _transition.PlayAsync(() =>
        {
            UpdateGameState(GameState.SetUp);
            UpdateGameState(GameState.GamePlay);
            SaveLoadManager.Instance.SaveGame();
        });
    }

    private void HandleLose()
    {
       losePanel.OnLose();
       isGameStart = false;
    }

    private void HandleReplay()
    {
        
    }

    private void HandleSetUp()
    {
        if (!isGameStart)
        {
            LevelManager.Instance.LoadCurrentLevel();
            isGameStart = true;
        }
        else
        {
            LevelManager.Instance.LoadNextLevel();
        }
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
