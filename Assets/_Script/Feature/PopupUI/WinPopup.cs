using System;
using UnityEngine;
using UnityEngine.UI;

public class WinPopup : MonoBehaviour
{
    [SerializeField] private Transform root;
    [SerializeField] private Button nextlevelbutton;

    private void OnEnable()
    {
         GameManager.Instance.OnGameStateChanged += Show;
        nextlevelbutton.onClick.AddListener(NextLevel);
    }

    private void OnDisable()
    {
         GameManager.Instance.OnGameStateChanged -= Show;
        nextlevelbutton.onClick.RemoveListener(NextLevel);
    }

    private void Start()
    {
        root.gameObject.SetActive(false);
    }

    private void Show(GameState gameState)
    {
        if (gameState != GameState.Win) return;
        root.gameObject.SetActive(true);
    }

    private void NextLevel()
    {
        root.gameObject.SetActive(false);
        LevelManager.Instance.LoadNextLevel();
    }
}
