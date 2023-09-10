using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// ClearSlots - 슬롯 부수기
/// GenerateAllCardsList - 슬롯 생성하고 정렬, 카드 Display
/// </summary>
public class EquipmentPanelManager : MonoBehaviour
{
    CardData CardOnDisplay { get; set; } // 디스플레이에 올라가 있는 오리 카드
    [SerializeField] CardData[] CardToEquip = new CardData[4]; // Equipment Info에 올라 갈 장비 카드
    
    int index; // 어떤 장비 슬롯인지

    CardDataManager cardDataManager;
    CardsDictionary cardDictionary;
    CardList cardList;

    EquipDisplayUI equipDisplayUI;
    [SerializeField] EquipInfoPanel equipInfoPanel;

    [Tooltip("Head, Chest, Legs, Weapon 순서")]

    // 카드들이 보여지는 Field
    [SerializeField] AllField field; // 모든 카드

    [SerializeField] GameObject slotPrefab;

    void Awake()
    {
        cardDataManager = FindObjectOfType<CardDataManager>();
        equipDisplayUI = GetComponentInChildren<EquipDisplayUI>();
        cardList = FindAnyObjectByType<CardList>();
        cardDictionary = FindAnyObjectByType<CardsDictionary>();

        for (int i = 0; i < CardToEquip.Length; i++)
        {
            CardToEquip[i] = null;
        }
    }

    void OnEnable()
    {
        SetAllFieldTypeOf("Weapon");
        DeActivateEquipInfoPanel();
        CardOnDisplay = null;
        equipDisplayUI.OffDisplay();
    }

    public void SetDisplay(CardData cardDataToDisplay)
    {
        equipDisplayUI.OnDisplay();
        equipDisplayUI.SetWeaponDisply(cardDataToDisplay);
        CardOnDisplay = cardDataToDisplay;
        PutEquipmentsOnCharCard(cardDataToDisplay);
    }

    public void ClearAllFieldSlots()
    {
        field.ClearSlots();
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
        // 장비 패널 UI 를 분리시켜서 카드를 업데이트 하는 등의 작업은 모두 거기서 하자 
        // 디스플레이 되는 charCard의 equipments
        EquipmentCard[] equipmentCards = cardList.GetEquipmentsCardData(CardOnDisplay);
        
        // 4개 슬롯에 업데이트 하기 
        equipDisplayUI.UpdateSlots(equipmentCards);

        // 장착하려는 장비 부위에 이미 다른 장비가 장착되어 있다면 CardList에서 그 장비를 해제하고
        if(equipmentCards[index].CardData.Name != null)
        {
            Debug.Log("장비가 이미 있습니다.");
            cardList.UnEquip(CardOnDisplay, equipmentCards[index]);
        }

        // 새로운 장비 장착
        cardList.Equip(CardOnDisplay, CardToEquip[index]);

        Item itemData = cardDictionary.GetWeaponItemData(CardToEquip[index]).itemData;

        equipDisplayUI.SetSlot(index, itemData, CardToEquip[index]);

        SetAllFieldTypeOf("Item");
        DeActivateEquipInfoPanel();
    }

    // info panel의 UnEquip 버튼
    public void UnEquipCard()
    {
        // 장비 해제
        EquipmentCard[] equipmentCards = cardList.GetEquipmentsCardData(CardOnDisplay);
        cardList.UnEquip(CardOnDisplay, equipmentCards[index]);

        equipDisplayUI.EmptyEquipSlot(index);
        CardToEquip[index] = null;

        SetAllFieldTypeOf("Item");
        DeActivateEquipInfoPanel();
    }
    void PutEquipmentsOnCharCard(CardData charCardData)
    {
        EquipmentCard[] equipmentCards = cardList.GetEquipmentsCardData(charCardData);

        // 슬롯 업데이트
        // 카드 리스트에서 불러오는 것이니까 카드 리스트에 따로 해줄 것은 없다.
        equipDisplayUI.UpdateSlots(equipmentCards);
        
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
