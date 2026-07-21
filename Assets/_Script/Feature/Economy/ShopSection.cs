using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class ShopSection
{
    [field:SerializeField] public string SectionTitle { get; private set; }
    [field:SerializeField] public List<ShopSlot> Slots { get; private set; } = new List<ShopSlot>();
}
