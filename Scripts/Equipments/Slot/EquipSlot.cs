using UnityEngine;

public class EquipSlot : MonoBehaviour
{
    CardData cardData;
    public bool IsEmpty { get; private set; } = true;

    public CardData GetCardData()
    {
        return cardData;
    }
    public void SetWeaponSlot(CardData _cardData, WeaponData _weaponData)
    {
        IsEmpty = false;
        // 슬롯에 카드 데이터를 넣고
        cardData = _cardData;

        // 디스플레이
        GetComponent<CardDisp>().InitWeaponCardDisplay(_weaponData);
    }
    public void SetItemSlot(CardData _cardData, Item _itemData)
    {
        IsEmpty = false;

        cardData = _cardData;
        GetComponent<CardDisp>().InitItemCardDisplay(_itemData);
    }
    public void EmptySlot()
    {
        IsEmpty = true;
        GetComponent<CardDisp>().EmptyCardDisplay();
    }
}
