using UnityEngine;

public class PersonDragAndDropHandler : MonoBehaviour, IDraggable
{
    [SerializeField] private Person person;
    [SerializeField] private PersonMovement movement;
    private Vector3 oldPosition;
    private Cell currentCell;
    public void StartDrag()
    {
        this.transform.SetParent(null);
        oldPosition = this.transform.position;
        currentCell = GridManager.Instance.Board.GetValueFromWorldPosition(oldPosition);
        if (currentCell == null) currentCell = GridManager.Instance.WaitLine.GetValueFromWorldPosition(oldPosition);
    }

    public void Drag(Vector3 dragPosition)
    {
        dragPosition.z = 0f;
        this.transform.position = dragPosition;
    }

    public void Drop(Vector3 endPosition)
    {
        int x, y;
        if (GridManager.Instance.Board.TryGetCellFromWorldPos(endPosition, out x, out y))
        {
            if (IsEmptySeat(x, y, GridManager.Instance.Board))
            {
                SetSeat(x,y,GridManager.Instance.Board);
                ResetOldSeat(oldPosition);
                return;
            }
            else
            {
                if (GridManager.Instance.Board.GetValue(x, y).Type == CellType.Seat)
                {
                    Vector3 snappedTargetPos = GridManager.Instance.Board.GetWorldPosition(x, y);
                   if(SwapSeat(oldPosition,snappedTargetPos))
                    return;
                }
            }
        }

        if (GridManager.Instance.WaitLine.TryGetCellFromWorldPos(endPosition, out x, out y))
        {
            if (IsEmptySeat(x, y, GridManager.Instance.WaitLine))
            {
                SetSeat(x,y,GridManager.Instance.WaitLine);
                ResetOldSeat(oldPosition);
                return;
            }
            else
            {
                if (GridManager.Instance.WaitLine.GetValue(x, y).Type == CellType.Seat)
                {
                    Vector3 snappedTargetPos = GridManager.Instance.WaitLine.GetWorldPosition(x, y);
                    if(SwapSeat(oldPosition,snappedTargetPos))
                    return;
                }
            }
        }
 
        this.transform.SetParent(currentCell.transform);
        transform.localScale = Vector3.one;
        movement.MoveToPosition(oldPosition);
        currentCell = null;
    }

    private bool IsEmptySeat(int x, int y, Grid<Cell> hold)
    {
        Cell cell = hold.GetValue(x, y);
        if (cell.Type == CellType.Seat && cell.CanSeat) return true;
        return false;
    }

    private void SetSeat(int x, int y, Grid<Cell> hold)
    {
        Cell cell = hold.GetValue(x, y);
        
        cell.CanSeat = false;
        cell.SetPersonToSeat(person);
        
        transform.SetParent(cell.transform);
        transform.localScale = Vector3.one;
        
        movement.MoveToPosition(hold.GetWorldPosition(x, y));
        return;
    }

    private bool SwapSeat(Vector3 origin,Vector3 target)
    {
        Cell cell1 = GridManager.Instance.Board.GetValueFromWorldPosition(origin);
        Cell cell2 = GridManager.Instance.Board.GetValueFromWorldPosition(target);
        if (cell1 == null) cell1 = GridManager.Instance.WaitLine.GetValueFromWorldPosition(origin);
        if (cell2 == null) cell2 = GridManager.Instance.WaitLine.GetValueFromWorldPosition(target);
        if(cell1 == null || cell2 == null || cell1 == cell2) return false;
        
        Person tmp1 = cell1.CurrentPerson;
        Person tmp2 = cell2.CurrentPerson;
        cell1.SetPersonToSeat(cell2.CurrentPerson);
        cell2.SetPersonToSeat(tmp1);
        
        this.transform.SetParent(cell2.transform);
        this.transform.localScale = Vector3.one;
        tmp2.transform.SetParent(cell1.transform);
        tmp2.transform.localScale = Vector3.one;
        
        
        movement.MoveToPosition(target);
        tmp2.GetComponent<PersonMovement>().MoveToPosition(origin);
        return true;
    }

    private void ResetOldSeat(Vector3 position)
    {
        Cell cell = GridManager.Instance.Board.GetValueFromWorldPosition(position);
        if (cell == null)
        {
            cell = GridManager.Instance.WaitLine.GetValueFromWorldPosition(position);
        }

        if (cell == null) return;
        if (cell.Type == CellType.Seat)
        {
            cell.CanSeat = true;
            cell.SetPersonToSeat(null);
        }
    }
}