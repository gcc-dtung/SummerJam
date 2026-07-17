using UnityEngine;

public class RemoveConditionBooster : MonoBehaviour,IDraggable
{
    [SerializeField] private ConditionsSO noCondition;
    public void StartDrag()
    {
        
    }

    public void Drag(Vector3 dragPosition)
    {
        this.transform.position = dragPosition;
    }

    public void Drop(Vector3 endPosition)
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
                this.gameObject.SetActive(false);
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
                this.gameObject.SetActive(false);
                return;
            }
        }
        this.gameObject.SetActive(false);
    }
}
