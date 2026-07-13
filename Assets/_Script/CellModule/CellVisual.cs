using System;
using UnityEngine;
using PrimeTween;

public class CellVisual : MonoBehaviour
{
    [SerializeField] private CellEventHandler eventHandler;
    [SerializeField] private float changeViewDuration;
    [SerializeField] private float changeViewScale;
    private SpriteRenderer sprite;
    private Tween scaleTween;
    private float baseScale;
    private int baseOrderInLayer;

    private void Awake()
    {
        sprite = this.GetComponent<SpriteRenderer>();
    }
    
    private void Start()
    {
        baseScale = sprite.transform.localScale.x;
        baseOrderInLayer = sprite.sortingOrder;
    }

    private void OnEnable()
    {
        eventHandler.OnSelected += ChangeVisualOnSelected;
        eventHandler.OnDeselected += ChangeVisualOnDeselected;
    }

    private void OnDisable()
    {
        eventHandler.OnSelected -= ChangeVisualOnSelected;
        eventHandler.OnDeselected -= ChangeVisualOnDeselected;
    }

    private void ChangeVisual(float viewScale, int orderInLayer)
    {
        if (scaleTween.isAlive)
            scaleTween.Stop();
        if (sprite.transform.localScale == Vector3.one * viewScale) return;
        sprite.sortingOrder = orderInLayer;
        scaleTween = Tween.Scale(sprite.transform, viewScale, changeViewDuration);
    }

    public void ChangeVisualOnSelected()
    {
        ChangeVisual(changeViewScale, Constaints.MAX_SORTING_LAYER);
    }

    public void ChangeVisualOnDeselected()
    {
        ChangeVisual(baseScale, baseOrderInLayer);
    }
}