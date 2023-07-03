using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


[Serializable]
public class ItemStats
{
    public int armor;
    public int magnetSize;
    public float moveSpeed;
    public int maxHp;
    public float damage;
    public int projectileAmount;
    public int projectileSpeed;
    public float knockBackChance;
    public float criticalDamageChance;
    public float coolTimeDown; 

    public int hp;
    public int coins;

    public int currentLevel;

    // coin과 hp는 Equip 같은 것들을 거치지 않고 바로 Level에서 적용되므로 Sum에 포함되지 않는다.
    // 최대치를 넘어서면 값을 최대치로 다시 정해준다
    // 무기들 리스트를 받아와서 값을 없그레이드 해준다
    // 증가분만 더해주기 위해서 stats의 값을 그대로 대입한다
    internal void SetStats(ItemStats stats)
    {
        armor = stats.armor;
        magnetSize = stats.magnetSize;

        moveSpeed = stats.moveSpeed;
        maxHp = stats.maxHp;
        damage = stats.damage;
        projectileAmount = stats.projectileAmount;
        projectileSpeed = stats.projectileSpeed;
        knockBackChance = stats.knockBackChance;
        criticalDamageChance = stats.criticalDamageChance;
        coolTimeDown = stats.coolTimeDown;

        currentLevel++;
    }
}

[CreateAssetMenu(fileName = "ItemData", menuName = "SO/ItemData")]
public class Item : ScriptableObject
{
    public string Name;
    public string SynergyWeapon;
    public ItemStats stats;
    public List<UpgradeData> upgrades;

    public void Init(string Name)
    {
        this.Name = Name; // 위에서 선언한 Name 
        this.name = Name; // 스크립터블 오브젝트의 이름
        stats = new ItemStats();
        upgrades = new List<UpgradeData>(); // UpgradeData들을 끌어다 놓기

        stats.currentLevel = 0;
    }

    // SetStats와 UpdtaeStats는 항상 함께 실행된다.
    // character에 setStats로 넘겨받은 증가분을 더해준다
    public void UpdateStats(Character character)
    {
        character.Armor += stats.armor;
        character.MagnetSize += stats.magnetSize;

        character.MoveSpeed += stats.moveSpeed;
        character.MaxHealth += stats.maxHp;
        character.DamageBonus += stats.damage;
        character.ProjectileAmount += stats.projectileAmount;
        character.ProjectileSpeed += stats.projectileSpeed;
        character.knockBackChance += stats.knockBackChance;
        character.CriticalDamageChance += stats.criticalDamageChance;
        character.Cooldown += stats.coolTimeDown;

        character.GetComponent<PassiveItems>().CheckIfMaxLevel(this);
    }

    void CheckIfMaxLevel(Character character)
    {
        if (stats.currentLevel == upgrades.Count + 1) // acquired에서 이미 레벨1이 되니까
        {
            Debug.Log(Name + " is Max Level");

            WeaponData wd = character.GetComponent<WeaponContainer>().GetCoupleWeaponData(SynergyWeapon);
            if (wd == null)
            {
                // Debug.Log("시너지 커플 웨폰이 없습니다");
                return;
            }

            if (character.GetComponent<WeaponContainer>().IsWeaponMaxLevel(wd))
            {
                // Debug.Log("it시너지 웨폰 활성화");
                character.GetComponent<SynergyManager>().AddSynergyUpgradeToPool(wd);
            }
            else
            {
                // Debug.Log("시너지 커플 웨폰이 최고레벨이 아닙니다");
            }
        }
    }

    public void UnEquip(Character character)
    {
        character.Armor -= stats.armor;
    }
}
