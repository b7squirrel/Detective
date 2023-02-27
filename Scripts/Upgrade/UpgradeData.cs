using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeType
{
    WeaponUpgrade,
    ItemUpgrade,
    WeaponGet,
    ItemGet,
    Heal,
    Coin
}

[CreateAssetMenu]
public class UpgradeData : ScriptableObject
{
    public int id;
    public UpgradeType upgradeType;
    public string Name;
    public Sprite icon;
    public string description;

    [Header("Weapons")]
    public WeaponData weaponData;
    public WeaponStats weaponUpgradeStats;
    
    [Header("Required only for Acquire Weapon Upgrade")]
    public RuntimeAnimatorController newKidAnim;

    [Header("Items")]
    public Item item;
    public ItemStats itemStats;
}
