using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipInfoPanel : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI grade;
    [SerializeField] TMPro.TextMeshProUGUI Name;
    [SerializeField] UnityEngine.UI.Image itemImage;
    [SerializeField] GameObject equipButton, unEquipButton;

    public void SetPanel(CardData cardData, bool isEquipButton)
    {
        grade.text = cardData.Grade;
        Name.text = cardData.Name;
        
        CardsDictionary cardsDictionary = FindAnyObjectByType<CardsDictionary>();
        WeaponItemData weaponItemData = cardsDictionary.GetWeaponItemData(cardData);
        itemImage.sprite = weaponItemData.itemData.charImage;

        equipButton.SetActive(isEquipButton);
        unEquipButton.SetActive(!isEquipButton);
    }
}
