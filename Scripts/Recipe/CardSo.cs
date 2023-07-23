using UnityEngine;

public enum grade
{
    Common,
    Rare,
    Epic,
    Unique,
    Legendary
}

public class CardSo : ScriptableObject
{
    public ItemGrade.grade grade;
    public WeaponData weaponData;
    public Item itemData;
}
