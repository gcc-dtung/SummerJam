using System;
using UnityEngine;
using PrimeTween;
using Unity.VisualScripting.Dependencies.NCalc;

public class CellVisual : MonoBehaviour
{
    [SerializeField] private CellEventHandler eventHandler;
    [SerializeField] private float changeViewDuration;
    [SerializeField] private float changeViewScale;
    [SerializeField, Min(0.01f)] private float cellSizeMultiplier = 1f;
    [SerializeField] private SpriteRenderer hoverSprite;
    [SerializeField] private SpriteRenderer backgroundSprite;
    [SerializeField] private Color backgroundColorWhenChange;
    
    private Tween scaleTween;
    private Vector3 baseScale;
    private int baseOrderInLayer;
    private Color baseColor;
    private Cell cell;

    private void Awake()
    {
        cell = gameObject.GetComponent<Cell>();
    }

    private void Start()
    {
        baseScale = hoverSprite.transform.localScale;
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
        Vector3 targetScale = baseScale * viewScale;
        if (hoverSprite.transform.localScale == targetScale) return;
        hoverSprite.sortingOrder = orderInLayer;
        scaleTween = Tween.Scale(hoverSprite.transform, targetScale, changeViewDuration);
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
        ChangeVisualHover(1f, baseOrderInLayer);
        ChangeVisualBackGround(baseColor);
    }

    public void SetCellSize(Vector2 cellSize)
    {
        if (hoverSprite == null || hoverSprite.sprite == null) return;

        Vector2 spriteSize = hoverSprite.sprite.bounds.size;
        if (spriteSize.x <= Mathf.Epsilon || spriteSize.y <= Mathf.Epsilon) return;

        Transform visualTransform = hoverSprite.transform;
        Vector3 parentScale = visualTransform.parent != null
            ? visualTransform.parent.lossyScale
            : Vector3.one;

        float scaleX = cellSize.x * cellSizeMultiplier /
                       (spriteSize.x * Mathf.Max(Mathf.Abs(parentScale.x), Mathf.Epsilon));
        float scaleY = cellSize.y * cellSizeMultiplier /
                       (spriteSize.y * Mathf.Max(Mathf.Abs(parentScale.y), Mathf.Epsilon));

        visualTransform.localScale = new Vector3(scaleX, scaleY, visualTransform.localScale.z);
        baseScale = visualTransform.localScale;
    }

    private bool CanChange() => cell.Type == CellType.Seat && cell.CanSeat;
}
