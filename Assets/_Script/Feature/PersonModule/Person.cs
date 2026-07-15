using System.Collections.Generic;   
using UnityEngine;

public class Person : MonoBehaviour
{
    [SerializeField] private PersonDataSO data;
    [SerializeField] private ConditionsSO conditions;
    public List<ConditionInfo> ConditionStatus { get; private set; } = new List<ConditionInfo>();
    public bool IsHappy { get; private set; }

    public string ID => data.ID;
    public List<Trait> Trait => data.Trait;

    public bool OutSide { get; private set; }
    public bool Seated { get; private set; }
    
    public void SetOutSideState(bool condition) => OutSide = condition;
    public void SetSeatedState(bool condition) => Seated = condition;

    public void CheckConditions(Cell currentCell,List<Cell> adjacency)
    {
        ConditionStatus.Clear();
        if(conditions == null)
        {
            IsHappy = true;
            return;
        }
        if (conditions.CheckCondition(currentCell, adjacency)) IsHappy = true;
        else IsHappy = false;
        conditions.GetConditionInfo(currentCell,adjacency,ConditionStatus);
    }
    
    public void SetCondition(ConditionsSO condition)
    {
        conditions = condition;
    }
    
}
