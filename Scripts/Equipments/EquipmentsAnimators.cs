using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 오리카드에 장착된 장비의 animator 정보를 넘겨줌
/// </summary>
public class EquipmentsAnimators : MonoBehaviour
{
    [SerializeField] CardList cardList;
    [SerializeField] CardsDictionary cardsDictionary;

    public Animator[] GetEquipmentAnimators(CardData charCardData, CardSlot targetSlot)
    {
        Animator[] equipAnims = targetSlot.GetComponent<CardDisp>().GetEquipmentAnimators();

        EquipmentCard[] equipCards = cardList.GetEquipmentsCardData(charCardData);
        for (int i = 0; i < 4; i++)
        {
            if (equipCards[i] == null)
            {
                equipAnims[i] = null;
                continue;
            }
                

            CardData equipCardData = equipCards[i].CardData;
            WeaponItemData weaponItemData = cardsDictionary.GetWeaponItemData(equipCardData);

            if (weaponItemData.itemData == null) continue;

            Debug.Log(equipAnims[i].name);
            equipAnims[i].runtimeAnimatorController = weaponItemData.itemData.CardItemAnimator.CardImageAnim;
        }
        Debug.Log(equipAnims.Length);
        return equipAnims;
    }
}
