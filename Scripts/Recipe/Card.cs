using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardType {weapon, item, none}

public class Card : MonoBehaviour
{
    CardSo cardSo;
    ItemGrade.grade cardGrade;
    
    public void SetCardData(CardSo cardSo, int grade)
    {
        this.cardSo = cardSo;
        cardGrade = (ItemGrade.grade)grade;
        Debug.Log("생성된 카드의 레벨은 " + cardGrade);
    }

    public CardSo GetCardSo()
    {
        if (cardSo == null)
        {
            Debug.Log("카드에 카드데이터가 없습니다");
        }
        return cardSo;
    }

    public CardType GetCardType()
    {
        if(cardSo.weaponData != null)
        {
            return CardType.weapon;
        }
        else if(cardSo.itemData != null)
        {
            return CardType.item;
        }
        Debug.Log("카드 타입이 정해지지 않았습니다.");
        return CardType.none;
    }

    public ItemGrade.grade GetCardGrad()
    {
        return cardGrade;
    }
}
