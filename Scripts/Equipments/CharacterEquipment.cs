using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment
{
    public Equipment(Card card)
    {
        this.card = card;
    }
    public Card card;
}

public class EquipedEquipment
{
    public EquipedEquipment(string id, Equipment[] equipments)
    {
        this.characterID = id;
        this.equipments = equipments;
    }
    public string characterID;
    public Equipment[] equipments = new Equipment[5];
}

public enum EquipmentType
{
    Head,
    Chest,
    Legs,
    Gloves,
    Weapon
}

// 각각의 카드에 붙어서 장비를 관리
public class CharacterEquipment : MonoBehaviour
{
    string characterID;
    Equipment[] equipments = new Equipment[5];

    public void EquipEquipment(Equipment equipment)
    {
        EquipmentType equipmentType = equipment.card.GetEquipType();
        equipments[(int)equipmentType] = equipment;
    }

    public void UnequipEquipment(EquipmentType equipmentType)
    {
        equipments[(int)equipmentType] = null;
    }
}
