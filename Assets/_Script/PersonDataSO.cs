using UnityEngine;

[CreateAssetMenu(menuName = "PersonData/Data")]
public class PersonDataSO : ScriptableObject
{
    [field:SerializeField] public string ID { get; private set; }
    [field:SerializeField] public TraitType Trait { get; private set; }
}
