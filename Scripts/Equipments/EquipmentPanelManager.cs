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

    DisplayCardOnSlot displayCardOnSlot; // 슬롯 위에 있는 카드 Display
    CardDataManager cardDataManager;
    UpPanelUI upPanelUI; // UI 관련 클래스

    // 카드들이 보여지는 Field
    [SerializeField] AllField allField; // 모든 카드
    [SerializeField] AllField allOriField; // 모든 오리 카드
    [SerializeField] MatField matField; // 오리 카드에 대한 장비 카드
    [SerializeField] AllField allEquipField; // 모든 장비 카드

    // 업그레이드 슬롯, 재료 슬롯
    [SerializeField] CardSlot upCardSlot;
    [SerializeField] CardSlot matCardSlot;

    void Awake()
    {
        displayCardOnSlot = GetComponent<DisplayCardOnSlot>();
        cardDataManager = FindObjectOfType<CardDataManager>();
        upPanelUI = GetComponent<UpPanelUI>();

        // upCardSlot.EmptySlot();
        // matCardSlot.EmptySlot();
        // SetAllField();
    }

    void OnEnable()
    {
        SetAllOriField();
    }

    public void SetAllField()
    {
        ClearAllFieldSlots();
        allField.gameObject.SetActive(true);
        // matField.gameObject.SetActive(false);

        // upCardSlot.EmptySlot();
        // matCardSlot.EmptySlot();
        allField.GenerateAllCardsOfType(cardDataManager.GetMyCardList());
    }
    public void SetAllOriField()
    {
        ClearAllFieldSlots();
        allOriField.gameObject.SetActive(true);
        allField.GenerateAllCardsOfType(GetAllOriList());
    }
    
    public void ClearAllFieldSlots()
    {
        allField.ClearSlots();
        // matField?.ClearSlots();
    }

    List<CardData> GetAllOriList()
    {
        List<CardData> ori = new();
        foreach (var item in cardDataManager.GetMyCardList())
        {
            if (item.Type == CardType.Weapon.ToString())
            {
                ori.Add(item);
            }
        }
        return ori;
    }
    List<CardData> GetAllEquipList()
    {
        List<CardData> equip = new();
        foreach (var item in cardDataManager.GetMyCardList())
        {
            if (item.Type == CardType.Item.ToString())
            {
                equip.Add(item);
            }
        }
        return equip;
    }
}
