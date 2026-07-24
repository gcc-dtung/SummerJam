using System;
using UnityEngine;

public class InputManager : Singleton<InputManager>
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
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.GamePlay)
        {
            InputAction.Player.Enable();
            InputAction.UI.Disable();
        }
        else
        {
            InputAction.Player.Disable();
            InputAction.UI.Enable();
        }
    }
}
