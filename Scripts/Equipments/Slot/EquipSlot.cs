using UnityEngine;

public class EquipSlot : MonoBehaviour, CardSlot
{
    CardData cardData;
    public bool isEmpty { get; private set; } = true;

    public bool IsEmpty()
    {
        return isEmpty;
    }
    public CardData GetCardData()
    {
        return cardData;
    }
    public void SetWeaponCard(CardData _cardData, WeaponData _weaponData)
    {
        isEmpty = false;
        // 슬롯에 카드 데이터를 넣고
        cardData = _cardData;

        // 디스플레이
        GetComponent<CardDisp>().InitWeaponCardDisplay(_weaponData);
    }
    public void SetItemCard(CardData _cardData, Item _itemData)
    {
        isEmpty = false;

        cardData = _cardData;
        
        GetComponent<CardDisp>().InitItemCardDisplay(_itemData);
    }
    public void EmptySlot()
    {
        isEmpty = true;
        GetComponent<CardDisp>().EmptyCardDisplay();
    }
}
