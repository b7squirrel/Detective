using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EquipmentType { Ori, Head, Chest, Legs, Gloves, Weapon }

public class Equipment : MonoBehaviour
{
    string[] equipments = new string[6];
    bool isEquipped;

    #region 오리카드
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
    #endregion

    #region 장비카드
    public void SetItemEquipped(bool _isEquipped)
    {
        isEquipped = _isEquipped;
    }
    #endregion
}
