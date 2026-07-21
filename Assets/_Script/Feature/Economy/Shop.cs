using System;
using UnityEngine;
[Serializable] 
public class Shop
{
  [field:SerializeField] public ShopSection GoldSection { get; private set; }
  [field:SerializeField] public ShopSection GemSection { get; private set; }
  [field:SerializeField] public ShopSection IAPSection { get; private set; }
}
