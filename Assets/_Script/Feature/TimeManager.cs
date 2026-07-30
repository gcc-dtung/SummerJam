using System;
using UnityEngine;

public class TimeManager : Singleton<TimeManager>
{
   public DateTime LastTime;
   public bool IsNextDay { get; private set; }

   public void NextToTomorrow()
   {
      Debug.Log(LastTime.Date + " " + DateTime.Now.Date);
      IsNextDay = (LastTime.Date != DateTime.Now.Date);
   }
}
