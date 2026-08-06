using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class WeeklyClaimManager : Singleton<WeeklyClaimManager>
{
   [SerializeField] private WeeklyClaim[] weeklyClaimHolders;
   [SerializeField] private WeekReward[] _weekRewards;
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
            weeklyClaimHolders[CurrentDay].SetupHadClaim();
         }
         else
         {
            weeklyClaimHolders[CurrentDay].SetUpCanClaim();
         }
      }
      
      for (int i = CurrentDay + 1; i < 7; i++)
      {
         weeklyClaimHolders[i].SetUpCannotClaim();
      }
   }
}

