using UnityEngine;

public class Blocked : Cell
{
    public override CellType Type { get; } = CellType.Block;
    public override bool CanSeat { get; protected set; } = false;
    public override bool CanInteract { get; protected set; } = false;
}
