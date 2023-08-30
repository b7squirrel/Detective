using UnityEngine;

public enum CardType { Weapon, Item, none }
public enum Grade { Common, Rare, Epic, Unique, Legendary }
public enum TargetSlot { UpField, MatField, UpSlot, MatSlot } // 클릭되었을 때 이동할 슬롯

// 슬롯 위에 있는 카드 내용 관리
public class CardSlot : MonoBehaviour
{
    CardData cardData;
    bool isEmpty = true;
    public TargetSlot targetSlot { get; set; }

    public CardData GetCardData()
    {
        return cardData;
    }

    public void SetWeaponCard(CardData _cardData, WeaponData _weaponData, TargetSlot _targetSlot)
    {
        isEmpty = false;
        targetSlot = _targetSlot;
        cardData = _cardData;
        // cardDisp 호출해서 카드 출력
        GetComponent<CardDisp>().InitWeaponCardDisplay(_weaponData);
    }
    public void SetItemCard(CardData _cardData, Item _itemData, TargetSlot _targetSlot)
    {
        isEmpty = false;
        targetSlot = _targetSlot;
        cardData = _cardData;

        // cardDisp 호출해서 카드 출력
        GetComponent<CardDisp>().InitItemCardDisplay(_itemData);
    }

    public void EmptySlot()
    {
        isEmpty = true;
        cardData = null;
        GetComponent<CardDisp>().EmptyCardDisplay();
    }
}
