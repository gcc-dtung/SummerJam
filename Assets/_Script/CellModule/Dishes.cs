using System.Collections.Generic;
using UnityEngine;

public class Dishes : Cell
{
    public override CellType Type { get; } = CellType.Dish;
    public override bool CanSeat { get; protected set; } = false;
    public override bool CanInteract { get; protected set; } = false;
    [field: SerializeField] public List<FoodType> Tags { get; private set; }
}
