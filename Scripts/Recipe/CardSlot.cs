using UnityEngine;

// 슬롯 위에 있는 카드 내용 관리
public class CardSlot : MonoBehaviour
{
    CardData cardData;
    public bool IsEmpty { get; private set; } = true;
    public bool OnEquipment {get; private set;} = false; // 착용되어 있거나 장비를 착용하고 있는지 여부

    [Header("debug")]
    [SerializeField] string ID;
    [SerializeField] string grade;
    [SerializeField] string Name;
    void OnEnable()
    {
        EmptySlot();
    }
    
    public CardData GetCardData()
    {
        return cardData;
    }

    public void SetWeaponCard(CardData _cardData, WeaponData _weaponData)
    {
        IsEmpty = false;
        cardData = _cardData;
        // cardDisp 호출해서 카드 출력
        CardDisp cardDisp = GetComponent<CardDisp>();
        cardDisp.InitWeaponCardDisplay(_weaponData);

        ID = cardData.ID;
        grade = cardData.Grade;
        Name = cardData.Name;
    }
    public void SetItemCard(CardData _cardData, Item _itemData, bool _onEquipment)
    {
        IsEmpty = false;
        cardData = _cardData;

        // cardDisp 호출해서 카드 출력
        CardDisp cardDisp = GetComponent<CardDisp>();
        cardDisp.InitItemCardDisplay(_itemData, _cardData, _onEquipment);

        ID = cardData.ID;
        grade = cardData.Grade;
        Name = cardData.Name;
    }

    public void EmptySlot()
    {
        IsEmpty = true;
        GetComponent<CardDisp>().EmptyCardDisplay();
    }
}
