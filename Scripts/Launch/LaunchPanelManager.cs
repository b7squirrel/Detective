using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchPanelManager : MonoBehaviour
{
    CardData CardOnDisplay { get; set; } // 디스플레이에 올라가 있는 오리 카드
    [SerializeField] CardData cardToEquip; // Equipment Info에 올라 갈 장비 카드

    int index; // 어떤 장비 슬롯인지

    CardDataManager cardDataManager;
    CardsDictionary cardDictionary;
    EquipmentSlotsManager equipmentSlotsManager;
    CardList cardList;
    StatManager statManager;
    CardDisp cardDisp; // Equip info panel이 활성화 되면 클릭한 카드의 disp클래스를 저장(equipped Text 표시를 위해)

    EquipDisplayUI equipDisplayUI;
    [SerializeField] EquipInfoPanel equipInfoPanel;

    [Tooltip("Head, Chest, Legs, Weapon 순서")]

    // 카드들이 보여지는 Field
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
        CardOnDisplay = null;
    }

    // 장비 필드에서 오리 카드를 클릭하면 equip Slot Action에서 호출
    // 오리 카드를 equip display에 보여준다
    public void InitDisplay(CardData oriCardDataToDisplay)
    {
        equipDisplayUI.OnDisplay(oriCardDataToDisplay); // 디스플레이 활성
        CardOnDisplay = oriCardDataToDisplay;
        equipmentSlotsManager.InitEquipSlots(oriCardDataToDisplay); // 오리 카드의 Data대로 장비 슬롯 설정 
        equipDisplayUI.SetWeaponDisply(oriCardDataToDisplay, equipmentSlotsManager.GetCurrentAttribute()); // 오리 카드 및 Attr
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
            card = cardDataManager.GetMyCardList().FindAll(x => x.Type == cardType); // field 오리만 보여줌
        }
        else if (cardType == CardType.Item.ToString())
        {
            foreach (var item in cardList.GetEquipmentCardsList())
            {
                if (item.IsEquipped)
                {
                    continue;
                    // 장착된 카드일 경우
                    // 카드에 있는 반투명 Equipped 활성화 시키기
                    // slot type 추가 : Equipped
                    // Equipped 카드는 터치하면 장착을 해제할 것인지 팝업을 띄움
                    // 장착에서는 아이템만 Equipped 처리하면 됨
                    // 장착해제를 선택하면 장착되어 있던 오리에게서 장착해제가 됨
                }
                card.Add(item.CardData);
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
}
