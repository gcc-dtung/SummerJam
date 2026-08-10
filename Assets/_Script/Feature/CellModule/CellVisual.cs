using UnityEngine;
using UnityEngine.Serialization;
using PrimeTween;

public class CellVisual : MonoBehaviour
{
    [Header("Hover Animation")]
    [SerializeField] private CellEventHandler eventHandler;
    [SerializeField, Min(0f)] private float changeViewDuration;
    [SerializeField, Min(0.01f)] private float changeViewScale = 1.2f;
    [SerializeField, Min(0.01f)] private float cellSizeMultiplier = 1f;
    [SerializeField] private SpriteRenderer hoverSprite;

    [Header("Hover Color")]
    [SerializeField] private SpriteRenderer backgroundSprite;
    [SerializeField, ColorUsage(false, false), Tooltip("Default color restored when this cell is no longer hovered.")]
    private Color normalColor = Color.white;
    [FormerlySerializedAs("backgroundColorWhenChange")]
    [SerializeField, ColorUsage(false, false), Tooltip("Color shown when this cell is hovered while dragging a person.")]
    private Color hoverColor = Color.red;
    [SerializeField, Tooltip("Preview Hover Color immediately in Scene/Prefab Mode. Turn this off to preview Normal Color.")]
    private bool previewHoverColor;
    
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
        if (hoverSprite != null)
        {
            baseScale = hoverSprite.transform.localScale;
            baseOrderInLayer = hoverSprite.sortingOrder;
            hoverSprite.enabled = false;
        }

        baseColor = normalColor;
        SetBackgroundColor(baseColor);
    }

    private void OnValidate()
    {
        if (Application.isPlaying || backgroundSprite == null) return;
        SetBackgroundColor(previewHoverColor ? hoverColor : normalColor);
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
        if(!CanChange() || hoverSprite == null) return;
        if (scaleTween.isAlive)
            scaleTween.Stop();
        Vector3 targetScale = baseScale * viewScale;
        if (hoverSprite.transform.localScale == targetScale) return;
        hoverSprite.sortingOrder = orderInLayer;
        scaleTween = Tween.Scale(hoverSprite.transform, targetScale, changeViewDuration);
    }

    private void ChangeVisualBackGround(Color c)
    {
        if(!CanChange() || backgroundSprite == null) return;
        SetBackgroundColor(c);
    }

    private void SetBackgroundColor(Color color)
    {
        if (backgroundSprite == null) return;
        color.a = 1f;
        if (backgroundSprite.color == color) return;
        backgroundSprite.color = color;
    }

    private void TurnOnHoverSprite()
    {
        if(!CanChange() || hoverSprite == null) return;
        hoverSprite.enabled = true;
    }
    
    private void TurnOffHoverSprite()
    {
        if (hoverSprite == null) return;
        hoverSprite.enabled = false;
    }

    public void ChangeVisualOnSelected()
    {
        ChangeVisualHover(changeViewScale, Constaints.MAX_SORTING_LAYER);
        ChangeVisualBackGround(hoverColor);
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
