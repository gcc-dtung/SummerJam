using System;
using UnityEngine;

public class InputManager : DontDestroyOnLoadSingleton<InputManager>
{
    public MobileInput InputAction { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        InputAction = new MobileInput();
    }

    private void OnEnable()
    {
        GameManager.Instance.OnGameStateChanged += OnUI;
        GameManager.Instance.OnGameStateChanged += OnGamePlay;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnGameStateChanged -= OnUI;
        GameManager.Instance.OnGameStateChanged -= OnGamePlay;
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
