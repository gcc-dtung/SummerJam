using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragAndDropController : MonoBehaviour
{
    private MobileInput inputAction;
    private bool isDragging = false;

    [SerializeField] private LayerMask draggableLayer;
    private float depthDistance = 10f;
    private IDraggable currentDragItem;
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
        inputAction.Player.Tap.performed += OnTap;
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

        inputAction.Player.Tap.performed -= OnTap;
        inputAction.Player.HoldAndDrag.performed -= OnHoldStarted;
        inputAction.Player.HoldAndDrag.canceled -= OnHoldCanceled;
        inputAction.Player.Disable();
    }

    private void OnTap(InputAction.CallbackContext context)
    {
        Vector2 screenPosition = inputAction.Player.PointerPosition.ReadValue<Vector2>();
        Vector2 worldPosition = mainCam.ScreenToWorldPoint(screenPosition);
        Detect(worldPosition);
        currentDragItem?.Press();
        currentDragItem = null;
    }

    private void OnHoldStarted(InputAction.CallbackContext context)
    {
        Vector2 screenPosition = inputAction.Player.PointerPosition.ReadValue<Vector2>();
        Vector2 worldPosition = mainCam.ScreenToWorldPoint(screenPosition);
        Detect(worldPosition);

        if (currentDragItem != null)
        {
            isDragging = true;
            currentDragItem?.StartDrag();
        }
    }

    private void OnHoldCanceled(InputAction.CallbackContext context)
    {
        isDragging = false;
        Vector2 screenPosition = inputAction.Player.PointerPosition.ReadValue<Vector2>();
        Vector2 worldPosition = mainCam.ScreenToWorldPoint(screenPosition);
        currentDragItem?.Drop(worldPosition);
        currentDragItem = null;
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
            EventBus.Notify<Vector2>(GameEventType.Dragging, worldPosition);
        }
    }

    void Detect(Vector2 position)
    {
        currentDragItem = null;
        Collider2D col = Physics2D.OverlapPoint(position, draggableLayer, -depthDistance, depthDistance);
        if (col != null && col.TryGetComponent<IDraggable>(out var draggable))
        {
            currentDragItem = draggable;
        }
    }
}