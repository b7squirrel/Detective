using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����ī�忡 ������ ����� animator ������ �Ѱ���
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
        foreach (var item in equipAnims)
        {
            if(item == null)
            continue;

            Debug.Log(item.name);
        }
        return equipAnims;
    }
}
