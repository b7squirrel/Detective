using System;
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

public enum EquipmentType
{
    Head,
    Chest,
    Legs,
    Gloves,
    Weapon
}

public class EquipmentManager : MonoBehaviour
{
    Equipment[] equipments = new Equipment[5];

    public void EquipEquipment(Equipment equipment)
    {
        EquipmentType equipmentType = equipment.card.GetEquipType();
        equipments[(int)equipmentType] = equipment;
    }
}
        
    
