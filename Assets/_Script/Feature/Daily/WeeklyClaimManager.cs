using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class WeeklyClaimManager : Singleton<WeeklyClaimManager>
{
   [SerializeField] private WeeklyClaim[] weeklyClaimHolders;
   [SerializeField] private WeekReward[] _weekRewards;
   [SerializeField] private Claim claimButton;
   public int CurrentDay { get; private set; }
   public bool HadClaimedToday { get; private set; }
   protected override void Awake()
   {
      base.Awake();
      for (int i = 0; i < 7; i++)
      {
         weeklyClaimHolders[i].Init(_weekRewards[i]);
      }
   }

   private void OnEnable()
   {
      if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.GameData != null)
      {
         InitializeData(SaveLoadManager.Instance.GameData.CurrentWeekReward, SaveLoadManager.Instance.GameData.HadClaimWeekReward);
      }
      SetUpLayout();

      if (TimeManager.Instance != null)
      {
         TimeManager.Instance.OnNewDay += HandleNewDay;
      }
   }

   private void OnDisable()
   {
      if (TimeManager.Instance != null)
      {
         TimeManager.Instance.OnNewDay -= HandleNewDay;
      }
   }

   private void HandleNewDay()
   {
      if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.GameData != null)
      {
         CurrentDay = SaveLoadManager.Instance.GameData.CurrentWeekReward;
         HadClaimedToday = SaveLoadManager.Instance.GameData.HadClaimWeekReward;
      }
      SetUpLayout();
   }
   
   public void InitializeData(int savedDay, bool savedHadClaimed)
   {
      CurrentDay = savedDay;
      HadClaimedToday = savedHadClaimed;
   }
   
   public void ClaimCurrentDay()
   {
      HadClaimedToday = true;
      weeklyClaimHolders[CurrentDay].SetupHadClaim();
      GrantReward(_weekRewards[CurrentDay].RewardType,_weekRewards[CurrentDay].Quanty);
      if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.GameData != null)
      {
         SaveLoadManager.Instance.GameData.HadClaimWeekReward = true;
         SaveLoadManager.Instance.SaveGame();
      }
   }

   private void SetUpLayout()
   {
      for (int i = 0; i < CurrentDay; i++)
      {
         weeklyClaimHolders[i].SetupHadClaim();
      }
      
      if (CurrentDay < 7)
      {
         if (HadClaimedToday)
         {
            claimButton.SetUpHadClaim();
            weeklyClaimHolders[CurrentDay].SetupHadClaim();
         }
         else
         {
            claimButton.SetUpHadnotClaim();
            weeklyClaimHolders[CurrentDay].SetUpCanClaim();
         }

         for (int i = CurrentDay + 1; i < 7; i++)
         {
            weeklyClaimHolders[i].SetUpCanClaim();
         }
      }
   }
   
   private void GrantReward(RewardType type, int quantity)
   {
      switch (type)
      {
         case RewardType.Gold:
            EconomyManager.Instance.GetGold(quantity);
            break;
         case RewardType.Gem:
            EconomyManager.Instance.GetGem(quantity);
            break;
         case RewardType.MoveBooster:
            BoosterManager.Instance.AddMoreBooster(Booster.Move,quantity);
            break; 
         case RewardType.RemoveBooster:
            BoosterManager.Instance.AddMoreBooster(Booster.Remove,quantity);
            break; 
         case RewardType.UndoBooster:
            BoosterManager.Instance.AddMoreBooster(Booster.Undo,quantity);
            break;
      }
   }
}

