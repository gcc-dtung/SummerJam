using System;
using UnityEngine;
[CreateAssetMenu(menuName = "Shop/ShopItem")]
public class ShopItemData : ScriptableObject
{
    [field: SerializeField] public Sprite ItemIcon { get; private set; }
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public int CostAmount { get; private set; }
    [field: SerializeField] public int Quantity { get; private set; }
    [field:SerializeField] public int PurchaseLimitPerDay { get; private set; }
    [field: SerializeField] public CurrencyType CostCurrency { get; private set; }
    [field: SerializeField] public RewardType RewardType { get; private set; }
}