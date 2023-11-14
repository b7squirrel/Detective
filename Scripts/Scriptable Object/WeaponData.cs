using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeaponStats
{
    public int damage;
    public float timeToAttack;
    public int numberOfAttacks;
    public float sizeOfArea;
    public float projectileSpeed;
    public float knockBackChance;
    public int currentLevel;

    public WeaponStats(int damage, float timeToAttack, int numberOfAttacks, float sizeOfArea, float projectileSpeed, float knockBackChance)
    {
        // 초기화
        this.damage = damage;
        this.timeToAttack = timeToAttack;
        this.numberOfAttacks = numberOfAttacks;
        this.sizeOfArea = sizeOfArea;
        this.projectileSpeed = projectileSpeed;
        this.knockBackChance = knockBackChance;
        this.currentLevel = 0;
    }
    internal void Sum(WeaponStats weaponUpgradeStats)
    {
        // 스탯 업데이트
        this.damage += weaponUpgradeStats.damage;
        this.timeToAttack += weaponUpgradeStats.timeToAttack;
        this.numberOfAttacks += weaponUpgradeStats.numberOfAttacks;
        this.sizeOfArea += weaponUpgradeStats.sizeOfArea;
        this.projectileSpeed += weaponUpgradeStats.projectileSpeed;
        this.knockBackChance += weaponUpgradeStats.knockBackChance;
        this.currentLevel++;
    }
}

[Serializable]
public class AnimatorData
{
    public RuntimeAnimatorController CardImageAnim;
    public RuntimeAnimatorController InGamePlayerAnim;
}

[CreateAssetMenu]
public class WeaponData : ScriptableObject
{
    public Grade grade;
    public string Name;
    public Sprite charImage;
    public string SynergyWeapon;
    public AnimatorData Animators;
    public Sprite DefaultHead, DefaultChest, DefaultFace, DefaultHand;
    [Header("Gun, Staff, etc")] public Transform weaponPrefab; // gun, staff, etc...
    public WeaponStats stats;
    public GameObject weaponBasePrefab;
    [Header("children anim")] public RuntimeAnimatorController animatorController;
    public List<UpgradeData> upgrades;
    public UpgradeData synergyUpgrade;
    public float dropChance; // 아이템 드랍 확률
    public bool hideEssentialEquipmentOnPlay;
    public bool needToSyncIdle;
}
