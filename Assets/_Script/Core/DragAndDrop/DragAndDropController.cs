using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragAndDropController : MonoBehaviour
{
    private bool isDragging = false;

    [SerializeField] private LayerMask draggableLayer;
    [SerializeField] private LayerMask pressableLayer;
    [SerializeField] private float dragThreshold = 15f;
    private Vector3 startScreenPosition;
    private bool isPointerDown = false;
    private float depthDistance = 10f;
    private IDraggable currentDragItem;
    private IPressable currentPressItem;
    private Person currentPerson;
    private Camera mainCam;
    
    private void Start()
    {
        mainCam = Camera.main;
    }

    private void OnEnable()
    {
        InputManager.Instance.InputAction.Player.HoldAndDrag.performed += OnHoldStarted;
        InputManager.Instance.InputAction.Player.HoldAndDrag.canceled += OnHoldCanceled;
    }

    private void OnDisable()
    {
        if (isDragging && currentDragItem != null)
        {
            Vector2 screenPosition = InputManager.Instance.InputAction.Player.PointerPosition.ReadValue<Vector2>();
            Vector2 worldPosition = mainCam.ScreenToWorldPoint(screenPosition);
            currentDragItem.Drop(worldPosition);
        }

        isDragging = false;
        isPointerDown = false;
        currentDragItem = null;
        
        InputManager.Instance.InputAction.Player.HoldAndDrag.performed -= OnHoldStarted;
        InputManager.Instance.InputAction.Player.HoldAndDrag.canceled -= OnHoldCanceled;
    }

    
    private void OnHoldStarted(InputAction.CallbackContext context)
    {
        Vector2 screenPosition = InputManager.Instance.InputAction.Player.PointerPosition.ReadValue<Vector2>();
        Vector2 worldPosition = mainCam.ScreenToWorldPoint(screenPosition);
        startScreenPosition = screenPosition;
        isPointerDown = true;
        isDragging = false;
        DetectDragItem(worldPosition);
    }

    private void OnHoldCanceled(InputAction.CallbackContext context)
    {
        isPointerDown = false;
        if (isDragging)
        {
            isDragging = false;
            if (currentDragItem == null) return;

            Vector2 screenPosition = InputManager.Instance.InputAction.Player.PointerPosition.ReadValue<Vector2>();
            Vector2 worldPosition = mainCam.ScreenToWorldPoint(screenPosition);

            currentDragItem?.Drop(worldPosition);
            currentDragItem = null;

            if (currentPerson != null)
            {
                EventBus.Notify(GameEventType.StopDragPerson);
                EventBus.Notify<Person>(GameEventType.StopDragPerson, currentPerson);
            }

            currentPerson = null;
        }
        else
        {
            Vector2 screenPosition = InputManager.Instance.InputAction.Player.PointerPosition.ReadValue<Vector2>();
            Vector2 worldPosition = mainCam.ScreenToWorldPoint(screenPosition);
            DetectPressItem(worldPosition);
            currentPressItem?.Press();
        
            if (currentPressItem != null) 
            {
                EventBus.Notify(GameEventType.Press);
            }
            else 
            {
                EventBus.Notify(GameEventType.PressOutSide);
            }
        
            if (currentPerson != null)
            {
                EventBus.Notify<Person>(GameEventType.Press, currentPerson);
            }
            currentPerson = null;
            currentPressItem = null;
            currentDragItem = null;
        }
        

    }

    private void Update()
    {
        if(InputManager.Instance.InputAction.Player.enabled != true) return;
        if (isPointerDown && !isDragging)
        {
            Vector2 screenPosition = InputManager.Instance.InputAction.Player.PointerPosition.ReadValue<Vector2>();
            float distance = Vector2.Distance(screenPosition, startScreenPosition);
            if (distance > dragThreshold)
            {
                if (currentDragItem != null)
                {
                    isDragging = true;
                    currentDragItem?.StartDrag();
                    if (currentPerson != null)
                    {
                        EventBus.Notify(GameEventType.StartDragPerson);
                        EventBus.Notify<Person>(GameEventType.StartDragPerson, currentPerson);
                    }
                }
                else
                {
                    isPointerDown = false;
                }
            }

        }
        
        
        if (isDragging && currentDragItem != null)
        {
            if (currentDragItem is UnityEngine.Object unityObj && unityObj == null)
            {
                isDragging = false;
                currentDragItem = null;
                return;
            }

            Vector2 screenPosition = InputManager.Instance.InputAction.Player.PointerPosition.ReadValue<Vector2>();
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