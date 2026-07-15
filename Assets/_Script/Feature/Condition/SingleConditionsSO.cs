using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Conditions/SingleCondition")]
public class SingleConditionsSO : ConditionsSO
{
    [field: SerializeField] public Scope Scope { get; private set; }
    [field: SerializeField] public Target FilterTarget { get; private set; }
    [field: SerializeField] public Filter Filter { get; private set; }
    [field: SerializeField] public Comparator Comparator { get; private set; }
    [field: SerializeField] public int Value { get; private set; }
    [field: SerializeField] public string Description { get; private set; }

    public override void GetConditionInfo(Cell currentCell, List<Cell> adjacency, List<ConditionInfo> results)
    {
        if(string.IsNullOrEmpty(Description)) return;
        results.Add(new ConditionInfo()
        {
            Description = this.Description,
            IsSatisfied =  CheckCondition(currentCell, adjacency)
        });        
    }

    public override bool CheckCondition(Cell currentCell, List<Cell> adjacency)
    {
        int matchCount = 0;
        Debug.Log($"{Scope} {matchCount}");
        if (Scope == Scope.Self)
        {
            if (EvaluateMatch(currentCell))
            {
                matchCount = 1;
            }
        }
        else if (Scope == Scope.Adjacent)
        {
            foreach (var cell in adjacency)
            {
                if (EvaluateMatch(cell))
                {
                    matchCount++;
                }
            }
        }

        return Comparator switch
        {
            Comparator.Exact => matchCount == Value,
            Comparator.AtLeast => matchCount >= Value,
            Comparator.AtMost => matchCount <= Value,
            _ => false
        };
    }
    private bool EvaluateMatch(Cell cell)
    {
        if (cell == null || cell.Type == CellType.Block) return false;
        if (FilterTarget == Target.Cell)
        {
            if (Filter.Column != -1 && cell.X != Filter.Column) return false;
            if (Filter.Row != -1 && cell.Y != Filter.Row) return false;
            return true;
        }
        
        if (FilterTarget == Target.Person)
        {
            if (cell.CurrentPerson == null) return false;
            if (!string.IsNullOrEmpty(Filter.Name))
            {
                if (cell.CurrentPerson.ID != Filter.Name) return false; 
            }
            foreach (var requiredTrait in Filter.TraitTags)
            {
                if (!cell.CurrentPerson.Trait.Contains(requiredTrait)) return false;
            }
            return true;
        }
        
        if (FilterTarget == Target.Dish)
        {
            if (cell.Type != CellType.Dish) return false;
            Dishes dish = cell.Data as Dishes;
            if (dish == null) return false;
            foreach (var requiredFood in Filter.FoodTags)
            {
                if (!dish.Tags.Contains(requiredFood)) return false;
            }
            return true;
        }
        return false;
    }
    
}


[Serializable]
public class Filter
{
   [field:SerializeField] public string Name { get; private set; }
   [field:SerializeField] public List<Food> FoodTags { get; private set; } = new List<Food>();
   [field:SerializeField] public List<Trait> TraitTags { get; private set; } = new List<Trait>();
   [field:SerializeField] public int Row { get; private set; }
   [field:SerializeField] public int Column { get; private set; }
}


