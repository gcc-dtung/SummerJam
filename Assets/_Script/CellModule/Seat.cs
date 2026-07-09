using UnityEngine;

public class Seat : Cell
{
    public override CellType Type { get; } = CellType.Seat;
    public override bool CanSeat { get; protected set; } = true;
    public override bool CanInteract { get; protected set; } = false;
    public Person CurrentPerson { get; private set; }

    public void SetPersonToSeat(Person person)
    {
        CurrentPerson = person;
    }

    public void SetSeatState(bool conditions) => CanSeat = conditions;
}
