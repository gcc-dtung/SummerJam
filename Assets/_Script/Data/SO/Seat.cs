using UnityEngine;
using NaughtyAttributes;
[CreateAssetMenu(menuName = "CellData/Seat")]
public class Seat : CellDataSO
{
    [field: SerializeField] public override string Name { get; protected set; }
    [field: SerializeField] public override CellType Type { get; protected set; } = CellType.Seat;
    [field: SerializeField] public override bool DefaultCanSeat { get; protected set; } = true;
    [field: SerializeField] public override bool DefaultCanInteract { get; protected set; } = true;
    [field: SerializeField,ShowAssetPreview(128, 128)] public override Sprite sprite { get; protected set; }
    [field: SerializeField, Tooltip("Optional Cell prefab used to render this seat type. Falls back to GridManager's default template when empty.")]
    public Cell CellPrefab { get; private set; }
    [field: SerializeField] public Person DefaultPerson { get; private set; }

    public void OnValidate()
    {
        if (DefaultPerson != null) DefaultCanSeat = false;
    }
}
