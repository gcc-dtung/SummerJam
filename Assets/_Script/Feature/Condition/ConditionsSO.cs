using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Conditions/Condition")]
public class ConditionsSO : ScriptableObject
{ 
    [field: SerializeField] public ConditionType Type { get; private set; }
    [field: SerializeField] public Target Target { get; private set; }
    [field: SerializeField] public List<Food> DishTags { get; private set; }
    [field: SerializeField] public List<Trait> PersonTags { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
}


