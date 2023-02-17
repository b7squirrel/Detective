using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeType
{
    WeaponUpgrade,
    ItemUpgrade,
    WeaponGet,
    ItemGet
}

[CreateAssetMenu]
public class UpgradeData : ScriptableObject
{
    public int id;
    public UpgradeType upgradeType;
    public string Name;
    public Sprite icon;

    [Header("Weapons")]
    public WeaponData weaponData;
    public WeaponStats weaponUpgradeStats;

    [Header("Items")]
    public Item item;
    public ItemStats itemStats;
}
