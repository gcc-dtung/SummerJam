using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
   [SerializeField] private Button purchaseButton;
   [field:SerializeField] public ShopItemData ItemData { get; private set; }
   [field:SerializeField] public int PurchaseLimitPerDay { get; private set; }
   public int PurchasedToday;
   public bool IsUnlimited => PurchaseLimitPerDay <= 0;
   public bool CanPurchase => IsUnlimited || PurchasedToday < PurchaseLimitPerDay;

   private void Start()
   {
      CheckAndResetDailyLimit();
   }

   
   private void CheckAndResetDailyLimit()
   {
      if (TimeManager.Instance.IsNextDay)
      {
         PurchasedToday = 0;
         if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.GameData != null && SaveLoadManager.Instance.GameData.shopPurchasedCounts != null && ItemData != null)
         {
            SaveLoadManager.Instance.GameData.shopPurchasedCounts[ItemData.ID] = 0;
         }
      }
      else
      {
         if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.GameData != null)
         {
            var gameData = SaveLoadManager.Instance.GameData;
            if (gameData.shopPurchasedCounts != null && ItemData != null && gameData.shopPurchasedCounts.TryGetValue(ItemData.ID, out int count))
            {
               PurchasedToday = count;
            }
         }
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
