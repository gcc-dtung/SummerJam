using System;
using UnityEngine;

public class ShopManager : Singleton<ShopManager>
{
    [field:SerializeField] public Shop ShopData { get; private set; }
    public event Action<ShopSlot> OnSlotPurchasedSuccessfully;

    public void CheckAllDailyLimit()
    {
        if(ShopData == null) return;
        foreach (var slot in ShopData.GoldSection.Slots)
            slot.CheckAndResetDailyLimit();

        foreach (var slot in ShopData.GemSection.Slots)
            slot.CheckAndResetDailyLimit();

        foreach (var slot in ShopData.IAPSection.Slots)
            slot.CheckAndResetDailyLimit();
    }

    public bool TryPurchaseSlot(ShopSlot slot)
    {
        slot.CheckAndResetDailyLimit();
        if (!slot.CanPurchase) {Debug.Log("Reached The Dayily Limit"); return false;}
        ShopItemData itemData = slot.ItemData;
        if (itemData.CostCurrency == CurrencyType.RealMoney && itemData.RewardType == RewardType.Booster)
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
        OnSlotPurchasedSuccessfully?.Invoke(slot);
        if (!slot.IsUnlimited)
        {
            slot.PurchasedToday++;
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
        }
    }
    
}
