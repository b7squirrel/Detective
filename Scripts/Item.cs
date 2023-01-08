using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "ItemData", menuName = "SO/ItemData")]
public class Item : ScriptableObject
{
    public string Name;
    public int armor;

    public void Equip(Character character)
    {
        character.Armor += armor;
    }

    public void UnEquip(Character character)
    {
        character.Armor -= armor;
    }
}
