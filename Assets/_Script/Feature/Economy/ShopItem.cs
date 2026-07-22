using System;
using UnityEngine;

[Serializable]
public class ShopItem
{
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public CurrencyType CostCurrency { get; private set; }
    [field: SerializeField] public int CostAmount { get; private set; }
    [field: SerializeField] public RewardType RewardType { get; private set; }
    [field: SerializeField] public int Quantity { get; private set; }
    public Sprite ItemIcon;
}