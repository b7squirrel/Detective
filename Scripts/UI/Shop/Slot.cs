using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Slot : MonoBehaviour
{
    [HideInInspector] public ItemProperty item;
    [SerializeField] UnityEngine.UI.Image image;
    [SerializeField] TMPro.TextMeshProUGUI itemName;
    [SerializeField] TMPro.TextMeshProUGUI price;

    public void SetItem(ItemProperty item)
    {
        this.item = item;
        if(item == null)
        {
            image.enabled = false;
            gameObject.name = "Empty";
            itemName.text = "Empty";
        }
        else
        {
            image.enabled = true;

            gameObject.name = item.name;
            image.sprite = item.sprite;
            itemName.text = item.name;
            price.text = item.price.ToString();
        }
    }
}
