using System;
using PrimeTween;
using UnityEngine;

public class CanvasMainMenu : MonoBehaviour
{
    [SerializeField] private RectTransform[] panels;
    [SerializeField] private float duration = 0.35f;

    private int currentIndex = 1;
    private int direction = 1;
    private Tween moveTween;
    private RectTransform currentPanel;
    private RectTransform newPanel;
    
    private float Width => ((RectTransform)transform).rect.width;

    private void Start()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].anchoredPosition = Vector2.right * Width * (i - 1);
        }
    }
    
    public void SlideTo(int index)
    {
        if (index == currentIndex || moveTween.isAlive) return;
        direction = currentIndex < index ? -1 : 1;
        
        currentPanel = panels[currentIndex];
        newPanel =  panels[index];
        currentIndex = index;
        newPanel.anchoredPosition = Vector2.right * (-direction * Width);
        moveTween = Tween.Custom(0f, Width, duration, onValueChange: MoveAnim);
    }

    private void MoveAnim(float value)
    {
        currentPanel.anchoredPosition = Vector2.right * (direction * value);
        newPanel.anchoredPosition = Vector2.right * (-direction * (Width - value));
    }
}
