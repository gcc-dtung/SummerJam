using System;
using System.Collections;
using UnityEngine;

public class TimeManager : Singleton<TimeManager>
{
   public DateTime LastTime;
   public bool IsNextDay { get; private set; }
   public event Action OnNewDay;

   private void Start()
   {
      StartCoroutine(TimeCheck());
   }

   private void OnApplicationFocus(bool hasFocus)
   {
      if (hasFocus)
      {
         Debug.Log($"[TimeManager] Focus gained. LastTime: {LastTime.Date}, CurrentTime: {DateTime.Now.Date}");
         if (LastTime.Date != DateTime.Now.Date)
         {
            TriggerDailyReset();
         }
      }
   }

   private void OnApplicationPause(bool pauseStatus)
   {
      if (!pauseStatus)
      {
         Debug.Log($"[TimeManager] App resumed. LastTime: {LastTime.Date}, CurrentTime: {DateTime.Now.Date}");
         if (LastTime.Date != DateTime.Now.Date)
         {
            TriggerDailyReset();
         }
      }
   }

   public void NextToTomorrow()
   {
      Debug.Log(LastTime.Date + " " + DateTime.Now.Date);
      IsNextDay = (LastTime.Date != DateTime.Now.Date);
      if (IsNextDay)
      {
         TriggerDailyReset();
      }
   }

   IEnumerator TimeCheck()
   {
      while (true)
      {
         yield return new WaitForSeconds(1f);
         if (LastTime.Date != DateTime.Now.Date)
         {
            TriggerDailyReset();
         }
      }
   }

   private void TriggerDailyReset()
   {
      IsNextDay = true;
      Debug.Log($"[TimeManager] TriggerDailyReset started.");
      if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.GameData != null)
      {
         var gameData = SaveLoadManager.Instance.GameData;
         if (gameData.shopPurchasedCounts != null)
         {
            gameData.shopPurchasedCounts.Clear();
         }
         int nextDay = (gameData.CurrentWeekReward + 1) % 7;
         bool nextClaim = false;
         if (WeeklyClaimManager.Instance != null)
         {
            WeeklyClaimManager.Instance.InitializeData(nextDay, nextClaim);
         }
         else
         {
            gameData.CurrentWeekReward = nextDay;
            gameData.HadClaimWeekReward = nextClaim;
         }
         SaveLoadManager.Instance.SaveGame();
      }
      LastTime = DateTime.Now;
      IsNextDay = false;
      OnNewDay?.Invoke();
   }
}
