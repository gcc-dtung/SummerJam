using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Conditions/Condition")]
public class ConditionsSO : ScriptableObject
{
 [field: SerializeField] public ConditionWrapper<string> PersonID { get; private set; } = new ConditionWrapper<string>();
 [field: SerializeField] public ConditionWrapper<FoodType> Food { get; private set; } = new ConditionWrapper<FoodType>();
 [field: SerializeField] public ConditionWrapper<TraitType> Trait { get; private set; } = new ConditionWrapper<TraitType>();
 [field: SerializeField] public string Description { get; private set; }
}
[Serializable]
public class ConditionWrapper<T>
{
 [field: SerializeField] public List<T> Like { get; private set; } = new List<T>();
 [field: SerializeField] public List<T> Hate { get; private set; } = new List<T>();
}