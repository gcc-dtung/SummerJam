using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
[CreateAssetMenu(menuName = "CellData/Block")]
public class Blocked : CellDataSO
{
    [field: SerializeField] public override string Name { get; protected set; } = "Blocked";
    [field: SerializeField] public override CellType Type { get; protected set; } = CellType.Block;
    [field: SerializeField] public override bool DefaultCanSeat { get; protected set; } = false;
    [field: SerializeField] public override bool DefaultCanInteract { get; protected set; } = false;
    [field: SerializeField,ShowAssetPreview(128, 128)] public override Sprite sprite { get; protected set; }
}