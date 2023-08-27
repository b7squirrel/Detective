using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Card는 ItemData가 아니라 Gear를 가지게 됨
[CreateAssetMenu(fileName = "New Equipment", menuName = "Custom/Equipment")]
public class Gear : ScriptableObject
{
    // Gear는 카드로만 존재한다. Gear의 Stats는 변하지 않는다
    public EquipmentType equipmentType; 
    public ItemGrade.grade grade;
    public string Name;
    public Sprite gearImage;
    public WeaponStats stats;
    public float dropChance;
}
