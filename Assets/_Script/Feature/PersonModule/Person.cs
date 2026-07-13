using System.Collections.Generic;   
using UnityEngine;

public class Person : MonoBehaviour
{
    [SerializeField] private PersonDataSO data;
    [SerializeField] private List<ConditionsSO> conditions;
    public PersonConditionCheck ConditionChecking { get; private set; }
    public string ID => data.ID;
    public List<Trait> Trait => data.Trait;

    public bool OutSide { get; private set; }
    public bool Seated { get; private set; }
    
    public void SetOutSideState(bool condition) => OutSide = condition;
    public void SetSeatedState(bool condition) => Seated = condition;

    public void SetCondition(List<ConditionsSO> condition)
    {
        conditions = condition;
        ConditionChecking.SwitchConditions(condition);
    }

    private void Awake()
    {
        ConditionChecking = new PersonConditionCheck(conditions);
    }
}
