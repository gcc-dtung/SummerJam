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
    
    private Person person;
    private SpriteRenderer sprite;
    private Color baseColor;
    private float baseScale;
    private int baseOrderInLayer;
    
    private Tween scaleTween;
    
    private void Awake()
    {
        sprite = this.GetComponent<SpriteRenderer>();
        person = this.GetComponentInParent<Person>();
        baseColor = sprite.color;
        // if(sprite == null) Debug.LogError("No sprite renderer");
    }

    private void OnEnable()
    {
        EventBus.AddListener(GameEventType.Checking,ChangeStatus);
        eventHandler.OnStartDrag += ChangeVisualOnStartDrag;
        eventHandler.OnDraggingWithoutMousePosition += Normal;
        eventHandler.OnDrop += ChangeVisualEndDrag;
        eventHandler.OnDraggingWithoutMousePosition += OnHandOnDrag;
        
    }
    
    private void OnDisable()
    {
        EventBus.RemoveListener(GameEventType.Checking,ChangeStatus);
        eventHandler.OnStartDrag -= ChangeVisualOnStartDrag;
        eventHandler.OnDrop -= ChangeVisualEndDrag;
        eventHandler.OnDraggingWithoutMousePosition -= Normal;
        eventHandler.OnDraggingWithoutMousePosition -= OnHandOnDrag;
        
    }

    private void Start()
    {
        baseScale = sprite.transform.localScale.x;
        baseOrderInLayer = sprite.sortingOrder;
        handOnPerson.enabled = false;
    }



    private void ChangeStatus()
    {
        if(person.OutSide) {Normal(); return;}
        if(person.ConditionChecking.IsHappy) Happy();
        else Sad();
    }

    private void Normal() => sprite.color = baseColor;
    private void Happy() => sprite.color = Color.green;
    private void Sad() => sprite.color = Color.red;

    private void ChangeVisual(float viewScale, int orderInLayer)
    {
        if(scaleTween.isAlive)
            scaleTween.Stop();
        if(sprite.transform.localScale == Vector3.one * viewScale) return;
        sprite.sortingOrder = orderInLayer;
        scaleTween = Tween.Scale(sprite.transform, viewScale, changeViewDuration);
    }
    
    #region Public API
    [Button("VisualOnDrag")]
    public void ChangeVisualOnStartDrag()
    {
        ChangeVisual(changeViewScale, Constaints.MAX_SORTING_LAYER);
        handOnPerson.enabled = true;
    }
    
    [Button("VisualEndDrag")]
    public void ChangeVisualEndDrag()
    {
        ChangeVisual(baseScale, baseOrderInLayer);
        handOnPerson.enabled = false;
    }

    public void OnHandOnDrag()
    {
        handOnPerson.transform.localPosition = handPosition + sprite.transform.localPosition;
    }
    #endregion
}
