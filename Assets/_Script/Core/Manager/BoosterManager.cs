using System;
using System.Collections.Generic;
using UnityEngine;

public class BoosterManager : Singleton<BoosterManager>
{
   private Dictionary<Booster, int> boosterHolder = new Dictionary<Booster, int>();
   public Dictionary<Booster, int> BoosterHolder
   {
      get => boosterHolder;
      set => boosterHolder = value;
   }
   protected override void Awake()
   {
      base.Awake();
      if (boosterHolder.Count == 0)
      {
         foreach (Booster boost in Enum.GetValues(typeof(Booster)))
         {
            AddMoreBooster(boost);
         }
      }
   }

   public void AddMoreBooster(Booster boost)
   {
      if(!boosterHolder.ContainsKey(boost)) boosterHolder.Add(boost,0);
      boosterHolder[boost]++;
      if (SaveLoadManager.Instance != null) SaveLoadManager.Instance.SaveGame();
   }


   public void Undo()
   {
      if(boosterHolder[Booster.Undo] <= 0) return;
      if (UndoManager.Instance.TryUndoMove())
      {
         boosterHolder[Booster.Undo]--;
         if (SaveLoadManager.Instance != null) SaveLoadManager.Instance.SaveGame();
      }
   }

   public void MoreMove()
   {
      if(boosterHolder[Booster.Move] <= 0) return;
     if(MoveManager.Instance.TryIncreaseMove())
     {
        boosterHolder[Booster.Move]--;
        if (SaveLoadManager.Instance != null) SaveLoadManager.Instance.SaveGame();
     }
   }

   public bool CanRemove()
   {
      if(boosterHolder[Booster.Remove] <= 0) return false;
      return true;
   }

   public void RemoveHandle()
   {
      boosterHolder[Booster.Remove]--;
      if (SaveLoadManager.Instance != null) SaveLoadManager.Instance.SaveGame();
   }
   
}

public enum Booster
{
   Move = 0,
   Undo = 1,
   Remove = 2
}