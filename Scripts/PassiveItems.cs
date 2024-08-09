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
        //newItemInstance.SynergyWeapon = itemToEquip.SynergyWeapon;
        newItemInstance.SynergyWeapons.AddRange(itemToEquip.SynergyWeapons);
        newItemInstance.upgrades.AddRange(itemToEquip.upgrades);

        items.Add(newItemInstance);
        newItemInstance.UpdateStats(character);

        // Pause Panel에 전달
        GameManager.instance.GetComponent<PausePanel>().InitItemSlot(itemToEquip);
    }

    public Item GetSynergyCouple(string synergyWeapon)
    {
        // synergyWeapon과 일치하는 무기가 있는지 확인
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
            {
                Debug.Log($"Item at index {i} is null");
                continue;
            }

            if (items[i].SynergyWeapons == null)
            {
                Debug.Log($"SynergyWeapons list for item at index {i} is null");
                continue;
            }

            if (items[i].SynergyWeapons.Count == 0)
            {
                Debug.Log($"SynergyWeapons list for item at index {i} is empty");
                continue;
            }

            for (int j = 0; j < items[i].SynergyWeapons.Count; j++)
            {
                if (synergyWeapon == items[i].SynergyWeapons[j])
                {
                    return items[i];
                }
            }
        }

        return null;
    }

    public void CheckIfMaxLevel(Item item)
    {
        // if (item.stats.currentLevel == item.upgrades.Count + 1) // acquired에서 이미 레벨1이 되니까
        if (item.stats.currentLevel >= 1) // 아이템을 획득하기만 하면
        {
            WeaponData wd = null;

            // SynergyWeapons 리스트를 순회하며 해당하는 무기 데이터 찾기
            foreach (string synergyWeapon in item.SynergyWeapons)
            {
                wd = character.GetComponent<WeaponContainer>().GetCoupleWeaponData(synergyWeapon);
                if (wd != null)
                {
                    break; // 유효한 무기를 찾으면 루프를 나감
                }
            }

            if (wd == null)
            {
                // Debug.Log("시너지 커플 웨폰이 없습니다");
                return;
            }

            if (character.GetComponent<WeaponContainer>().IsWeaponMaxLevel(wd))
            {
                character.GetComponent<SynergyManager>().AddSynergyUpgradeToPool(wd);
            }
            else
            {
                // Debug.Log("시너지 커플 웨폰이 최고레벨이 아닙니다");
            }
        }
    }

    public int GetItemLevel(Item item)
    {
        foreach (var ob in items)
        {
            if (ob.Name == item.Name)
            {
                return ob.stats.currentLevel;
            }
        }
        return 0; // 가지고 있는 무기가 아니라 새로운 무기라면 레벨 0
    }

    public void UnEquip(Item itemToUnEquip)
    {

    }

    internal void UpgradeItem(UpgradeData upgradeData)
    {
        Item itemToUpgrade = items.Find(id => id.Name == upgradeData.item.Name);
        // Debug.Log(itemToUpgrade.stats.armor);
        itemToUpgrade.stats.SetStats(upgradeData.itemStats);
        itemToUpgrade.UpdateStats(character);

        GameManager.instance.GetComponent<PausePanel>().UpdateItemLevel(itemToUpgrade);
    }
}
