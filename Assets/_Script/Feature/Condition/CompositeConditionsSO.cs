using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Conditions/LogicCondition")]
public class CompositeConditionsSO : ConditionsSO
{
    [SerializeField] private LogicalOperator logicalOperator;
    [SerializeField] private List<ConditionsSO> conditions = new List<ConditionsSO>();
    public override void GetConditionInfo( List<ConditionInfo> results)
    {
        int startIndex = results.Count;
        foreach (var cond in conditions)
        {
            if(cond != null) cond.GetConditionInfo(results);
        }
        //
        // if(logicalOperator != LogicalOperator.Or) return;
        //
        // for (int i = startIndex; i < results.Count; i++)
        // {
        //     var cf = results[i];
        //     cf.IsSatisfied = true;
        //     results[i] = cf;
        // }
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

            return true;
        }

        foreach (var cond in conditions)
        {
            if (cond!=null && cond.CheckCondition(currentCell, adjacency)) return true;
        }
        
        return false;
    }
}
