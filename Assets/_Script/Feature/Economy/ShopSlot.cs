using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
   [SerializeField] private Button purchaseButton;
   [field:SerializeField] public ShopItemData ItemData { get; private set; }
   [field:SerializeField] public int PurchaseLimitPerDay { get; private set; }
   [field: SerializeField] public int PurchasedToday;
   [field:SerializeField] public string LastResetDate { get; private set; }
   public bool IsUnlimited => PurchaseLimitPerDay <= 0;
   public bool CanPurchase => IsUnlimited || PurchasedToday < PurchaseLimitPerDay;

   private void CheckAndResetDailyLimit()
   {
      string todayStr = DateTime.Now.ToString("yyyy-MM-dd");
      if (LastResetDate != todayStr)
      {
         PurchasedToday = 0;
         LastResetDate = todayStr;
      }
   }


   private void OnEnable()
   {
      purchaseButton.onClick.AddListener(() => { ShopManager.Instance.TryPurchaseSlot(this);});
   }

   private void OnDisable()
   {
      purchaseButton.onClick.RemoveAllListeners();
   }
   
   
}
