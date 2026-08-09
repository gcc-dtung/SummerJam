using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
   [SerializeField] private TextMeshProUGUI amountText;
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

   private void OnApplicationFocus(bool hasFocus)
   {
      if (hasFocus)
      {
         CheckAndResetDailyLimit();
         Setup();
      }
   }

   private void Setup()
   {
      if(ItemData == null) return;
      quantiText.text = "x" + ItemData.Quantity.ToString();
      amountText.text = ItemData.CostAmount.ToString();
      image.sprite = ItemData.ItemIcon;
      if (!IsUnlimited && PurchasedToday >= ItemData.PurchaseLimitPerDay)
      {
         ReachedLimit = true;
         purchaseButton.interactable = false;
      }
      else
      {
         ReachedLimit = false;
         purchaseButton.interactable = true;
      }
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
            if (gameData.shopPurchasedCounts != null && ItemData != null)
            {
               if (gameData.shopPurchasedCounts.TryGetValue(ItemData.ID, out int count))
               {
                  PurchasedToday = count;
               }
               else
               {
                  PurchasedToday = 0;
               }
            }
         }
      }
   }


   private void OnEnable()
   {
      purchaseButton.onClick.AddListener(PurchaseSlot);
      if (TimeManager.Instance != null)
      {
         TimeManager.Instance.OnNewDay += HandleNewDay;
      }
   }

   private void OnDisable()
   {
      purchaseButton.onClick.RemoveAllListeners();
      if (TimeManager.Instance != null)
      {
         TimeManager.Instance.OnNewDay -= HandleNewDay;
      }
   }

   private void HandleNewDay()
   {
      CheckAndResetDailyLimit();
      Setup();
   }

   private void PurchaseSlot()
   {
      if(ReachedLimit) return;
      if (!ShopManager.TryPurchaseSlot(this))
      {
         if (PurchasedToday >= ItemData.PurchaseLimitPerDay)
         {
            ReachedLimit = true;
            purchaseButton.interactable = false;
         }

         return;
      }
   }
   
   
   
}
