using UnityEngine;

public enum grade
{
    Common,
    Rare,
    Epic,
    Unique,
    Legendary
}

[CreateAssetMenu]
public class CardSo : ScriptableObject
{
    public ItemGrade.grade grade;
    public WeaponData weaponData;
    public Item itemData;
}
