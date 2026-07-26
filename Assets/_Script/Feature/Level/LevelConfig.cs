using UnityEngine;
[CreateAssetMenu(menuName = "Level/LevelConfig")]
public class LevelConfig : ScriptableObject
{
   [field:SerializeField] public int ID { get; private set; }
   [field:SerializeField] public int MoveLimit { get; private set; }
   [field:SerializeField] public int Gold { get; private set; }
   [field:SerializeField] public GameObject Layout { get; private set; }
   [field:SerializeField] public GridConfig BoardGrid { get; private set; }
   [field:SerializeField] public GridConfig WaitLineGrid { get; private set; }
}
