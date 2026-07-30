using NaughtyAttributes;
using PrimeTween;
using UnityEngine;
using UnityEngine.Serialization;

public class PersonVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PersonEventHandler eventHandler;
    [SerializeField] private TooltipPopup tooltipPopup;

    [Header("Skin")]
    [SerializeField] private PersonSkinSO skinSO;
    [FormerlySerializedAs("bodyRenderer")]
    [SerializeField] private SpriteRenderer skinRenderer;
    [SerializeField] private SpriteRenderer faceRenderer;
    [SerializeField] private SpriteRenderer traitRenderer;
    [SerializeField] private int baseSkinIndex;

    [Header("Drag Visual")]
    [SerializeField] private float changeViewDuration;
    [SerializeField] private float changeViewScale;
    [SerializeField] private Vector3 handPosition;
    [SerializeField] private SpriteRenderer handOnPerson;

    private Person person;
    private SpriteRenderer sprite;
    
    private float baseScale;
    private int baseOrderInLayer;
    private int baseSkinOrderInLayer;
    private int baseFaceOrderInLayer;
    private int baseTraitOrderInLayer;
    private int baseHandOrderInLayer;

    private Tween scaleTween;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        person = GetComponentInParent<Person>();

        if (sprite == null && skinRenderer != null)
            sprite = skinRenderer;

        if (sprite == null)
            sprite = GetComponentInChildren<SpriteRenderer>();

        if (skinRenderer == null)
            skinRenderer = sprite;

        if (faceRenderer == null)
            faceRenderer = sprite;

        if (tooltipPopup == null)
            tooltipPopup = GetComponentInParent<Person>()?.GetComponentInChildren<TooltipPopup>(true);

        ApplyDefaultSkin();
    }

    private void OnEnable()
    {
        EventBus.AddListener(GameEventType.Checking, ChangeStatus);
        EventBus.AddListener<Person>(GameEventType.Press, OnPressToAnotherPerson);

        eventHandler.OnStartDrag += ChangeVisualOnStartDrag;
        eventHandler.OnDraggingWithoutMousePosition += ApplyTraitSkin;
        eventHandler.OnDrop += ChangeVisualEndDrag;
        eventHandler.OnDraggingWithoutMousePosition += OnHandOnDrag;
        eventHandler.OnPress += ShowTooltip;
    }

    private void OnDisable()
    {
        EventBus.RemoveListener(GameEventType.Checking, ChangeStatus);
        EventBus.RemoveListener<Person>(GameEventType.Press, OnPressToAnotherPerson);

        eventHandler.OnStartDrag -= ChangeVisualOnStartDrag;
        eventHandler.OnDrop -= ChangeVisualEndDrag;
        eventHandler.OnDraggingWithoutMousePosition -= ApplyTraitSkin;
        eventHandler.OnDraggingWithoutMousePosition -= OnHandOnDrag;
        eventHandler.OnPress -= ShowTooltip;
    }

    private void Start()
    {
        baseScale = sprite.transform.localScale.x;
        baseOrderInLayer = sprite.sortingOrder;
        baseSkinOrderInLayer = skinRenderer != null ? skinRenderer.sortingOrder : baseOrderInLayer;
        baseFaceOrderInLayer = faceRenderer != null ? faceRenderer.sortingOrder : baseOrderInLayer + 1;
        baseTraitOrderInLayer = traitRenderer != null ? traitRenderer.sortingOrder : baseOrderInLayer + 2;
        baseHandOrderInLayer = handOnPerson.sortingOrder;

        handOnPerson.sortingLayerID = sprite.sortingLayerID;
        handOnPerson.enabled = false;
    }

    private void OnPressToAnotherPerson(Person pressedPerson)
    {
        if (person == pressedPerson) return;

        if (tooltipPopup != null)
            tooltipPopup.Hide();
    }

    private void ChangeStatus()
    {
        if (person.OutSide)
        {
            ApplyNormalFace();
            ApplyTraitSkin();
            return;
        }

        ApplyStateSkin();
        ApplyTraitSkin();
    }

    private void ApplyDefaultSkin()
    {
        ApplyBaseSkin();
        ApplyNormalFace();
        ApplyTraitSkin();
    }

    private void ApplyBaseSkin()
    {
        if (skinSO == null || skinRenderer == null) return;

        Sprite baseSkin = skinSO.GetBaseSkin(baseSkinIndex);
        if (baseSkin != null)
            skinRenderer.sprite = baseSkin;
    }

    private void ApplyNormalFace()
    {
        if (skinSO == null || faceRenderer == null) return;

        Sprite normalFace = skinSO.GetNormalFace();
        if (normalFace != null)
            faceRenderer.sprite = normalFace;
    }

    private void ApplyTraitSkin()
    {
        if (skinSO == null || traitRenderer == null || person == null) return;

        Sprite traitSkin = skinSO.GetTraitSkin(person.Trait);
        traitRenderer.sprite = traitSkin;
        traitRenderer.enabled = traitSkin != null;
    }

    private void ApplyStateSkin()
    {
        if (skinSO == null || faceRenderer == null || person == null) return;

        Sprite stateFace = skinSO.GetStateFace(person.IsHappy);
        if (stateFace != null)
            faceRenderer.sprite = stateFace;
    }


    private void ChangeVisual(float viewScale, int orderInLayer)
    {
        if (scaleTween.isAlive)
            scaleTween.Stop();

        ChangeSortingOrder(orderInLayer);

        // DragStretch tu scale/xoay quanh diem tay nam.
        // Neu scale them o day, diem dau co the truot khoi ban tay.
        if (GetComponent<DragStretch>()) return;

        if (sprite.transform.localScale == Vector3.one * viewScale) return;

        scaleTween = Tween.Scale(sprite.transform, viewScale, changeViewDuration);
    }

    private void ChangeSortingOrder(int orderInLayer)
    {
        sprite.sortingOrder = orderInLayer;

        if (skinRenderer)
            skinRenderer.sortingOrder = orderInLayer;

        if (faceRenderer)
            faceRenderer.sortingOrder = orderInLayer + 1;

        if (traitRenderer)
            traitRenderer.sortingOrder = orderInLayer + 2;
    }

    private void RestoreSortingOrder()
    {
        sprite.sortingOrder = baseOrderInLayer;

        if (skinRenderer != null)
            skinRenderer.sortingOrder = baseSkinOrderInLayer;

        if (faceRenderer != null)
            faceRenderer.sortingOrder = baseFaceOrderInLayer;

        if (traitRenderer != null)
            traitRenderer.sortingOrder = baseTraitOrderInLayer;
    }

    #region Public API

    [Button("VisualOnDrag")]
    public void ChangeVisualOnStartDrag()
    {
        tooltipPopup?.HideImmediate();

        ChangeVisual(changeViewScale, Constaints.MAX_SORTING_LAYER);

        handOnPerson.sortingLayerID = sprite.sortingLayerID;
        handOnPerson.sortingOrder = Constaints.MAX_SORTING_LAYER + 1;
        handOnPerson.enabled = true;
    }

    [Button("VisualEndDrag")]
    public void ChangeVisualEndDrag()
    {
        ChangeVisual(baseScale, baseOrderInLayer);
        RestoreSortingOrder();

        handOnPerson.enabled = false;
        handOnPerson.sortingOrder = baseHandOrderInLayer;
    }

    public void OnHandOnDrag()
    {
        handOnPerson.transform.localPosition = handPosition;
    }

    public Vector3 HandPosition => handPosition;

    public void ShowTooltip()
    {
        string personName = person.Name;
        string personTrait = GetTraitText();
        string personDescription = person.BuildTooltipContent();

        if (tooltipPopup != null)
            tooltipPopup.Show(personName, personTrait, personDescription);
    }

    private string GetTraitText()
    {
        if (person.Trait == null || person.Trait.Count == 0)
            return string.Empty;

        return person.Trait[0].ToString();
    }

    #endregion
}
