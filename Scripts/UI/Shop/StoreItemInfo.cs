using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreItemInfo : ItemInfo
{
    [SerializeField] TMPro.TextMeshProUGUI itemPrice;
    public override void SetInfo(ItemProperty itemProperty)
    {
        base.SetInfo(itemProperty);
        itemPrice.text = itemProperty.price.ToString();
    }
}
