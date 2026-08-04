using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
   [SerializeField] private TextMeshProUGUI LimitedText;
   [SerializeField] private TextMeshProUGUI quantiText;
   [SerializeField] private Image image;
   [SerializeField] private Button purchaseButton;
   
   [field:SerializeField] public ShopItemData ItemData { get; private set; }
   
   public int PurchasedToday;
   private bool ReachedLimit = false;
   public bool IsUnlimited => ItemData.PurchaseLimitPerDay <= 0;
   public bool CanPurchase => IsUnlimited || PurchasedToday < ItemData.PurchaseLimitPerDay;
   
   public void SetItemData(ShopItemData data)
   {
      ItemData = data;
      CheckAndResetDailyLimit();
      Setup();
   }

   // private void OnApplicationFocus(bool hasFocus)
   // {
   //    if (hasFocus)
   //    {
   //       CheckAndResetDailyLimit();
   //    }
   // }

   private void Setup()
   {
      
      quantiText.text = "x" + ItemData.Quantity.ToString();
      image.sprite = ItemData.ItemIcon;
      if (PurchasedToday >= ItemData.PurchaseLimitPerDay)
      {
         ReachedLimit = true;
         purchaseButton.interactable = false; // sau tách hàm riêng từ đây
      }
      LimitedText.text = PurchasedToday.ToString()+"/"+ItemData.PurchaseLimitPerDay.ToString();
      
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
      purchaseButton.onClick.AddListener(PurchaseSlot);
   }

   private void OnDisable()
   {
      purchaseButton.onClick.RemoveAllListeners();
   }

   private void PurchaseSlot()
   {
      if(ReachedLimit) return;
      if (!ShopManager.TryPurchaseSlot(this))
      {
         if (PurchasedToday >= ItemData.PurchaseLimitPerDay)
         {
            ReachedLimit = true;
            purchaseButton.interactable = false; // sau tách hàm riêng từ đây
         }

         return;
      }
      LimitedText.text = PurchasedToday.ToString()+"/"+ItemData.PurchaseLimitPerDay.ToString();
   }
   
   
   
}
