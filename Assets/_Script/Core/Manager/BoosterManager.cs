using System;
using System.Collections.Generic;
using UnityEngine;

public class BoosterManager : Singleton<BoosterManager>
{
   [SerializeField] private MoreMoveBooster moveBooster;
   [SerializeField] private UndoBooster undoBooster;
   private Dictionary<Booster, int> boosterHolder = new Dictionary<Booster, int>();

   protected override void Awake()
   {
      base.Awake();
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
      if (UndoManager.Instance.TryUndoMove())
      {
         boosterHolder[Booster.Undo]--;
      }
   }

   public void MoreMove()
   {
      if(boosterHolder[Booster.Move] <= 0) return;
     if(MoveManager.Instance.TryIncreaseMove())
        boosterHolder[Booster.Move]--;
   }

   public bool CanRemove()
   {
      if(boosterHolder[Booster.Remove] <= 0) return false;
      return true;
   }

   public void RemoveHandle()
   {
      boosterHolder[Booster.Remove]--;
   }
   
}

public enum Booster
{
   Move = 0,
   Undo = 1,
   Remove = 2
}