using UnityEngine;

public class PersonDragAndDropHandler : MonoBehaviour, IDraggable,IPressable
{
    [SerializeField] private PersonEventHandler eventHandler;
    [SerializeField] private Person person;
    [SerializeField] private Vector3 offset = new Vector3(0,0.5f,0);
    private Vector3 oldPosition;
    private Cell oldCell;
    private Cell hoveredCell;
    
    public void Press()
    {
       eventHandler.OnPressNotify();
    }

    public void StartDrag()
    {
        oldCell = this.transform.parent.GetComponent<Cell>();
        this.transform.SetParent(null);
        oldPosition = oldCell.transform.position;
        eventHandler.OnStartDragNotify();
    }

    public void Drag(Vector3 dragPosition)
    {
        dragPosition.z = 0f;
        eventHandler.OnDraggingNotify(dragPosition + offset);
        
        Cell currentCell = GridManager.Instance.Board.GetValueFromWorldPosition(this.transform.position);
        if(currentCell == null)  currentCell = GridManager.Instance.WaitLine.GetValueFromWorldPosition(this.transform.position);
        
        if (currentCell ==null || !currentCell.CanSeat)
        {
            if (hoveredCell != null)
            {
                hoveredCell.CellEventHandler.OnDeselectedNotify();
                hoveredCell = null;
            }
            return;
        }
        
        if (currentCell != hoveredCell)
        {
            hoveredCell?.CellEventHandler.OnDeselectedNotify();
            hoveredCell = currentCell;
            hoveredCell?.CellEventHandler.OnSelectedNotify();
        }
        
    }

    public void Drop(Vector3 endPosition)
    {
        if (hoveredCell != null)
        {
            hoveredCell.CellEventHandler.OnDeselectedNotify();
            hoveredCell = null;
        }
        
        int x, y;
        if (GridManager.Instance.Board.TryGetCellFromWorldPos(this.transform.position, out x, out y))
        {
            if (IsEmptySeat(x, y, GridManager.Instance.Board))
            {
                Cell targetCell = GridManager.Instance.Board.GetValue(x, y);
                UndoManager.Instance.RecordMove(person, oldCell, null, targetCell, true);
                
                SetSeat(x,y,GridManager.Instance.Board);
                ResetOldSeat(oldPosition);
                person.SetOutSideState(false);
                
                eventHandler.OnDropNotify();
                EventBus.Notify(GameEventType.PlacePerson);
                return;
            }
            else
            {
                if (GridManager.Instance.Board.GetValue(x, y).Type == CellType.Seat)
                {
                    Vector3 snappedTargetPos = GridManager.Instance.Board.GetWorldPosition(x, y);
                    if (SwapSeat(oldPosition, snappedTargetPos))
                    {
                        eventHandler.OnDropNotify();
                        return;
                    }
                }
            }
        }

        if (GridManager.Instance.WaitLine.TryGetCellFromWorldPos(this.transform.position, out x, out y))
        {
            if (IsEmptySeat(x, y, GridManager.Instance.WaitLine))
            {
                Cell targetCell = GridManager.Instance.WaitLine.GetValue(x, y);
                UndoManager.Instance.RecordMove(person, oldCell, null, targetCell, !person.OutSide);
               if(!person.OutSide) EventBus.Notify(GameEventType.PlacePerson);
                SetSeat(x,y,GridManager.Instance.WaitLine);
                ResetOldSeat(oldPosition);
                person.SetOutSideState(true);
                
                
                eventHandler.OnDropNotify();
                return;
            }
            else
            {
                if (GridManager.Instance.WaitLine.GetValue(x, y).Type == CellType.Seat)
                {
                    Vector3 snappedTargetPos = GridManager.Instance.WaitLine.GetWorldPosition(x, y);
                    if (SwapSeat(oldPosition, snappedTargetPos))
                    {
                        eventHandler.OnDropNotify();
                        return;
                    }
                }
            }
        }
 
        this.transform.SetParent(oldCell.transform);
        transform.localScale = Vector3.one;
        eventHandler.OnMoveToSeatNotify(oldPosition);
        eventHandler.OnDropNotify();
        oldCell = null;
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
        
        eventHandler.OnMoveToSeatNotify( hold.GetWorldPosition(x, y));
        return;
    }

    private bool SwapSeat(Vector3 origin,Vector3 target)
    {
        bool cell1OutSide = false, cell2OutSide = false;
        Cell cell1 = GridManager.Instance.Board.GetValueFromWorldPosition(origin);
        Cell cell2 = GridManager.Instance.Board.GetValueFromWorldPosition(target);
        if (cell1 == null)
        {
            cell1 = GridManager.Instance.WaitLine.GetValueFromWorldPosition(origin);
            cell1OutSide = true;
        }

        if (cell2 == null)
        {
            cell2 = GridManager.Instance.WaitLine.GetValueFromWorldPosition(target);
            cell2OutSide = true;
        }
        
        if(cell1 == null || cell2 == null || cell1 == cell2) return false;
        
        Person tmp1 = cell1.CurrentPerson;
        Person tmp2 = cell2.CurrentPerson;
        
                
        bool willDeductMove = (cell1OutSide && !cell2OutSide) || (!cell1OutSide && cell2OutSide);
        UndoManager.Instance.RecordMove(tmp1, cell1, tmp2, cell2, willDeductMove);
        
        
        cell1.SetPersonToSeat(cell2.CurrentPerson);
        cell2.SetPersonToSeat(tmp1);

        if (cell1OutSide && !cell2OutSide)
        {
            EventBus.Notify(GameEventType.PlacePerson);
            tmp2.SetOutSideState(true);
            tmp1.SetOutSideState(false);
        }
        else if (!cell1OutSide && cell2OutSide)
        {
            EventBus.Notify(GameEventType.PlacePerson);
            tmp2.SetOutSideState(false);
            tmp1.SetOutSideState(true);
        }
        else 
        if (!cell1OutSide && !cell2OutSide)
        {
            EventBus.Notify(GameEventType.PlacePerson);
            tmp2.SetOutSideState(false);
            tmp1.SetOutSideState(false);
        }
        
        
        this.transform.SetParent(cell2.transform);
        this.transform.localScale = Vector3.one;
        tmp2.transform.SetParent(cell1.transform);
        tmp2.transform.localScale = Vector3.one;
        

        
        eventHandler.OnMoveToSeatNotify(target);
        tmp2.GetComponent<PersonEventHandler>().OnMoveToSeatNotify(origin);
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