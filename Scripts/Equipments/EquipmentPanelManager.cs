using System.Collections;
using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// ClearSlots - 슬롯 부수기
/// GenerateAllCardsList - 슬롯 생성하고 정렬, 카드 Display
/// </summary>
public class EquipmentPanelManager : MonoBehaviour
{
    CardData CardOnDisplay { get; set; } // 디스플레이에 올라가 있는 오리 카드
    [SerializeField] CardData[] CardToEquip = new CardData[4]; // Equipment Info에 올라 갈 장비 카드
    [SerializeField] CardData[] currentEquipments = new CardData[4]; // Equipment Info에 올라가 있는 장비 카드
    int index; // 어떤 장비 슬롯인지

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

        for (int i = 0; i < CardToEquip.Length; i++)
        {
            currentEquipments[i] = null;
            CardToEquip[i] = null;
        }
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
        PutEquipmentsOnCharCard(cardDataToDisplay);
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
        if (currentEquipments[index] != null)
        {
            Debug.Log("장비가 이미 있습니다.");
            // 이미 장비가 장착되어 있다면 해제하고
            cardList.UnEquip(CardOnDisplay, currentEquipments[index]);
            Destroy(equipDispSlots[index].GetComponentInChildren<EquipSlot>().gameObject);
        }
        // 장비 장착
        cardList.Equip(CardOnDisplay, CardToEquip[index]);
        currentEquipments[index] = CardToEquip[index];

        var slot = Instantiate(slotPrefab);
        slot.transform.SetParent(equipDispSlots[index]);
        slot.transform.localPosition = Vector2.zero;
        slot.transform.localScale = Vector2.one;
        slot.transform.rotation = equipDispSlots[index].rotation;

        Item itemData = cardDictionary.GetWeaponItemData(currentEquipments[index]).itemData;

        slot.GetComponent<CardSlot>().SetItemCard(currentEquipments[index], itemData);
        slot.GetComponent<CardDisp>().InitItemCardDisplay(itemData);
        slot.GetComponent<EquipSlotAction>().SetSlotType(EquipSlotType.UpEquipment);

        SetAllFieldTypeOf("Item");
        DeActivateEquipInfoPanel();
    }
    // info panel의 UnEquip 버튼
    public void UnEquipCard()
    {
        // 장비 해제
        cardList.UnEquip(CardOnDisplay, currentEquipments[index]);
        Destroy(equipDispSlots[index].GetComponentInChildren<EquipSlot>().gameObject);
        currentEquipments[index] = null;
        CardToEquip[index] = null;

        SetAllFieldTypeOf("Item");
        DeActivateEquipInfoPanel();
    }
    void PutEquipmentsOnCharCard(CardData charCardData)
    {
        EquipmentCard[] equipmentCards = cardList.GetEquipmentsCardData(charCardData);

        // 슬롯을 다 부수고
        for (int i = 0; i < equipDispSlots.Length; i++)
        {
            EquipSlot equipSlot = equipDispSlots[i].GetComponentInChildren<EquipSlot>();
            if (equipSlot != null) Destroy(equipSlot.gameObject);

            // 슬롯을 equipmentCards 갯수대로 만들고 배치
            if (equipmentCards[i] != null)
            {
                currentEquipments[i] = equipmentCards[i].CardData;
                
                var slot = Instantiate(slotPrefab);
                slot.transform.SetParent(equipDispSlots[i]);
                slot.transform.localPosition = Vector2.zero;
                slot.transform.localScale = Vector2.one;
                slot.transform.rotation = equipDispSlots[i].rotation;

                Item itemData = cardDictionary.GetWeaponItemData(currentEquipments[i]).itemData;

                slot.GetComponent<CardSlot>().SetItemCard(currentEquipments[i], itemData);
                slot.GetComponent<CardDisp>().InitItemCardDisplay(itemData);
                slot.GetComponent<EquipSlotAction>().SetSlotType(EquipSlotType.UpEquipment);

            }
            currentEquipments[i] = null;
        }
        // 카드 리스트에서 불러오는 것이니까 카드 리스트에 따로 해줄 것은 없다.
    }
    public void ActivateEquipInfoPanel(CardData cardData, bool isEquipButton)
    {
        index = new EquipmentTypeConverter().ConvertStringToInt(cardData.EquipmentType);

        equipInfoPanel.gameObject.SetActive(true);
        equipInfoPanel.SetPanel(cardData, isEquipButton);
        CardToEquip[index] = cardData;
    }
    public void DeActivateEquipInfoPanel()
    {
        equipInfoPanel.gameObject.SetActive(false);
    }
}
