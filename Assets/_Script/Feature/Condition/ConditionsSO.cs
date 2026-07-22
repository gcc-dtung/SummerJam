using System.Collections.Generic;
using UnityEngine;

public abstract class ConditionsSO : ScriptableObject
{
   public abstract void ResetConditionInfo(List<ConditionInfo> results);
   public abstract void GetConditionInfo(List<ConditionInfo> results);
   public abstract bool CheckCondition(Cell currentCell, List<Cell> adjacency);
}
