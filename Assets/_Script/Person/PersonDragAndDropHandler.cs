using UnityEngine;

public class PersonDragAndDropHandler : MonoBehaviour,IDraggable
{
    [SerializeField] private Person person;
    [SerializeField] private PersonMovement movement;
    private Vector3 oldPosition;
    
    public void StartDrag()
    {
        oldPosition = this.transform.position;
    }

    public void Drag(Vector3 dragPosition)
    {
        this.transform.position = dragPosition;
    }

    public void Drop(Vector3 endPosition)
    {
        int x, y;
        if (GridManager.Instance.Board.TryGetCellFromWorldPos(endPosition,out x,out y))
        {
            if (SetSeat(x, y, GridManager.Instance.Board))
            {
                ResetOldSeat(oldPosition);
                return;
            }
        }
        
        if (GridManager.Instance.WaitLine.TryGetCellFromWorldPos(endPosition, out x, out y))
        {
            if (SetSeat(x, y, GridManager.Instance.WaitLine))
            {
                ResetOldSeat(oldPosition);
                return;
            }
        }
        
        movement.MoveToPosition(oldPosition);
    }

    private bool SetSeat(int x,int y,Grid<Cell> hold)
    {
        Cell cell = hold.GetValue(x, y);
        if (cell.Type == CellType.Seat)
        {
            if (cell.CanSeat)
            {
                cell.CanSeat = false;
                cell.SetPersonToSeat(person);
                movement.MoveToPosition( hold.GetWorldPosition(x, y));
                return true;
            }
        }
         return false;
    }

    private void ResetOldSeat(Vector3 position)
    {
        Cell cell = GridManager.Instance.Board.GetValueInScreenPosition(position);
        if (cell == null)
        {
            cell = GridManager.Instance.WaitLine.GetValueInScreenPosition(position);
        }
        if(cell == null) return;
        if (cell.Type == CellType.Seat)
        {
            cell.CanSeat = true;
            cell.SetPersonToSeat(null);
        }
    }


}
