using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentPanelManager : MonoBehaviour
{
    CardData CardOnDisplay { get; set; } // 디스플레이에 올라가 있는 오리 카드
    [SerializeField] CardData cardToEquip; // Equipment Info에 올라 갈 장비 카드

    int index; // 어떤 장비 슬롯인지
    bool isEquipped; // 장비 정보창에 띄워진 장비가 착용중인지 아닌지 판단. 레벨업을 할 때 오리에게 attr을 적용할지 말지를 결정하기 위해

    CardDataManager cardDataManager;
    CardsDictionary cardDictionary;
    EquipmentSlotsManager equipmentSlotsManager;
    CardList cardList;
    StatManager statManager;
    CardDisp cardDisp; // Equip info panel이 활성화 되면 클릭한 카드의 disp클래스를 저장(equipped Text 표시를 위해)

    EquipDisplayUI equipDisplayUI;
    [SerializeField] EquipInfoPanel equipInfoPanel;
    [SerializeField] AllField field; // 모든 카드

    void Awake()
    {
        cardDataManager = FindObjectOfType<CardDataManager>();
        equipDisplayUI = GetComponentInChildren<EquipDisplayUI>();
        cardList = FindAnyObjectByType<CardList>();
        cardDictionary = FindAnyObjectByType<CardsDictionary>();
        equipmentSlotsManager = GetComponent<EquipmentSlotsManager>();
        statManager = FindAnyObjectByType<StatManager>();

        cardToEquip = null;
    }

    void OnEnable()
    {
        cardToEquip = null;
        SetAllFieldTypeOf("Weapon");
        DeActivateEquipInfoPanel();
        CardOnDisplay = null;
        ClearAllEquipmentSlots(); // logic, UI 모두 처리
    }

    // 장비 필드에서 오리 카드를 클릭하면 equip Slot Action에서 호출
    // 오리 카드를 equip display에 보여준다
    public void InitDisplay(CardData oriCardDataToDisplay)
    {
        equipDisplayUI.OnDisplay(oriCardDataToDisplay); // 디스플레이 활성
        CardOnDisplay = oriCardDataToDisplay;
        equipmentSlotsManager.InitEquipSlots(oriCardDataToDisplay); // 오리 카드의 Data대로 장비 슬롯 설정 
        equipDisplayUI.SetWeaponDisplay(oriCardDataToDisplay, equipmentSlotsManager.GetCurrentAttribute()); // 오리 카드 및 Attr
        isEquipped = false;
    }

    public void ClearAllFieldSlots()
    {
        field.ClearSlots();
    }
    public void SetAllFieldTypeOf(string cardType)
    {
        cardToEquip = null;

        ClearAllFieldSlots();
        List<CardData> card = new();

        // 아이템 카드는 착용되어 있지 않는 것들만 보여주기
        if (cardType == CardType.Weapon.ToString())
        {
            ClearAllEquipmentSlots(); // logic, UI 모두 처리

            card = cardDataManager.GetMyCardList().FindAll(x => x.Type == cardType); // field에는 오리만 보여줌
        }
        else if (cardType == CardType.Item.ToString())
        {
            foreach (var item in cardList.GetEquipmentCardsList())
            {
                if (item.IsEquipped) // 다른 오리에 장착된 카드는 보여주지 않음
                {
                    continue;
                    // 장착된 카드일 경우
                    // 카드에 있는 반투명 Equipped 활성화 시키기
                    // slot type 추가 : Equipped
                    // Equipped 카드는 터치하면 장착을 해제할 것인지 팝업을 띄움
                    // 장착에서는 아이템만 Equipped 처리하면 됨
                    // 장착해제를 선택하면 장착되어 있던 오리에게서 장착해제가 됨
                }

                // 범용이거나 해당 오리에 바인딩 되어 있는 장비라면 필드에 추가
                if (item.CardData.BindingTo == "All")
                {
                    // 범용이어도 필수 장비 슬롯과 겹치면서 해당 오리에 바인딩 되어 있지 않다면 빼기
                    if (item.CardData.EquipmentType == CardOnDisplay.EssentialEquip && item.CardData.BindingTo != CardOnDisplay.Name) 
                        continue;
                    card.Add(item.CardData);
                    continue;
                }
                if (item.CardData.BindingTo == CardOnDisplay.Name)
                {
                    card.Add(item.CardData);
                    continue;
                }
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
    void ClearAllEquipmentSlots()
    {
        // Display의 장비 슬롯들을 모두 비우기
        equipmentSlotsManager.ClearEquipSlots(); // logic
        equipDisplayUI.OffDisplay(); // UI
    }

    // info panel 의 equip 버튼
    public void OnEquipButton()
    {
        // 디스플레이 되는 charCard의 equipments
        EquipmentCard[] equipmentCards = cardList.GetEquipmentsCardData(CardOnDisplay);

        // 장착하려는 장비 부위에 이미 다른 장비가 장착되어 있다면 CardList에서 그 장비를 해제하고
        if (equipmentSlotsManager.IsEmpty(index) == false)
        {
            Debug.Log("장비가 이미 있습니다. 교체합니다.");
            cardList.UnEquip(CardOnDisplay, equipmentCards[index]);
        }

        // 새로운 장비 장착
        cardList.Equip(CardOnDisplay, cardToEquip);
        Item itemData = cardDictionary.GetWeaponItemData(cardToEquip).itemData;
        equipmentSlotsManager.SetEquipSlot(index, itemData, cardToEquip);

        SetAllFieldTypeOf("Item");
        DeActivateEquipInfoPanel();
    }

    public void EquipCard(CardData oriCard, CardData equipCard)
    {
        cardList.Equip(oriCard, equipCard);
    }

    // info panel의 UnEquip 버튼
    public void OnUnEquipButton()
    {
        // 장비 해제
        EquipmentCard[] equipmentCards = cardList.GetEquipmentsCardData(CardOnDisplay);
        cardList.UnEquip(CardOnDisplay, equipmentCards[index]);

        equipmentSlotsManager.EmptyEquipSlot(index);

        cardToEquip = null;

        cardDisp.SetEquppiedTextActive(false);
        SetAllFieldTypeOf("Item");
        DeActivateEquipInfoPanel();
    }
    // equip slot action 에서 호출
    public void ActivateEquipInfoPanel(CardData itemCardData, CardDisp cardDisp, bool isEquipButton, EquipmentType equipType)
    {
        index = new Convert().EquipmentTypeToInt(itemCardData.EquipmentType);
        isEquipped = !isEquipButton; // equip button을 띄운다는 것은 field에 있는 장비 카드라는 뜻이므로

        Item iData = cardDictionary.GetWeaponItemData(itemCardData).itemData;

        equipInfoPanel.gameObject.SetActive(true);

        bool isEssential = false;
        if (CardOnDisplay.EssentialEquip == equipType.ToString())
        {
            isEssential = true;
        }
        equipInfoPanel.SetPanel(itemCardData, iData, cardDisp, isEquipButton, isEssential);
        cardToEquip = itemCardData;
        this.cardDisp = cardDisp;
    }

    public void DeActivateEquipInfoPanel()
    {
        equipInfoPanel.gameObject.SetActive(false);
        this.cardDisp = null;
    }
    // info panel의 Upgrade 버튼
    public void UpgradeCard()
    {
        statManager.LevelUp(cardToEquip);

        // 장착되어 있는 장비를 레벨업 하는 경우라면 바로바로 currentAttr을 업데이트
        if (isEquipped) 
        {
            equipmentSlotsManager.InitEquipSlots(CardOnDisplay);
        }
    }
}
