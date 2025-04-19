using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllField : MonoBehaviour
{
    #region 참조 변수
    [SerializeField] CardDataManager cardDataManager;
    [SerializeField] SetCardDataOnSlot displayCardOnSlot;
    [SerializeField] SlotPool slotPool;
    CardSlotManager cardSlotManager;
    MainMenuManager mainMenuManager; // 슬롯이 다 옮겨지면 탭이 바뀌도록 하기 위해
    #endregion

    #region 슬롯 생성 관련 변수
    int numSlots;
    [SerializeField] GameObject slotPrefab;
    [SerializeField] Slots slotType;
    [SerializeField] Vector2 slotSize;
    // Dictionary<string, Transform> slotsOnField = new Dictionary<string, Transform>();
    List<CardData> slotsOnField = new();

    bool slotSwappingFinished;
    #endregion

    void OnDisable()
    {
        ClearSlots();
    }
    IEnumerator DelayClearSlots()
    {
        yield return null; // 한 프레임 대기
        ClearSlots(); // 여기서 SetParent 호출됨
    }

    #region Refresh
    public void GenerateAllCardsOfType(List<CardData> cardList)
    {
        slotSwappingFinished = false;

        List<CardData> cardDatas = new();
        List<GameObject> slots = new();

        cardDatas.AddRange(cardList); // 재료가 될 수 있는 카드들의 리스트

        numSlots = cardDatas.Count;

        // // 슬롯 생성
        // for (int i = 0; i < numSlots; i++)
        // {
        //     var slot = Instantiate(slotPrefab, transform);
        //     // var slot = slotPool.GetSlot(slotType, transform);
        //     slot.transform.position = Vector3.zero;
        //     // slot.transform.localScale = new Vector2(0, 0);
        //     // slot.transform.DOScale(new Vector2(.5f, .5f), .2f).SetEase(Ease.OutBack);
        //     slot.transform.localScale = slotSize;
        //     slots.Add(slot);
        // }

        // 카드 데이터 정렬
        List<CardData> cardDataSorted = new();
        cardDataSorted.AddRange(cardDatas);

        // 내림차순으로 카드 정렬 
        cardDataSorted.Sort((a, b) =>
        {
            return new Sort().ByGrade(a, b);
        });

        cardDataSorted.Reverse();

        // // 카드 Display
        // for (int i = 0; i < numSlots; i++)
        // {
        //     displayCardOnSlot.PutCardDataIntoSlot(cardDataSorted[i], slots[i].GetComponent<CardSlot>());
        // }

        if(cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();

        foreach (var item in cardDataSorted)
        {
            Transform picked = cardSlotManager.pickedSlotTransforms(item);
            if (picked == null) continue;
            bool isWeapon = item.Type == "Weapon" ? true : false;
            slotsOnField.Add(item);

            cardSlotManager.SetSlotsPosition(picked, isWeapon, transform);
        }

        slotSwappingFinished = true;
    }
    public void GenerateAllLaunchCardOfType(List<CardData> _cardList, CardData _lead)
    {
        List<CardData> cardDatas = new();
        List<GameObject> slots = new();

        cardDatas.AddRange(_cardList); // 재료가 될 수 있는 카드들의 리스트

        numSlots = cardDatas.Count;

        // 슬롯 생성
        for (int i = 0; i < numSlots; i++)
        {
            var slot = Instantiate(slotPrefab, transform);
            slot.transform.position = Vector3.zero;
            slot.transform.localScale = slotSize;
            slots.Add(slot);
        }

        // 카드 데이터 정렬
        List<CardData> cardDataSorted = new();
        cardDataSorted.AddRange(cardDatas);

        // 내림차순으로 카드 정렬 
        cardDataSorted.Sort((a, b) =>
        {
            return new Sort().ByGrade(a, b);
        });

        cardDataSorted.Reverse();

        // 리드 오리를 가장 앞에 배치
        cardDataSorted.Remove(_lead);
        cardDataSorted.Insert(0, _lead);

        // 카드 Display
        for (int i = 0; i < numSlots; i++)
        {
            displayCardOnSlot.PutCardDataIntoSlot(cardDataSorted[i], slots[i].GetComponent<CardSlot>());
        }
    }

    public void ClearSlots()
    {
        // int childCount = transform.childCount;
        // if(childCount == 0) return;

        // for (int i = childCount - 1; i >= 0; i--)
        // {
        //     Transform child = transform.GetChild(i);
        //     Destroy(child.gameObject);
        //     // slotPool.ReturnToPool(child);
        // }
        if (cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        if (slotsOnField.Count > 0)
        {
            foreach (var item in slotsOnField)
            {
                Transform slotTrans = cardSlotManager.pickedSlotTransforms(item);
                bool isWeapon = item.Type == "Weapon" ? true : false;
                cardSlotManager.SetSlotsPosition(slotTrans, isWeapon, null);
            }
        }
        slotsOnField.Clear();
    }
    #endregion

    void SetSwapState(bool isFinished)
    {
        if(mainMenuManager == null) mainMenuManager = FindObjectOfType<MainMenuManager>();
        mainMenuManager.SetSlotSwapState(isFinished);
    }
}
