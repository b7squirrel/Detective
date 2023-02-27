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

    public WeaponStats(int damage, float timeToAttack, int numberOfAttacks, float sizeOfArea, float projectileSpeed, float knockBackChance)
    {
        this.damage = damage;
        this.timeToAttack = timeToAttack;
        this.numberOfAttacks = numberOfAttacks;
        this.sizeOfArea = sizeOfArea;
        this.projectileSpeed = projectileSpeed;
        this.knockBackChance = knockBackChance;
    }
    internal void Sum(WeaponStats weaponUpgradeStats)
    {
        this.damage += weaponUpgradeStats.damage;
        this.timeToAttack += weaponUpgradeStats.timeToAttack;
        this.numberOfAttacks += weaponUpgradeStats.numberOfAttacks;
        this.sizeOfArea += weaponUpgradeStats.sizeOfArea;
        this.projectileSpeed += weaponUpgradeStats.projectileSpeed;
        this.knockBackChance += weaponUpgradeStats.knockBackChance;
    }
}

[CreateAssetMenu]
public class WeaponData : ScriptableObject
{
    public string Name;
    public Transform weaponPrefab; // gun, staff, etc...
    public WeaponStats stats;
    public GameObject weaponBasePrefab;
    public RuntimeAnimatorController animatorController;
    public List<UpgradeData> upgrades;
    public string description;
}
