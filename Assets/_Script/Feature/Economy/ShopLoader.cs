using System;
using UnityEngine;

public class ShopLoader : MonoBehaviour
{
   [SerializeField] private ShopItemData[] _itemDatas;
   [SerializeField] private ShopSlot prefab;
   [SerializeField] private Transform scrollviewPosition;

   private void Start()
   {
       for (int i = 0; i < _itemDatas.Length; i++)
       {
           ShopSlot shop = Instantiate<ShopSlot>(prefab,scrollviewPosition);
           shop.SetItemData(_itemDatas[i]);
       }
   }
}
