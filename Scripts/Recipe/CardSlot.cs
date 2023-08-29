using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Grade
{
    Common,
    Rare,
    Epic,
    Unique,
    Legendary
}

// 카드를 슬롯위에 보여주는 역할
public class CardSlot : MonoBehaviour
{
    CardData cardData;

    public CardData GetCardData()
    {
        return cardData;
    }
    public string GetCardID()
    {
        return cardData.ID;
    }
    public CardType GetCardType()
    {
        if (cardData.Type == "Weapon")
        {
            return CardType.Weapon;
        }
        else
        {
            return CardType.Item;
        }
    }
    public string GetCardName()
    {
        return cardData.Name;
    }

    public string GetCardGrade()
    {
        return cardData.Grade;
    }
}
