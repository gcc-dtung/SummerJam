using System;
using UnityEngine;

public class InputManager : DontDestroyOnLoadSingleton<InputManager>
{
    private MobileInput _inputAction;
    public MobileInput InputAction
    {
        get
        {
            if (_inputAction == null)
            {
                _inputAction = new MobileInput();
            }
            return _inputAction;
        }
    }
    
    private void OnEnable()
    {
        GameManager.Instance.OnGameStateChanged += OnUI;
        GameManager.Instance.OnGameStateChanged += OnGamePlay;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= OnUI;
            GameManager.Instance.OnGameStateChanged -= OnGamePlay;
        }
    }

    private void OnUI(GameState state)
    {
        if(state == GameState.GamePlay) return;
        InputAction.Player.Disable();
        InputAction.UI.Enable();
        
    }

    private void OnGamePlay(GameState state)
    {
        if(state != GameState.GamePlay) return;
        InputAction.Player.Enable();
        InputAction.UI.Enable();
    }
}
