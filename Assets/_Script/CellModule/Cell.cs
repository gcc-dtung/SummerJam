using UnityEngine;

public abstract class Cell : MonoBehaviour
{
    public int X { get; protected set; }
    public int Y { get; protected set; }
    public abstract CellType Type { get; }
    public abstract bool CanSeat { get; protected set; }
    public abstract bool CanInteract { get; protected set; }

    public void SetGridIndex(int X, int Y)
    {
        this.X = X;
        this.Y = Y;
    }
    
}
