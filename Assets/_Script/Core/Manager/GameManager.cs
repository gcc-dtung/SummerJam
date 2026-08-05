using System;
using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private CanvasTransition _transition;
    private GameObject currentLayOut;
    private bool isGameStart;
    public Action<GameState> OnGameStateChanged;
    public event Action<LevelConfig, GameObject> OnLevelReady;
    public GameState currentState { get; private set; }

    private void Start()
    {
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.LoadGame();
        }
        SoundManager.PlayBGMSound(BGMType.MainMenu);
        LevelManager.Instance.LoadLevelText();
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
        SoundManager.PlaySFXSound(SFXType.Win);
    }

    private void HandleLose()
    {
        SoundManager.PlaySFXSound(SFXType.Lose);
       isGameStart = false;
    }

    public void ResumeCurrentLevelOnNextStart()
    {
        isGameStart = false;
    }

    public void AdvanceLevelOnNextStart()
    {
        isGameStart = true;
    }

    private void HandleReplay()
    {
        isGameStart = false;
        UpdateGameState(GameState.SetUp);
        UpdateGameState(GameState.GamePlay);
    }

    private void HandleSetUp()
    {
        SoundManager.PlayBGMSound(BGMType.GamePlay);
        if (!isGameStart)
        {
          if(currentLayOut != null)  Destroy(currentLayOut);
            LevelManager.Instance.LoadCurrentLevel();
            currentLayOut = Instantiate(LevelManager.Instance.CurrentLevel.Layout);
            currentLayOut.SetActive(true);
            isGameStart = true;
        }
        else
        {
          if(currentLayOut != null)  Destroy(currentLayOut);
            LevelManager.Instance.LoadNextLevel();
            currentLayOut = Instantiate(LevelManager.Instance.CurrentLevel.Layout);
            currentLayOut.SetActive(true);
        }

        OnLevelReady?.Invoke(LevelManager.Instance.CurrentLevel, currentLayOut);
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
