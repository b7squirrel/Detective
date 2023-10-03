using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentsAnimators : MonoBehaviour
{
    [SerializeField] CardList cardList;
    [SerializeField] CardsDictionary cardsDictionary;

    public RuntimeAnimatorController[] GetEquipmentAnimators(CardData charCardData, CardSlot targetSlot)
    {
        Animator[] equipAnims = targetSlot.GetComponent<CardDisp>().GetEquipmentAnimators();
        RuntimeAnimatorController[] animCons = new RuntimeAnimatorController[4];

        EquipmentCard[] equipCards = cardList.GetEquipmentsCardData(charCardData);
        for (int i = 0; i < 4; i++)
        {
            if (equipCards[i] == null)
            {
                animCons[i] = null;
                continue;
            }
                

            CardData equipCardData = equipCards[i].CardData;
            WeaponItemData weaponItemData = cardsDictionary.GetWeaponItemData(equipCardData);

            if (weaponItemData.itemData == null) continue;

            animCons[i] = weaponItemData.itemData.CardItemAnimator.CardImageAnim;
        }
        foreach (var item in equipAnims)
        {
            if(item == null)
            continue;
        }
        return animCons;
    }
}
