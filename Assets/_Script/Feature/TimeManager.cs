using System;
using System.Collections;
using UnityEngine;

public class TimeManager : Singleton<TimeManager>
{
   public DateTime LastTime;
   public bool IsNextDay { get; private set; }

   private void Start()
   {
      StartCoroutine(TimeCheck());
   }

   public void NextToTomorrow()
   {
      Debug.Log(LastTime.Date + " " + DateTime.Now.Date);
      IsNextDay = (LastTime.Date != DateTime.Now.Date);
   }

   IEnumerator TimeCheck()
   {
      while (true)
      {
         yield return new WaitForSeconds(60);
         IsNextDay = (LastTime.Date != DateTime.Now.Date);
      }
   }
}
