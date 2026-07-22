using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShopSlot
{
   [field:SerializeField] public ShopItem Item { get; private set; }
   [field:SerializeField] public int PurchaseLimitPerDay { get; private set; }
   [field: SerializeField] public int PurchasedToday;
   [field:SerializeField] public string LastResetDate { get; private set; }
   public bool IsUnlimited => PurchaseLimitPerDay <= 0;
   public bool CanPurchase => IsUnlimited || PurchasedToday < PurchaseLimitPerDay;

   public void CheckAndResetDailyLimit()
   {
      string todayStr = DateTime.Now.ToString("yyyy-MM-dd");
      if (LastResetDate != todayStr)
      {
         PurchasedToday = 0;
         LastResetDate = todayStr;
      }
   }
}
