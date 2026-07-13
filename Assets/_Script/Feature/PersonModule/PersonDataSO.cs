using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PersonData/Data")]
public class PersonDataSO : ScriptableObject
{
    [field:SerializeField] public string Name { get; private set; }
    [field:SerializeField] public string ID { get; private set; }
    [field:SerializeField] public List<Trait> Trait { get; private set; }
}
    