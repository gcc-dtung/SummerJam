using UnityEngine;
[CreateAssetMenu(menuName = "WeekReward/Reward")]
public class WeekReward : ScriptableObject
{
   [field:SerializeField] public Sprite icon { get; private set; }
   [field:SerializeField] public int Quanty { get; private set; }
   [field: SerializeField] public RewardType RewardType { get; private set; }
}
