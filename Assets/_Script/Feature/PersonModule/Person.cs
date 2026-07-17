using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Person : MonoBehaviour
{
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

    private void Start()
    {
        if (conditions != null)
        {
            conditions = Instantiate(conditions);
            conditions?.GetConditionInfo(ConditionStatus);
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

            string icon = info.IsSatisfied
                ? "<color=#4CAF50>\u2611</color>"   // ☑ xanh lá
                : "<color=#E53935>\u2612</color>";  // ☒ đỏ
            //TODO : Sau sửa = TMP Sprite Asset để dùng ảnh
            sb.Append(icon).Append(" ").Append(info.Description);

            if (i < ConditionStatus.Count - 1)
                sb.Append("\n");
        }

        return sb.ToString();
    }
}
