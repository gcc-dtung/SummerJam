using System.Collections.Generic;   
using UnityEngine;

public class Person : MonoBehaviour
{
    [field:SerializeField] public string ID { get; private set; }
    [field:SerializeField] public TraitType Trait { get; private set; }
    public bool OutSide { get; private set; }
    public bool Seated { get; private set; }
    [SerializeField] private ConditionsSO conditions;

    public void SetOutSideState(bool condition) => OutSide = condition;
    public void SetSeatedState(bool condition) => Seated = condition;
    public bool IsHappy(List<Cell> adjacentCells)
    {
        if (conditions == null) return true;
        return IsSatisfiedWithFood(adjacentCells) && IsSatisfiedWithPerson(adjacentCells) && IsSatisfiedWithPersonTrait(adjacentCells);
    }
    private bool IsSatisfiedWithFood(List<Cell> adjacentCells)
    {
        if (conditions.Food.Like.Count == 0 && conditions.Food.Hate.Count == 0) return true;
        List<FoodType> foods = new List<FoodType>();
        foreach (var cell in adjacentCells)
        {
            if (cell is Dishes dishes)
            {
                foods.AddRange(dishes.Tags);
            }
        }
        foreach (var likedFood in conditions.Food.Like)
        {
            if (!foods.Contains(likedFood)) return false;
        }
        foreach (var hatedFood in conditions.Food.Hate)
        {
            if (foods.Contains(hatedFood)) return false;
        }
        return true;
    }
    private bool IsSatisfiedWithPersonTrait(List<Cell> adjacentCells)
    {
        if (conditions.Trait.Like.Count == 0 && conditions.Trait.Hate.Count == 0) return true;
        List<TraitType> traits = new List<TraitType>();
        foreach (var cell in adjacentCells)
        {
            if (cell is Seat seat && seat.CurrentPerson != null)
            {
                traits.Add(seat.CurrentPerson.Trait);
            }
        }

        foreach (var likeTrait in conditions.Trait.Like)
        {
            if (!traits.Contains(likeTrait)) return false;
        }        
        
        foreach (var hateTrait in conditions.Trait.Hate)
        {
            if (traits.Contains(hateTrait)) return false;
        }
        
        return true;
    }
    private bool IsSatisfiedWithPerson(List<Cell> adjacentCells)
    {
        if (conditions.PersonID.Like.Count == 0 && conditions.PersonID.Hate.Count == 0) return true;
        List<string> personIDs = new List<string>();
        foreach (var cell in adjacentCells)
        {
            if (cell is Seat seat && seat.CurrentPerson != null)
            {
                personIDs.Add(seat.CurrentPerson.ID);
            }
        }

        foreach (var likePerson in conditions.PersonID.Like)
        {
            if (!personIDs.Contains(likePerson)) return false;
        }        
        
        foreach (var hatePerson in conditions.PersonID.Hate)
        {
            if (personIDs.Contains(hatePerson)) return false;
        }
        
        return true;
    }
}
