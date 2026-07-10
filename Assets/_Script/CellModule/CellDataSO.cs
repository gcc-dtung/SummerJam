using UnityEngine;

public abstract class CellDataSO : ScriptableObject
{
    public abstract CellType Type { get; protected set; }
    public abstract bool DefaultCanSeat { get; protected set; }
    public abstract bool DefaultCanInteract { get; protected set; }
}


