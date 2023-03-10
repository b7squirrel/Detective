using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInfo : MonoBehaviour
{
    [SerializeField] UnityEngine.UI.Image itemImage;
    [SerializeField] TMPro.TextMeshProUGUI itemName;

    public void SetInfo(ItemProperty itemProperty)
    {
        itemImage.sprite = itemProperty.sprite;
        itemName.text = itemProperty.name;
    }
}
