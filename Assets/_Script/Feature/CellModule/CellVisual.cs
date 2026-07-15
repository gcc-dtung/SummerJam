using System;
using UnityEngine;
using PrimeTween;
using Unity.VisualScripting.Dependencies.NCalc;

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
    private Cell cell;

    private void Awake()
    {
        cell = gameObject.GetComponent<Cell>();
    }

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
        EventBus.AddListener(GameEventType.StartDragPerson, TurnOnHoverSprite);
        EventBus.AddListener(GameEventType.StopDragPerson, TurnOffHoverSprite);
    }

    private void OnDisable()
    {
        eventHandler.OnSelected -= ChangeVisualOnSelected;
        eventHandler.OnDeselected -= ChangeVisualOnDeselected;
        EventBus.RemoveListener(GameEventType.StartDragPerson, TurnOnHoverSprite);
        EventBus.RemoveListener(GameEventType.StopDragPerson, TurnOffHoverSprite);
    }

    private void ChangeVisualHover(float viewScale, int orderInLayer)
    {
        if(!CanChange()) return;
        if (scaleTween.isAlive)
            scaleTween.Stop();
        if (hoverSprite.transform.localScale == Vector3.one * viewScale) return;
        hoverSprite.sortingOrder = orderInLayer;
        scaleTween = Tween.Scale(hoverSprite.transform, viewScale, changeViewDuration);
    }

    private void ChangeVisualBackGround(Color c)
    {
        if(!CanChange()) return;
        if (backgroundSprite.color == c) return;
        Color newColor = c;
        newColor.a = 1;
        backgroundSprite.color = newColor;
    }

    private void TurnOnHoverSprite()
    {
        if(!CanChange()) return;
        hoverSprite.enabled = true;
    }
    
    private void TurnOffHoverSprite()
    {
        hoverSprite.enabled = false;
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

    private bool CanChange() => cell.Type == CellType.Seat && cell.CanSeat;
}