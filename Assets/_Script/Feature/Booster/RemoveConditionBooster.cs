using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class RemoveConditionBooster : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private ConditionsSO noCondition;
    [SerializeField] private Transform holder;
    [SerializeField] private GameObject removeBoosterVisual;
    [SerializeField] private LayerMask targetLayer;
    private Camera mainCam;
    private GameObject ghostIcon;
    private RectTransform ghostRect;

    private void Start()
    {
        mainCam = Camera.main;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!BoosterManager.Instance.CanRemove()) return;
        ghostIcon = Instantiate(removeBoosterVisual, holder);
        ghostRect = ghostIcon.GetComponent<RectTransform>();
        ghostRect.position = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghostIcon != null)
        {
            ghostRect.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ghostIcon == null) return;
        Destroy(ghostIcon);

        Vector2 worldPosition = mainCam.ScreenToWorldPoint(eventData.position);
        Collider2D col = Physics2D.OverlapPoint(worldPosition, targetLayer, -10, 10);
        if (col != null && col.TryGetComponent<Person>(out var person))
        {
            person?.SetCondition(noCondition);
            EventBus.Notify(GameEventType.StopDragPerson);
            EventBus.Notify(GameEventType.PressOutSide);
            BoosterManager.Instance.RemoveHandle();
            return;
        }
    }
}
