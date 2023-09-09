using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

/// <summary>
/// ClearSlots - 슬롯 부수기
/// GenerateAllCardsList - 슬롯 생성하고 정렬, 카드 Display
/// </summary>
public class EquipmentPanelManager : MonoBehaviour
{
    CardData CardOnDisplay { get; set; } // 디스플레이에 올라가 있는 오리 카드
    CardData CardOnInfo; // Equipment Info에 올라 갈 장비 카드
    CardData currentCardOnInfo; // Equipment Info에 올라가 있는 장비 카드

    DisplayCardOnSlot displayCardOnSlot; // 슬롯 위에 있는 카드 Display
    CardDataManager cardDataManager;
    CardsDictionary cardDictionary;
    CardList cardList;

    UpPanelUI upPanelUI; // UI 관련 클래스
    EquipDisplayUI equipDisplayUI;
    [SerializeField] EquipInfoPanel equipInfoPanel;

    [Tooltip("Head, Chest, Legs, Weapon 순서")]
    [SerializeField] Transform[] equipDispSlots; // 디스플레이에 있는 4개 슬롯

    // 카드들이 보여지는 Field
    [SerializeField] AllField field; // 모든 카드

    // 업그레이드 슬롯, 재료 슬롯
    [SerializeField] CardSlot upCardSlot;
    [SerializeField] CardSlot matCardSlot;

    [SerializeField] GameObject slotPrefab;

    void Awake()
    {
        displayCardOnSlot = GetComponent<DisplayCardOnSlot>();
        cardDataManager = FindObjectOfType<CardDataManager>();
        upPanelUI = GetComponent<UpPanelUI>();
        equipDisplayUI = GetComponentInChildren<EquipDisplayUI>();
        cardList = FindAnyObjectByType<CardList>();
        cardDictionary = FindAnyObjectByType<CardsDictionary>();

        // upCardSlot.EmptySlot();
        // matCardSlot.EmptySlot();
        // SetAllField();
    }

    void OnEnable()
    {
        SetAllFieldTypeOf("Weapon");
        // SetDisplay(field.GetFirstCardData());
        DeActivateEquipInfoPanel();
        CardOnDisplay = null;
    }

    public void SetDisplay(CardData cardDataToDisplay)
    {
        equipDisplayUI.SetWeaponDisply(cardDataToDisplay);
        CardOnDisplay = cardDataToDisplay;
    }

    public void ClearAllFieldSlots()
    {
        field.ClearSlots();
        // matField?.ClearSlots();
    }
    public void SetAllFieldTypeOf(string cardType)
    {
        ClearAllFieldSlots();
        List<CardData> card = new();

        // 아이템 카드는 착용되어 있지 않는 것들만 보여주기
        if (cardType == CardType.Weapon.ToString())
        {
            card = cardDataManager.GetMyCardList().FindAll(x => x.Type == cardType);
        }
        else if (cardType == CardType.Item.ToString())
        {
            foreach (var item in cardDataManager.GetMyCardList())
            {
                if (item.Type == CardType.Weapon.ToString())
                    continue;

                if (cardList.FindEquipmentCard(item).IsEquipped)
                    continue;
                card.Add(item);
            }
        }
        field.GenerateAllCardsOfType(card);

        // 장비 슬롯 타입 
        EquipSlotType currentSlotType = EquipSlotType.FieldOri;
        if (cardType == "Item")
        {
            currentSlotType = EquipSlotType.FieldEquipment;
        }

        EquipSlotAction[] slot = field.GetComponentsInChildren<EquipSlotAction>();
        foreach (var item in slot)
        {
            item.SetSlotType(currentSlotType);
        }
    }
    // info panel 의 equip 버튼
    public void EquipToUpCard()
    {
        // 장착 조건을 만족하지 않으면 버튼을 누를 수 없도록 하자.
        // equipmentUIManger.SetEquipSlot(CardData cardDataToEquip);

        int index = new EquipmentTypeConverter().ConvertStringToInt(CardOnInfo.EquipmentType);

        if(currentCardOnInfo != null)
        {
            // 이미 장비가 장착되어 있다면 해제하고
            cardList.UnEquip(CardOnDisplay, currentCardOnInfo);
            Destroy(equipDispSlots[index].GetComponentInChildren<EquipSlot>().gameObject);
        }
        // 장비 장착
        cardList.Equip(CardOnDisplay, CardOnInfo);
        currentCardOnInfo = CardOnInfo;

        var slot = Instantiate(slotPrefab, transform);
        slot.transform.SetParent(equipDispSlots[index]);
        slot.transform.localPosition = Vector2.zero;
        slot.transform.localScale = Vector2.one;
        slot.transform.rotation = equipDispSlots[index].rotation;

        Item itemData = cardDictionary.GetWeaponItemData(currentCardOnInfo).itemData;

        slot.GetComponent<CardSlot>().SetItemCard(currentCardOnInfo, itemData);
        slot.GetComponent<CardDisp>().InitItemCardDisplay(itemData);
        slot.GetComponent<EquipSlotAction>().SetSlotType(EquipSlotType.UpEquipment);

        SetAllFieldTypeOf("Item");
        DeActivateEquipInfoPanel();
    }
    public void ActivateEquipInfoPanel(CardData cardData)
    {
        equipInfoPanel.gameObject.SetActive(true);
        equipInfoPanel.SetPanel(cardData);
        CardOnInfo = cardData;
    }
    public void DeActivateEquipInfoPanel()
    {
        equipInfoPanel.gameObject.SetActive(false);
    }
}
