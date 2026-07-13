using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(menuName = "CellData/Dish")]
public class Dishes : CellDataSO
{
    [field: SerializeField] public override string Name { get; protected set; }
    [field: SerializeField] public override CellType Type { get; protected set; } = CellType.Dish;
    [field: SerializeField] public override bool DefaultCanSeat { get; protected set; } = false;
    [field: SerializeField] public override bool DefaultCanInteract { get; protected set; } = false;
    [field: SerializeField,ShowAssetPreview(128, 128)] public override Sprite sprite { get; protected set; }
    [field: SerializeField] public List<Food> Tags { get; private set; }

    private void OnEnable()
    {
        if (Tags == null) Tags = new List<Food>();
    }
}