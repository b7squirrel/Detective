using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardType { Weapon, Item, none }

public class Card : MonoBehaviour
{
    WeaponData weaponData;
    Item itemData;
    Gear gear;
    CardType cardType;
    string ID, Name;
    int exp;
    int level;
    ItemGrade.grade Grade;

    int To_Level_Up_Card
    {
        get
        {
            return (int)(Mathf.Pow((level) / 3.5f, 2)) * 1000 + (100 * level);
        }
    }

    public void SetWeaponCardData(WeaponData _weaponData)
    {
        if(_weaponData == null) Debug.Log("weaponData가 Null입니다.");
        this.weaponData = _weaponData;
        ID = GetInstanceID().ToString();
        cardType = CardType.Weapon;
        Name = _weaponData.Name;
        Grade = _weaponData.grade;
        level = 1;
        GetComponent<CardDisplay>().InitWeaponCardDisplay(this.weaponData);
    }
    public void SetItemCardData(Item _itemData)
    {
        this.itemData = _itemData;
        ID = GetInstanceID().ToString();
        cardType = CardType.Item;
        Name = _itemData.Name;
        Grade = _itemData.grade;
        level = 1;
        GetComponent<CardDisplay>().InitItemCardDisplay(this.itemData);
    }

    public string GetCardID()
    {
        return ID;
    }

    public string GetCardName()
    {
        return Name;
    }

    public CardType GetCardType()
    {
        if (cardType != CardType.Weapon && cardType != CardType.Item)
        {
            Debug.Log("카드 타입이 정해지지 않았습니다.");
            return CardType.none;
        }
        return cardType;
    }

    public ItemGrade.grade GetCardGrade()
    {
        return Grade;
    }

    public WeaponData GetWeaponData()
    {
        if(weaponData == null)
        {
            Debug.Log("Weapon Data Null");
            return null;
        }
        return weaponData;
    }

    public Item GetItemData()
    {
        if(itemData == null)
        {
            Debug.Log("Item Data Null");
            return null;
        }
        return itemData;
    }

    public EquipmentType GetEquipType()
    {
        return gear.equipmentType;
    }

    public void AddExp(int expToAdd)
    {
        exp += expToAdd;
    }

    void LevelUp()
    {
        if(level < 30)
        {
            level++;
            GetComponent<CardDisplay>().UpdateCard(level);
        }
    }
}
