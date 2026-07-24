using System;
using NaughtyAttributes;
using PrimeTween;
using UnityEngine;

public class PersonVisual : MonoBehaviour
{
    [SerializeField] private PersonEventHandler eventHandler;
    [SerializeField] private float changeViewDuration;
    [SerializeField] private float changeViewScale;
    [SerializeField] private Vector3 handPosition;
    [SerializeField] private SpriteRenderer handOnPerson;
    [SerializeField] private TooltipPopup tooltipPopup;
    
    private Person person;
    private SpriteRenderer sprite;
    private Color baseColor;
    private float baseScale;
    private int baseOrderInLayer;
    private int baseHandOrderInLayer;
    
    private Tween scaleTween;
    
    private void Awake()
    {
        sprite = this.GetComponent<SpriteRenderer>();
        person = this.GetComponentInParent<Person>();
        baseColor = sprite.color;
        // if(sprite == null) Debug.LogError("No sprite renderer");
        if (tooltipPopup == null)
        {
            tooltipPopup = GetComponentInParent<Person>()?.GetComponentInChildren<TooltipPopup>(true);
        }
    }

    private void OnEnable()
    {
        EventBus.AddListener(GameEventType.Checking,ChangeStatus);
        EventBus.AddListener<Person>(GameEventType.Press,OnPressToAnotherPerson);
        eventHandler.OnStartDrag += ChangeVisualOnStartDrag;
        eventHandler.OnDraggingWithoutMousePosition += Normal;
        eventHandler.OnDrop += ChangeVisualEndDrag;
        eventHandler.OnDraggingWithoutMousePosition += OnHandOnDrag;
        eventHandler.OnPress += ShowTooltip;
        
    }
    
    private void OnDisable()
    {
        EventBus.RemoveListener(GameEventType.Checking,ChangeStatus);
        EventBus.RemoveListener<Person>(GameEventType.Press,OnPressToAnotherPerson);
        eventHandler.OnStartDrag -= ChangeVisualOnStartDrag;
        eventHandler.OnDrop -= ChangeVisualEndDrag;
        eventHandler.OnDraggingWithoutMousePosition -= Normal;
        eventHandler.OnDraggingWithoutMousePosition -= OnHandOnDrag;
        eventHandler.OnPress -= ShowTooltip;
        
    }

    private void Start()
    {
        baseScale = sprite.transform.localScale.x;
        baseOrderInLayer = sprite.sortingOrder;
        baseHandOrderInLayer = handOnPerson.sortingOrder;
        handOnPerson.sortingLayerID = sprite.sortingLayerID;
        handOnPerson.enabled = false;
    }

    private void OnPressToAnotherPerson(Person person)
    {
        if(this.person == person) return;
        if (tooltipPopup != null) tooltipPopup.Hide();
    }

    private void ChangeStatus()
    {
        if(person.OutSide) {Normal(); return;}
        if(person.IsHappy) Happy();
        else Sad();
    }

    private void Normal() => sprite.color = baseColor;
    private void Happy() => sprite.color = Color.green;
    private void Sad() => sprite.color = Color.red;

    private void ChangeVisual(float viewScale, int orderInLayer)
    {
        if(scaleTween.isAlive)
            scaleTween.Stop();

        sprite.sortingOrder = orderInLayer;

        // DragStretch tự scale/xoay quanh điểm tay nắm. Nếu scale thêm ở đây,
        // sprite sẽ scale quanh tâm và làm điểm đầu trượt khỏi bàn tay.
        if (GetComponent<DragStretch>() != null) return;

        if(sprite.transform.localScale == Vector3.one * viewScale) return;
        scaleTween = Tween.Scale(sprite.transform, viewScale, changeViewDuration);
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
        string personDescription = person.BuildTooltipContent();
        
        if (tooltipPopup != null) tooltipPopup.Show(personName, personDescription);
    }
    #endregion
}
