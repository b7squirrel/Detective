using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveItems : MonoBehaviour
{
    [SerializeField] List<Item> items;

    Character character;

    void Awake()
    {
        character = GetComponent<Character>();
    }

    public void Equip(Item itemToEquip)
    {
        if (items == null)
        {
            items = new List<Item>();
        }

        Item newItemInstance = ScriptableObject.CreateInstance<Item>();
        newItemInstance.Init(itemToEquip.Name);
        newItemInstance.stats.Sum(itemToEquip.stats);
        newItemInstance.name = itemToEquip.Name;

        items.Add(newItemInstance);
        newItemInstance.Equip(character);
    }

    public void UnEquip(Item itemToUnEquip)
    {

    }

    internal void UpgradeItem(UpgradeData upgradeData)
    {
        Item itemToUpgrade = items.Find(id => id.Name == upgradeData.item.Name);
        // Debug.Log(itemToUpgrade.stats.armor);
        itemToUpgrade.Equip(character);
        itemToUpgrade.stats.Sum(upgradeData.itemStats);
    }
}
