using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEngine;
using VHierarchy.Libs;

public class CardSlotManager : MonoBehaviour
{
    [SerializeField] List<CardSlot> weaponCardSlots;
    [SerializeField] List<CardSlot> itemCardSlots;
    [SerializeField] List<CardData> weaponCardData;
    [SerializeField] List<CardData> itemCardData;
    CardDataManager cardDataManager;

    #region 참조 변수
    [SerializeField] SetCardDataOnSlot displayCardOnSlot;
    [SerializeField] SlotPool slotPool;
    #endregion

    #region 슬롯 생성 관련 변수
    int numSlots;
    Dictionary<int, CardSlot> weaponSlots;
    Dictionary<int, CardSlot> itemSlots;
    [SerializeField] GameObject slotPrefab;
    [SerializeField] Transform weaponSlotField;
    [SerializeField] Transform itemSlotField;
    [SerializeField] Vector2 slotSize;
    #endregion

    void Start()
    {
        StartCoroutine(DelayInitCo());
    }
    IEnumerator DelayInitCo()
    {
        yield return new WaitForSeconds(1f);
        InitCardSlots();
    }
    // 내가 가진 모든 카드를 무기, 아이템으로 따로 저장
    public void InitCardSlots()
    {
        if (cardDataManager == null) cardDataManager = FindObjectOfType<CardDataManager>();
        List<CardData> myAllCardDatas = new();
        myAllCardDatas.AddRange(cardDataManager.GetMyCardList());

        if (weaponCardData == null) weaponCardData = new List<CardData>();
        if (itemCardData == null) itemCardData = new List<CardData>();


        foreach (var item in myAllCardDatas)
        {
            if (item.Type == "Weapon")
            {
                weaponCardData.Add(item);
            }
            else
            {
                itemCardData.Add(item);
            }
        }

        int totalCards = weaponCardData.Count + itemCardData.Count;
        Debug.Log($"실제 카드 수 = {myAllCardDatas.Count}와 초기화 시킨 카드의 수 = {totalCards}");


        // 무기 카드 슬롯 생성
        GenerateAllCardsOfType(weaponCardData, weaponSlotField);

        // 아이템 슬롯 생성
        GenerateAllCardsOfType(itemCardData, itemSlotField);
    }

    // 카드 슬롯 생성
    public void GenerateAllCardsOfType(List<CardData> cardList, Transform slotField)
    {
        List<CardData> cardDatas = new();
        List<GameObject> slots = new();

        if (weaponSlots == null) weaponSlots = new Dictionary<int, CardSlot>();
        if (itemSlots == null) itemSlots = new Dictionary<int, CardSlot>();

        cardDatas.AddRange(cardList); // 재료가 될 수 있는 카드들의 리스트

        numSlots = cardDatas.Count;

        // 카드 데이터 갯수만큼 빈 슬롯 생성
        for (int i = 0; i < numSlots; i++)
        {
            var slot = Instantiate(slotPrefab, slotField);
            // var slot = slotPool.GetSlot(slotType, transform);
            slot.transform.position = Vector3.zero;
            // slot.transform.localScale = new Vector2(0, 0);
            // slot.transform.DOScale(new Vector2(.5f, .5f), .2f).SetEase(Ease.OutBack);
            slot.transform.localScale = slotSize;
            slots.Add(slot);
        }

        // 카드 데이터 정렬
        List<CardData> cardDataSorted = new();
        cardDataSorted.AddRange(cardDatas);

        // // 내림차순으로 카드 정렬 
        // cardDataSorted.Sort((a, b) =>
        // {
        //     return new Sort().ByGrade(a, b);
        // });

        // cardDataSorted.Reverse();

        // 슬롯 풀의 Dispaly 설정
        for (int i = 0; i < numSlots; i++)
        {
            if (displayCardOnSlot == null) displayCardOnSlot = GetComponent<SetCardDataOnSlot>();
            displayCardOnSlot.PutCardDataIntoSlot(cardDataSorted[i], slots[i].GetComponent<CardSlot>());

            // Weapon 혹은 item 딕셔너리에 저장
            SetCardSlotDictionary(cardDataSorted[i], slots[i].GetComponent<CardSlot>());
        }
    }

    void SetCardSlotDictionary(CardData cardData, CardSlot cardSlot)
    {
        if (cardData.Type == "Weapon")
        {
            weaponSlots.Add(cardData.ID, cardSlot);
        }
        else
        {
            itemSlots.Add(cardData.ID, cardSlot);
        }
    }

    public void AddCardSlot(CardData card)
    {
        // 빈 슬롯을 생성해서 Weapon 혹은 Item 필드에 배치
        Transform field = card.Type == "Weapon" ? field = weaponSlotField : itemSlotField;
        var slot = Instantiate(slotPrefab, field);
        slot.transform.position = Vector3.zero;
        slot.transform.localScale = slotSize;

        SetCardSlotDictionary(card, slot.GetComponent<CardSlot>());

        UpdateCardDisplay(card);
    }

    public void UpdateCardDisplay(CardData card)
    {
        // 아이디로 슬롯을 찾아내서 display 변경
        if (card.Type == "Weapon")
        {
            if (weaponSlots.ContainsKey(card.ID))
            {
                displayCardOnSlot.PutCardDataIntoSlot(card, weaponSlots[card.ID]);
            }
            else
            {
                Debug.Log("Weapon 슬롯 플에 해당 Card Data {card}에 해당하는 슬롯이 없습니다. 에러입니다.");
            }
        }
        else
        {
            if (itemSlots.ContainsKey(card.ID))
            {
                displayCardOnSlot.PutCardDataIntoSlot(card, itemSlots[card.ID]);
            }
            else
            {
                Debug.Log("Item 슬롯 플에 해당 Card Data {card}에 해당하는 슬롯이 없습니다. 에러입니다.");
            }
        }
    }

    public void ClearAllSlots()
    {
        // 무기 슬롯 복사 및 파괴
        List<CardSlot> weaponSlotsCopy = new List<CardSlot>(weaponSlots.Values);
        foreach (var slot in weaponSlotsCopy)
        {
            if (slot != null && slot.gameObject != null)
            {
                Destroy(slot.gameObject);
            }
        }

        // 아이템 슬롯 복사 및 파괴
        List<CardSlot> itemSlotsCopy = new List<CardSlot>(itemSlots.Values);
        foreach (var slot in itemSlotsCopy)
        {
            if (slot != null && slot.gameObject != null)
            {
                Destroy(slot.gameObject);
            }
        }

        // Dictionary 비우기
        weaponSlots.Clear();
        itemSlots.Clear();

        // 리드 카드(시작 멤버)를 찾아 슬롯 초기화
        CardData leadCard = cardDataManager.GetMyCardList()
            .Find(x => x.StartingMember == StartingMember.Zero.ToString());

        if (leadCard != null)
        {
            AddCardSlot(leadCard);
        }
        else
        {
            Debug.LogWarning("리드 카드를 찾을 수 없습니다.");
        }
    }
}
