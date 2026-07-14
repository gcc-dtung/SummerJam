using System;
using UnityEngine;
using PrimeTween;

public class CellVisual : MonoBehaviour
{
    [SerializeField] private CellEventHandler eventHandler;
    [SerializeField] private float changeViewDuration;
    [SerializeField] private float changeViewScale;
    [SerializeField] private SpriteRenderer hoverSprite;
    [SerializeField] private SpriteRenderer backgroundSprite;
    [SerializeField] private Color backgroundColorWhenChange;
    
    private Tween scaleTween;
    private float baseScale;
    private int baseOrderInLayer;
    private Color baseColor;
    
    private void Start()
    {
        baseScale = hoverSprite.transform.localScale.x;
        baseOrderInLayer = hoverSprite.sortingOrder;
        baseColor = backgroundSprite.color;
        hoverSprite.enabled = false;
    }

    private void OnEnable()
    {
        eventHandler.OnSelected += ChangeVisualOnSelected;
        eventHandler.OnDeselected += ChangeVisualOnDeselected;
        EventBus.AddListener(GameEventType.StartDrag, () => hoverSprite.enabled = true);
        EventBus.AddListener(GameEventType.StopDrag, () => hoverSprite.enabled = false);
    }

    private void OnDisable()
    {
        eventHandler.OnSelected -= ChangeVisualOnSelected;
        eventHandler.OnDeselected -= ChangeVisualOnDeselected;
        EventBus.RemoveListener(GameEventType.StartDrag, () => hoverSprite.enabled = true);
        EventBus.RemoveListener(GameEventType.StopDrag, () => hoverSprite.enabled = false);
    }

    private void ChangeVisualHover(float viewScale, int orderInLayer)
    {
        if (scaleTween.isAlive)
            scaleTween.Stop();
        if (hoverSprite.transform.localScale == Vector3.one * viewScale) return;
        hoverSprite.sortingOrder = orderInLayer;
        scaleTween = Tween.Scale(hoverSprite.transform, viewScale, changeViewDuration);
    }

    private void ChangeVisualBackGround(Color c)
    {
        if (backgroundSprite.color == c) return;
        Color newColor = c;
        newColor.a = 1;
        backgroundSprite.color = newColor;
    }

    public void ChangeVisualOnSelected()
    {
        ChangeVisualHover(changeViewScale, Constaints.MAX_SORTING_LAYER);
        ChangeVisualBackGround(backgroundColorWhenChange);
    }

    public void ChangeVisualOnDeselected()
    {
        ChangeVisualHover(baseScale, baseOrderInLayer);
        ChangeVisualBackGround(baseColor);
    }
}