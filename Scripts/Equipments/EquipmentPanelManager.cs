using System.Collections.Generic;
using UnityEngine;

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
        for (int i = 0; i < CardToEquip.Length; i++)
        {
            CardToEquip[i] = null;
        }
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
        LoadEquipmentsOf(cardDataToDisplay);
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
            equipDisplayUI.OffDisplay(); // Display의 장비 슬롯들을 모두 비우고
            card = cardDataManager.GetMyCardList().FindAll(x => x.Type == cardType); // field 오리만 보여줌
        }
        else if (cardType == CardType.Item.ToString())
        {
            foreach (var item in cardList.GetEquipmentCardsList())
            {
                if (item.IsEquipped)
                    continue;
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
    
    public void Equip() // info panel 의 equip 버튼
    {
        // 디스플레이 되는 charCard의 equipments
        EquipmentCard[] equipmentCards = cardList.GetEquipmentsCardData(CardOnDisplay);

        // 장착하려는 장비 부위에 이미 다른 장비가 장착되어 있다면 CardList에서 그 장비를 해제하고
        if(equipDisplayUI.isEmpty(index) == false)
        {
            Debug.Log("장비가 이미 있습니다. 교체합니다.");
            cardList.UnEquip(CardOnDisplay, equipmentCards[index]);
        }

        // 새로운 장비 장착
        cardList.Equip(CardOnDisplay, CardToEquip[index]);

        Item itemData = cardDictionary.GetWeaponItemData(CardToEquip[index]).itemData;

        equipDisplayUI.SetSlot(index, itemData, CardToEquip[index]);

        SetAllFieldTypeOf("Item");
        DeActivateEquipInfoPanel();
    }
    
    public void UnEquip() // info panel의 UnEquip 버튼
    {
        // 장비 해제
        EquipmentCard[] equipmentCards = cardList.GetEquipmentsCardData(CardOnDisplay);
        cardList.UnEquip(CardOnDisplay, equipmentCards[index]);

        equipDisplayUI.EmptyEquipSlot(index);
        CardToEquip[index] = null;

        SetAllFieldTypeOf("Item");
        DeActivateEquipInfoPanel();
    }
    void LoadEquipmentsOf(CardData charCardData)
    {
        EquipmentCard[] equipmentCards = cardList.GetEquipmentsCardData(charCardData);
        if (equipmentCards == null) return;
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
