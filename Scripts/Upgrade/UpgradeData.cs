using UnityEngine;

public enum UpgradeType
{
    WeaponUpgrade,
    ItemUpgrade,
    WeaponGet,
    ItemGet,
    Heal,
    Coin,
    SynergyUpgrade
}

[CreateAssetMenu]
public class UpgradeData : ScriptableObject
{
    public int id;
    public UpgradeType upgradeType;
    public string Name;
    [TextArea] public string description;
    public string description2;

    [Header("Weapons")]
    public WeaponData weaponData;
    public WeaponStats weaponUpgradeStats;
    
    [Header("Required only for Acquire Weapon Upgrade")]
    public RuntimeAnimatorController newKidAnim;

    [Header("Items")]
    public Item item;
    public ItemStats itemStats;
}
