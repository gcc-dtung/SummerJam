using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Conditions/LogicCondition")]
public class CompositeConditionsSO : ConditionsSO
{
    [SerializeField] private LogicalOperator logicalOperator;
    [SerializeField] private List<ConditionsSO> conditions = new List<ConditionsSO>();
    public override void GetConditionInfo(Cell currentCell, List<Cell> adjacency, List<ConditionInfo> results)
    {
        foreach (var cond in conditions)
        {
            if(cond != null) cond.GetConditionInfo(currentCell, adjacency, results);
        }
        if(logicalOperator != LogicalOperator.Or) return;
        if(!CheckCondition(currentCell, adjacency)) return;

        foreach (var cf in results)
        {
          cf.SetUpIsSatisfied(true);
        }
        
    }

    public override bool CheckCondition(Cell currentCell, List<Cell> adjacency)
    {
        if (conditions.Count == 0) return true;
        
        if (logicalOperator == LogicalOperator.And)
        {
            foreach (var cond in conditions)
            {
             if (cond!=null && !cond.CheckCondition(currentCell, adjacency)) return false;
            }
        }

        foreach (var cond in conditions)
        {
            if (cond!=null && cond.CheckCondition(currentCell, adjacency)) return true;
        }
        
        return false;
    }
}
