using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EquipmentType { Ori, Head, Chest, Legs, Gloves, Weapon }

// 각각의 카드에 붙어서 장비를 관리
public class CharacterEquipment : MonoBehaviour
{
    string[] equipments = new string[5];

    public void EquipEquipment(CardData cardData)
    {
        // 해당 부위의 equiopments에 cardData.ID 저장
        // equipments UI 
        // equipment data manager
    }

    public void UnequipEquipment(EquipmentType equipmentType)
    {
        equipments[(int)equipmentType] = null;
    }
}
