using System.Collections.Generic;
using UnityEngine;

public abstract class ConditionsSO : ScriptableObject
{
   public abstract void GetConditionInfo(Cell currentCell, List<Cell> adjacency,List<ConditionInfo> results);
   public abstract bool CheckCondition(Cell currentCell, List<Cell> adjacency);
}
