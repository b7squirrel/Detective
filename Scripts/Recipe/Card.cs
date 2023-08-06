using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardType { weapon, item, none }

public class Card : MonoBehaviour
{
    WeaponData weaponData;
    Item itemData;
    CardType cardType;
    string Name;
    ItemGrade.grade Grade;

    public void SetWeaponCardData(WeaponData _weaponData)
    {
        this.weaponData = _weaponData;
        cardType = CardType.weapon;
        Name = _weaponData.Name;
        Grade = _weaponData.grade;
        GetComponent<CardDisplay>().SetCardDisplay(Grade.ToString(), Name);
    }
    public void SetItemCardData(Item _itemData)
    {
        this.itemData = _itemData;
        cardType = CardType.item;
        Name = _itemData.Name;
        Grade = _itemData.grade;
        GetComponent<CardDisplay>().SetCardDisplay(Grade.ToString(), Name);
    }

    public string GetCardName()
    {
        return Name;
    }

    public CardType GetCardType()
    {
        if (cardType != CardType.weapon && cardType != CardType.item)
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
}
