using UnityEngine;

[CreateAssetMenu(menuName = "MoveData/Data")]
public class MoveDataSO : ScriptableObject
{
    [field:SerializeField] public int Limit { get; private set; }
}
