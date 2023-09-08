using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EquipmentType { Ori, Head, Chest, Legs, Gloves, Weapon }

public class Equipment
{
    public Equipment(string[] _equipments)
    {
        for (int i = 0; i < _equipments.Length; i++)
        {
            equipments[i] = _equipments[i];
        }
    }
    string[] equipments = new string[6];
    bool isEquipped;

    // 오리카드
    public void Equip(CardData equipmentCard)
    {
        // 해당 부위의 equiopments에 cardData.ID 저장
        int index = new EquipmentTypeConverter().ConvertStringToInt(equipmentCard.EquipmentType);
        equipments[index] = equipmentCard.ID;

        // equipments UI 
        // equipment data manager
    }

    public void UnequipEquipment(EquipmentType equipmentType)
    {
        equipments[(int)equipmentType] = null;
    }

    // 장비 카드
    public void SetItemEquipped(bool _isEquipped)
    {
        isEquipped = _isEquipped;
    }

}
