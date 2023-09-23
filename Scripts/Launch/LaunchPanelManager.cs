using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchPanelManager : MonoBehaviour
{
    CardData CardOnDisplay { get; set; } // ���÷��̿� �ö� �ִ� ���� ī��
    [SerializeField] CardData cardToEquip; // Equipment Info�� �ö� �� ��� ī��

    int index; // � ��� ��������

    CardDataManager cardDataManager;
    CardsDictionary cardDictionary;
    EquipmentSlotsManager equipmentSlotsManager;
    CardList cardList;
    StatManager statManager;
    CardDisp cardDisp; // Equip info panel�� Ȱ��ȭ �Ǹ� Ŭ���� ī���� dispŬ������ ����(equipped Text ǥ�ø� ����)

    EquipDisplayUI equipDisplayUI;
    [SerializeField] EquipInfoPanel equipInfoPanel;

    [Tooltip("Head, Chest, Legs, Weapon ����")]

    // ī����� �������� Field
    [SerializeField] AllField field; // ��� ī��

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

    // ��� �ʵ忡�� ���� ī�带 Ŭ���ϸ� equip Slot Action���� ȣ��
    // ���� ī�带 equip display�� �����ش�
    public void InitDisplay(CardData oriCardDataToDisplay)
    {
        equipDisplayUI.OnDisplay(oriCardDataToDisplay); // ���÷��� Ȱ��
        CardOnDisplay = oriCardDataToDisplay;
        equipmentSlotsManager.InitEquipSlots(oriCardDataToDisplay); // ���� ī���� Data��� ��� ���� ���� 
        equipDisplayUI.SetWeaponDisply(oriCardDataToDisplay, equipmentSlotsManager.GetCurrentAttribute()); // ���� ī�� �� Attr
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

        // ������ ī��� ����Ǿ� ���� �ʴ� �͵鸸 �����ֱ�
        if (cardType == CardType.Weapon.ToString())
        {
            card = cardDataManager.GetMyCardList().FindAll(x => x.Type == cardType); // field ������ ������
        }
        else if (cardType == CardType.Item.ToString())
        {
            foreach (var item in cardList.GetEquipmentCardsList())
            {
                if (item.IsEquipped)
                {
                    continue;
                    // ������ ī���� ���
                    // ī�忡 �ִ� ������ Equipped Ȱ��ȭ ��Ű��
                    // slot type �߰� : Equipped
                    // Equipped ī��� ��ġ�ϸ� ������ ������ ������ �˾��� ���
                    // ���������� �����۸� Equipped ó���ϸ� ��
                    // ���������� �����ϸ� �����Ǿ� �ִ� �������Լ� ���������� ��
                }
                card.Add(item.CardData);
            }
        }
        field.GenerateAllCardsOfType(card);

        // ��� ���� Ÿ�� 
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
