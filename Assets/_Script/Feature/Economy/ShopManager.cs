using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : Singleton<ShopManager>
{
    public bool TryPurchaseSlot(ShopSlot slot)
    {
        if (!slot.CanPurchase) {Debug.Log("Reached The Dayily Limit"); return false;}
        ShopItemData itemData = slot.ItemData;
        if (itemData.CostCurrency == CurrencyType.RealMoney && (itemData.RewardType == RewardType.MoveBooster || itemData.RewardType == RewardType.RemoveBooster || itemData.RewardType == RewardType.UndoBooster) )
        {
            Debug.LogError("Can't Buy ");
            return false;
        }

        bool paymentSuccess = false;
        switch (itemData.CostCurrency)
        {
            case CurrencyType.Gem:
              paymentSuccess =  EconomyManager.Instance.SpendGem((int)itemData.CostAmount);
                break;
            case CurrencyType.Gold:
               paymentSuccess = EconomyManager.Instance.SpendGold((int)itemData.CostAmount);
                break;
        }
        
        if(!paymentSuccess) {Debug.Log("Have EnoughMoney"); return false;}
        GrantReward(itemData.RewardType,itemData.Quantity);
        // OnSlotPurchasedSuccessfully?.Invoke(slot);
        if (!slot.IsUnlimited)
        {
            slot.PurchasedToday++;
            if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.GameData != null)
            {
                var gameData = SaveLoadManager.Instance.GameData;
                if (gameData.shopPurchasedCounts == null)
                {
                    gameData.shopPurchasedCounts = new Dictionary<int, int>();
                }
                if (slot.ItemData != null)
                {
                    gameData.shopPurchasedCounts[slot.ItemData.ID] = slot.PurchasedToday;
                }
                SaveLoadManager.Instance.SaveGame();
            }
        }
        return true;
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
