using System.Collections.Generic;
using UnityEngine;

public class PersonConditionCheck
{
    private List<ConditionsSO> conditions;
    public bool IsHappy { get; private set; }

    public PersonConditionCheck(List<ConditionsSO> conditions)
    {
        this.conditions = conditions;
    }

    public void SwitchConditions(List<ConditionsSO> conditions)
    {
        this.conditions = conditions;
    }

    public void CheckConditions(List<Cell> adjacency)
    {
        IsHappy = true;
        foreach (var condition in conditions)
        {
            if (!IsConditionSatisfied(condition, adjacency))
            {
                IsHappy = false;
                return;
            }
        }
    }
    
    private bool IsConditionSatisfied(ConditionsSO condition, List<Cell> adjacency)
    {
        bool hasMatchingAdjacent = false;
        foreach (var cell in adjacency)
        {
            if (cell == null || cell.Type == CellType.Block) continue;
            if (EvaluateCellMatch(cell, condition))
            {
                hasMatchingAdjacent = true;
                if (condition.Type == ConditionType.Like) 
                {
                    return true; 
                }
            }
        }
        if (condition.Type == ConditionType.Hate)
        {
            return !hasMatchingAdjacent;
        }
        return false;
    }
    
    private bool EvaluateCellMatch(Cell cell, ConditionsSO condition)
    {
        if (condition.Target == Target.Dish && cell.Type == CellType.Dish)
        {
            if (condition.DishTags == null || condition.DishTags.Count == 0) return false;
            Dishes dish = cell.Data as Dishes;

            if (dish == null)
            {
                Debug.LogError("One of the (Dish) conditionSOs had incorrect data pulled. ");
                return false;
            }
            foreach (var requiredTag in condition.DishTags)
            {
                if (!dish.Tags.Contains(requiredTag)) return false;
            }
            return true;
        }
        if (condition.Target == Target.Person && cell.Type == CellType.Seat)
        {
            if (cell.CurrentPerson == null || condition.PersonTags == null || condition.PersonTags.Count == 0) return false;
            foreach (var requiredTag in condition.PersonTags)
            {
                if (!cell.CurrentPerson.Trait.Contains(requiredTag)) return false;
            }
            return true;
        }

        return false;
    }


}