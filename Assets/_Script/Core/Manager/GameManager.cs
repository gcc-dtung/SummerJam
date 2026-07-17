using System;
using UnityEngine;

public class GameManager : DontDestroyOnLoadSingleton<GameManager>
{
    public Action<GameState> OnGameStateChanged;
    public GameState currentState { get; private set; }

    private void Start()
    {
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
                HandleSetUp();
                break;
        }
        OnGameStateChanged?.Invoke(currentState);
    }

    private void HandleWin()
    {
        
    }

    private void HandleLose()
    {
        
    }

    private void HandleReplay()
    {
        
    }

    private void HandleSetUp()
    {
        
    }
    
    
    
}

public enum GameState
{
    Win = 0,
    Lose = 1,
    Replay = 2,
    GamePlay = 4,
}
