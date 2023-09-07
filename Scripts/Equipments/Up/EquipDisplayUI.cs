using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipDisplayUI : MonoBehaviour
{
    [SerializeField] Image charImage;
    [SerializeField] CardsDictionary cardDictionary;

    public void SetWeaponDisply(CardData cardData)
    {
        WeaponData wd = cardDictionary.GetWeaponData(cardData);
        charImage.sprite = wd.charImage;
    }
    public void SetItemDisplay(CardData cardData)
    {
        Item item = cardDictionary.GetItemData(cardData);
        charImage.sprite = item.charImage;
    }
}
