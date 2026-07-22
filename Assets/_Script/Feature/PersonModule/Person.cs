using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Person : MonoBehaviour
{
    private const string SatisfiedIcon = "<sprite=\"TooltipTick\" name=\"TooltipTick\">";
    private const string UnsatisfiedIcon = "<sprite=\"TooltipO\" name=\"TooltipO\">";

    [SerializeField] private PersonDataSO data;
    [SerializeField] private ConditionsSO conditions;
    public List<ConditionInfo> ConditionStatus { get; private set; } = new List<ConditionInfo>();
    public bool IsHappy { get; private set; }

    public string ID => data.ID;
    public string Name => data.Name;
    public List<Trait> Trait => data.Trait;

    public bool OutSide { get; private set; }
    public bool Seated { get; private set; }
    
    public void SetOutSideState(bool condition) => OutSide = condition;
    public void SetSeatedState(bool condition) => Seated = condition;
    
    public void ResetValue()
    {
        ConditionStatus.Clear();
        if (conditions != null)
        {
            conditions = Instantiate(conditions);
            conditions?.ResetConditionInfo(ConditionStatus);
        }
    }

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
        conditions.GetConditionInfo(ConditionStatus);
    }
    
    public void SetCondition(ConditionsSO condition)
    {
        ConditionStatus.Clear();
        conditions = condition;
        if (conditions != null)
        {
            conditions = Instantiate(conditions);
            conditions?.GetConditionInfo(ConditionStatus);
        }
    }
    public string BuildTooltipContent()
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < ConditionStatus.Count; i++)
        {
            ConditionInfo info = ConditionStatus[i];
            sb.Append(info.IsSatisfied ? SatisfiedIcon : UnsatisfiedIcon)
                .Append(" ")
                .Append(info.Description);

            if (i < ConditionStatus.Count - 1)
                sb.Append("\n");
        }

        return sb.ToString();
    }
}
