using System;
using UnityEngine;

public class WinPopup : MonoBehaviour
{
    [SerializeField] private Transform root;

    private void OnEnable()
    {
        GameManager.Instance.OnGameStateChanged += Show;
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
}
