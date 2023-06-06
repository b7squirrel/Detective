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
        newItemInstance.stats.SetStats(itemToEquip.stats);
        // newItemInstance.name = itemToEquip.Name; // Init에서 이름을 정해주니까 필요없어 보인다
        newItemInstance.SynergyWeapon = itemToEquip.SynergyWeapon;
        newItemInstance.upgrades.AddRange(itemToEquip.upgrades);

        items.Add(newItemInstance);
        newItemInstance.UpdateStats(character);
    }

    public Item GetSynergyCouple(string synergyWeapon)
    {
        Item couple = items.Find(x => x.SynergyWeapon == synergyWeapon);
        return couple;
    }

    // public void CheckIfMaxLevel(Character character)
    // {
    //     if (stats.currentLevel == upgrades.Count + 1) // acquired에서 이미 레벨1이 되니까
    //     {
    //         Debug.Log(Name + " is Max Level");

    //         WeaponData wd = character.GetComponent<WeaponContainer>().GetCoupleWeaponData(SynergyWeapon);
    //         if (wd == null)
    //         {
    //             // Debug.Log("시너지 커플 웨폰이 없습니다");
    //             return;
    //         }

    //         if (character.GetComponent<WeaponContainer>().IsWeaponMaxLevel(wd))
    //         {
    //             // Debug.Log("it시너지 웨폰 활성화");
    //             character.GetComponent<SynergyManager>().AddSynergyUpgradeToPool(wd);
    //         }
    //         else
    //         {
    //             // Debug.Log("시너지 커플 웨폰이 최고레벨이 아닙니다");
    //         }
    //     }
    // }

    public void UnEquip(Item itemToUnEquip)
    {

    }

    internal void UpgradeItem(UpgradeData upgradeData)
    {
        Item itemToUpgrade = items.Find(id => id.Name == upgradeData.item.Name);
        // Debug.Log(itemToUpgrade.stats.armor);
        itemToUpgrade.stats.SetStats(upgradeData.itemStats);
        itemToUpgrade.UpdateStats(character);
    }
}
