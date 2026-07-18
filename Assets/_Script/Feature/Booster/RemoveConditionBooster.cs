using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class RemoveConditionBooster : MonoBehaviour
{
    [SerializeField] private ConditionsSO noCondition;
    private Camera mainCam;
    private Action onUsedCallback;
    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void OnEnable()
    {
        InputManager.Instance.InputAction.Player.HoldAndDrag.canceled += OnHoldCanceled;
    }

    private void OnDisable()
    {
        InputManager.Instance.InputAction.Player.HoldAndDrag.canceled -= OnHoldCanceled;
    }

    private void Update()
    {
        if(InputManager.Instance.InputAction.Player.enabled != true) return;
        Vector2 screenPosition = InputManager.Instance.InputAction.Player.PointerPosition.ReadValue<Vector2>();
        Vector2 worldPosition = mainCam.ScreenToWorldPoint(screenPosition);
        this.transform.position = worldPosition;
    }
    
    public void Init(Action onUsed)
    {
        onUsedCallback = onUsed;
    }

    private void OnHoldCanceled(InputAction.CallbackContext context)
    {
        Vector2 screenPosition = InputManager.Instance.InputAction.Player.PointerPosition.ReadValue<Vector2>();
        Vector2 worldPosition = mainCam.ScreenToWorldPoint(screenPosition);
        ApplyBooster(worldPosition);
    }


    public void ApplyBooster(Vector3 endPosition)
    {
        int x, y;
        if (GridManager.Instance.Board.TryGetCellFromWorldPos(endPosition, out x, out y))
        {
            Cell target = GridManager.Instance.Board.GetValue(x, y);
            if (target.Type == CellType.Seat && !target.CanSeat)
            {
                Person person = target.CurrentPerson;
                person?.SetCondition(noCondition);
                EventBus.Notify(GameEventType.StopDragPerson);
                EventBus.Notify(GameEventType.PressOutSide);
                onUsedCallback?.Invoke();
                Destroy(this.gameObject);
                return;
            }
        }

        if (GridManager.Instance.WaitLine.TryGetCellFromWorldPos(endPosition, out x, out y))
        {
            Cell target = GridManager.Instance.WaitLine.GetValue(x, y);
            if (target.Type == CellType.Seat && !target.CanSeat)
            {
                Person person = target.CurrentPerson;
                person?.SetCondition(noCondition);
                EventBus.Notify(GameEventType.StopDragPerson);
                EventBus.Notify(GameEventType.PressOutSide);
                onUsedCallback?.Invoke();
                Destroy(this.gameObject);
                return;
            }
        }
        Destroy(this.gameObject);
    }
}
