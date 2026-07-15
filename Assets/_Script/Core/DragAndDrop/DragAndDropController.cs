using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragAndDropController : MonoBehaviour
{
    private MobileInput inputAction;
    private bool isDragging = false;

    [SerializeField] private LayerMask draggableLayer;
    [SerializeField] private LayerMask pressableLayer;
    private float depthDistance = 10f;
    private IDraggable currentDragItem;
    private IPressable currentPressItem;
    private Person currentPerson;
    private Camera mainCam;

    private void Awake()
    {
        inputAction = new MobileInput();
    }

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void OnEnable()
    {
        inputAction.Player.Enable();
        inputAction.Player.Tap.performed += OnPress;
        inputAction.Player.HoldAndDrag.performed += OnHoldStarted;
        inputAction.Player.HoldAndDrag.canceled += OnHoldCanceled;
    }

    private void OnDisable()
    {
        if (isDragging && currentDragItem != null)
        {
            Vector2 screenPosition = inputAction.Player.PointerPosition.ReadValue<Vector2>();
            Vector2 worldPosition = mainCam.ScreenToWorldPoint(screenPosition);
            currentDragItem.Drop(worldPosition);
        }

        isDragging = false;
        currentDragItem = null;

        inputAction.Player.Tap.performed -= OnPress;
        inputAction.Player.HoldAndDrag.performed -= OnHoldStarted;
        inputAction.Player.HoldAndDrag.canceled -= OnHoldCanceled;
        inputAction.Player.Disable();
    }

    private void OnPress(InputAction.CallbackContext context)
    {
        Vector2 screenPosition = inputAction.Player.PointerPosition.ReadValue<Vector2>();
        Vector2 worldPosition = mainCam.ScreenToWorldPoint(screenPosition);
        DetectPressItem(worldPosition);
        currentPressItem?.Press();
        if(currentPressItem != null) EventBus.Notify(GameEventType.Press);
        else EventBus.Notify(GameEventType.PressOutSide);
        
        if (currentPerson != null)
        {
            EventBus.Notify<Person>(GameEventType.Press,currentPerson);
        }

        currentPerson = null;
        currentPressItem = null;
    }

    private void OnHoldStarted(InputAction.CallbackContext context)
    {
        Vector2 screenPosition = inputAction.Player.PointerPosition.ReadValue<Vector2>();
        Vector2 worldPosition = mainCam.ScreenToWorldPoint(screenPosition);
        DetectDragItem(worldPosition);
        
        if (currentDragItem != null)
        {
            isDragging = true;
            currentDragItem.StartDrag();
            
            if (currentPerson != null)
            {
                EventBus.Notify(GameEventType.StartDragPerson);
                EventBus.Notify<Person>(GameEventType.StartDragPerson, currentPerson);
            }
        }
    }

    private void OnHoldCanceled(InputAction.CallbackContext context)
    {
        isDragging = false;
        if (currentDragItem == null) return;
        
        Vector2 screenPosition = inputAction.Player.PointerPosition.ReadValue<Vector2>();
        Vector2 worldPosition = mainCam.ScreenToWorldPoint(screenPosition);
        
        currentDragItem?.Drop(worldPosition);
        currentDragItem = null;
        
        if (currentPerson != null)
        {
            EventBus.Notify(GameEventType.StopDragPerson);
            EventBus.Notify<Person>(GameEventType.StopDragPerson,currentPerson);
        }

        currentPerson = null;

    }

    private void Update()
    {
        if (isDragging && currentDragItem != null)
        {
            if (currentDragItem is UnityEngine.Object unityObj && unityObj == null)
            {
                isDragging = false;
                currentDragItem = null;
                return;
            }

            Vector2 screenPosition = inputAction.Player.PointerPosition.ReadValue<Vector2>();
            Vector2 worldPosition = mainCam.ScreenToWorldPoint(screenPosition);
            currentDragItem?.Drag(worldPosition);
            
         if(currentDragItem != null && currentPerson != null)    EventBus.Notify<Vector2>(GameEventType.DraggingPerson, worldPosition);
        }
    }

    void DetectDragItem(Vector2 position)
    {
        currentDragItem = null;
        currentPerson = null;
        Collider2D col = Physics2D.OverlapPoint(position, draggableLayer, -depthDistance, depthDistance);
        if (col != null && col.TryGetComponent<Person>(out var person))
        {
            currentPerson = person;
        }
        
        if (col != null && col.TryGetComponent<IDraggable>(out var draggable))
        {
            currentDragItem = draggable;
        }
    }

    void DetectPressItem(Vector2 position)
    {
        currentPressItem = null;
        currentPerson = null;
        Collider2D col = Physics2D.OverlapPoint(position, pressableLayer, -depthDistance, depthDistance);
        
        if (col != null && col.TryGetComponent<Person>(out var person))
        {
            currentPerson = person;
        }
        
        if (col != null && col.TryGetComponent<IPressable>(out var pressable))
        {
            currentPressItem = pressable;
        }
    }
}