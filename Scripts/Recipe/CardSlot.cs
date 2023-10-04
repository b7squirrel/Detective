using UnityEngine;

public enum CardType { Weapon, Item, none }
public enum Grade { Common, Rare, Epic, Unique, Legendary }
public enum TargetSlot { UpField, MatField, UpSlot, MatSlot } // 클릭되었을 때 이동할 슬롯

// 슬롯 위에 있는 카드 내용 관리
public class CardSlot : MonoBehaviour
{
    CardData cardData;
    public bool IsEmpty { get; private set; } = true;
    public bool OnEquipment {get; private set;} = false; // 착용되어 있거나 장비를 착용하고 있는지 여부
    [SerializeField] Animator[] equipAnims; // 카드의 장비 animator들을 끌어다 놓기

    [Header("debug")]
    [SerializeField] string ID;
    void OnEnable()
    {
        EmptySlot();
    }
    
    public CardData GetCardData()
    {
        return cardData;
    }

    public void SetWeaponCard(CardData _cardData, WeaponData _weaponData, bool _onEquipment)
    {
        IsEmpty = false;
        cardData = _cardData;
        // cardDisp 호출해서 카드 출력
        CardDisp cardDisp = GetComponent<CardDisp>();
        cardDisp.InitWeaponCardDisplay(_weaponData, _onEquipment);

        ID = cardData.ID;
    }
    public void SetItemCard(CardData _cardData, Item _itemData, bool _onEquipment)
    {
        IsEmpty = false;
        cardData = _cardData;

        // cardDisp 호출해서 카드 출력
        CardDisp cardDisp = GetComponent<CardDisp>();
        cardDisp.InitItemCardDisplay(_itemData, _onEquipment);

        ID = cardData.ID;
    }
    public Animator[] GetEquipAnims()
    {
        return equipAnims;
    }

    public void EmptySlot()
    {
        IsEmpty = true;
        GetComponent<CardDisp>().EmptyCardDisplay();
    }
}
