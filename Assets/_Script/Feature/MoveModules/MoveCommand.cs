using Unity.VisualScripting;
using UnityEngine;

public class MoveCommand
{
    private Person person1;
    private Person person2;
    private Cell fromCell;
    private Cell toCell;
    private bool person1PrevOutside;
    private bool person2PrevOutside;
    private bool wasMoveDeducted;
    
    public MoveCommand(Person p1, Cell from, Person p2, Cell to, bool moveDeducted)
    {
        person1 = p1;
        person2 = p2;
        fromCell = from;
        toCell = to;
        wasMoveDeducted = moveDeducted;
        if (person1 != null) person1PrevOutside = person1.OutSide;
        if (person2 != null) person2PrevOutside = person2.OutSide;
    }

    public void Undo()
    {
        if (person1 != null && fromCell != null)
        {
            person1.transform.SetParent(fromCell.transform);
            person1.transform.localScale = Vector3.one;
            person1.SetOutSideState(person1PrevOutside);
            fromCell.SetPersonToSeat(person1);
            fromCell.CanSeat = false;
            var handler = person1.GetComponent<PersonEventHandler>();
            if (handler != null)
            {
                handler.OnMoveToSeatNotify(fromCell.transform.position);
            }
        }

        if (person2 != null && toCell != null)
        {
            person2.transform.SetParent(toCell.transform);
            person2.transform.localScale = Vector3.one;
            person2.SetOutSideState(person2PrevOutside);
            toCell.SetPersonToSeat(person2);
            toCell.CanSeat = false;
            var handler = person2.GetComponent<PersonEventHandler>();
            if (handler != null) handler.OnMoveToSeatNotify(toCell.transform.position);
        }
        else if (toCell != null)
        {
            toCell.SetPersonToSeat(null);
            toCell.CanSeat = true;
        }
        
        if (wasMoveDeducted)
        {
            MoveManager.Instance.TryIncreaseMove();
        }
        EventBus.Notify(GameEventType.StopDragPerson);
    }
    
}
