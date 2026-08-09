using System;
using UnityEngine;

public class ShopLoader : MonoBehaviour
{
   [SerializeField] private ShopItemData[] _itemDatas;
   [SerializeField] private ShopSlot[] shopSlots;

   private void Awake()
   {
       for (int i = 0; i < _itemDatas.Length; i++)
       {
           shopSlots[i].SetItemData(_itemDatas[i]);
       }
   }
}
