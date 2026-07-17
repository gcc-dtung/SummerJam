using System;
using System.Collections.Generic;
using UnityEngine;

public class BoosterManager : MonoBehaviour
{
   [SerializeField] private MoreMoveBooster moveBooster;
   [SerializeField] private UndoBooster undoBooster;
   [SerializeField] private RemoveConditionBooster removeBooster;
   private Dictionary<Booster, int> boosterHolder = new Dictionary<Booster, int>();

   private void Awake()
   {
      foreach (Booster boost in Enum.GetValues(typeof(Booster)))
      {
         AddMoreBooster(boost);
      }
   }

   public void AddMoreBooster(Booster boost)
   {
      if(!boosterHolder.ContainsKey(boost)) boosterHolder.Add(boost,0);
      boosterHolder[boost]++;
   }


   public void Undo()
   {
      if(boosterHolder[Booster.Undo] <= 0) return;
      boosterHolder[Booster.Undo]--;
      undoBooster.Undo();
   }

   public void MoreMove()
   {
      if(boosterHolder[Booster.Move] <= 0) return;
      boosterHolder[Booster.Move]--;
      moveBooster.TakeMoreMove();
   }

   public void RemoveBooster()
   {
      if(boosterHolder[Booster.Remove] <= 0) return;
      boosterHolder[Booster.Remove]--;
      RemoveConditionBooster remover = Instantiate(removeBooster);
      remover.gameObject.SetActive(true);
   }
   
}

public enum Booster
{
   Move = 0,
   Undo = 1,
   Remove = 2
}