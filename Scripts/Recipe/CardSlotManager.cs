using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardSlotManager : MonoBehaviour
{
    public static CardSlotManager instance;
    CardDataManager cardDataManager;
    CardList cardList;

    #region 참조 변수
    [SerializeField] SetCardDataOnSlot setCardDataOnSLot;
    #endregion

    [Header("필드 제어")]
    [SerializeField] Animator fieldAnim;
    [SerializeField] Animator slotButton; // 다른 탭으로 이동할 때 펼쳐져 있는 버튼들을 모으기
    [SerializeField] ScrollRect scrollRect;

    #region 슬롯 생성 관련 변수
    Dictionary<int, CardSlot> mySlots = new Dictionary<int, CardSlot>();
    [SerializeField] GameObject slotPrefab;
    [SerializeField] Transform presentSlotField;
    [SerializeField] Vector2 slotSize;
    Dictionary<string, int> defaultEquipIndex = new Dictionary<string, int>{
            { "Head", 0 },
            { "Chest", 1 },
            { "Face", 2 },
            { "Hand", 3 }
    };
    #endregion

    #region 정렬 관련 변수
    SortType currentSortType = SortType.Level;
    bool ascending = false;
    #endregion

    #region 초기화 플래그
    bool isInitialized = false;
    #endregion

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        StartCoroutine(DelayInitCo());
    }

    // ⭐ 개선된 DelayInitCo - GameInitializer 대기
    IEnumerator DelayInitCo()
    {
        // GameInitializer가 모든 초기화를 완료할 때까지 대기
        Logger.Log("[CardSlotManager] 게임 초기화 대기 중...");
        yield return new WaitUntil(() => GameInitializer.IsInitialized);
        
        Logger.Log("[CardSlotManager] 초기화 시작");
        InitSlots();
        isInitialized = true;
        Logger.Log("[CardSlotManager] 초기화 완료");
    }

    void InitSlots()
    {
        if (cardDataManager == null) 
            cardDataManager = FindObjectOfType<CardDataManager>();
        
        List<CardData> myAllCardDatas = new();
        myAllCardDatas.AddRange(cardDataManager.GetMyCardList());

        foreach (var item in myAllCardDatas)
        {
            GenSlots(item);
        }

        InitialSortingByGrade();
    }
    
    void GenSlots(CardData cardData)
    {
        var slot = Instantiate(slotPrefab, presentSlotField);
        slot.transform.localScale = slotSize * Vector2.one;
        mySlots.Add(cardData.ID, slot.GetComponent<CardSlot>());

        UpdateCardDisplay(cardData);
    }
    
    public void SetSlotActive(int cardID, bool _active)
    {
        if (!isInitialized)
        {
            Logger.LogWarning($"[CardSlotManager] 슬롯 초기화 전 호출됨: ID {cardID}");
            return;
        }
        
        if (mySlots.TryGetValue(cardID, out CardSlot slot))
        {
            slot.gameObject.SetActive(_active);
            Debug.Log($"[CardSlotManager] SetSlotActive: ID {cardID} → {_active}");
        }
        else
        {
            Logger.LogWarning($"[CardSlotManager] SetSlotActive 실패: ID {cardID} 슬롯이 존재하지 않습니다.");
        }
    }
    
    public void DestroySlot(int cardID)
    {
        if (mySlots.TryGetValue(cardID, out CardSlot slot))
        {
            GameObject slotToDestroy = slot.gameObject;
            mySlots.Remove(cardID);
            Destroy(slotToDestroy);
        }
    }
    
    public void AddSlot(CardData card)
    {
        var slot = Instantiate(slotPrefab, presentSlotField);
        slot.transform.position = Vector3.zero;
        slot.transform.localScale = slotSize;

        mySlots.Add(card.ID, slot.GetComponent<CardSlot>());
        UpdateCardDisplay(card);
    }

    public void UpdateCardDisplay(CardData card)
    {
        int cardID = card.ID;
        if (mySlots.ContainsKey(cardID))
        {
            setCardDataOnSLot.PutCardDataIntoSlot(card, mySlots[cardID]);
        }
        else
        {
            AddSlot(card);
            Logger.Log($"[CardSlotManager] Weapon 슬롯 풀에 해당 Card Data에 해당하는 슬롯이 없습니다. 슬롯 생성함.");
        }
    }

    public void ClearPresentationField()
    {
        foreach (var item in mySlots)
        {
            item.Value.gameObject.SetActive(false);
        }

        slotButton.SetTrigger("CloseIdle"); // Close를 거치지 않고 바로 Close Idle로 가도록
    }

    public void UpdateAllCardSlotDisplay()
    {
        foreach (KeyValuePair<int, CardSlot> item in mySlots)
        {
            UpdateCardDisplay(item.Value.GetCardData());
        }
    }

    public void ClearAllSlots()
    {
        ClearPresentationField();
        List<CardSlot> slotsToDestroy = new List<CardSlot>();
        foreach (var item in mySlots)
        {
            slotsToDestroy.Add(item.Value);
        }
        mySlots.Clear();
        foreach (var item in slotsToDestroy)
        {
            if (item == null) continue;
            GameObject.Destroy(item);
        }
        slotsToDestroy.Clear();
        mySlots.Clear();

        CardData leadCard = cardDataManager.GetMyCardList()
            .Find(x => x.StartingMember == StartingMember.Zero.ToString());

        if (leadCard != null)
        {
            AddSlot(leadCard);
            AddItemSlotOf(leadCard);
        }
        else
        {
            Logger.LogWarning("[CardSlotManager] 리드 카드를 찾을 수 없습니다.");
        }
    }

    public void AddItemSlotOf(CardData oriCard)
    {
        if (cardList == null) cardList = FindObjectOfType<CardList>();
        List<CardData> equipCardDatas = cardList.GetEquipCardDataOf(oriCard);

        foreach (var item in equipCardDatas)
        {
            AddSlot(item);
        }
    }

    public void SettrigerAnim(string trigger)
    {
        Debug.Log($"[CardSlotManager] SettrigerAnim(\"{trigger}\") 호출됨. 프레임={Time.frameCount}\n{System.Environment.StackTrace}");
        fieldAnim.SetTrigger(trigger);
        SortSlots(currentSortType, ascending); // 현재 상태 유지하며 재정렬
        scrollRect.verticalNormalizedPosition = 1f;
    }

    public CardSlot GetSlotByID(int cardID)
    {
        mySlots.TryGetValue(cardID, out CardSlot slot);
        return slot;
    }

    // ⭐ 트리거만 세팅 (정렬/스크롤 부수효과 없이)
    public void SetFieldAnimTriggerOnly(string trigger)
    {
        fieldAnim.SetTrigger(trigger);
    }

    // ⭐ 탭 전환 시점에 호출: Slot Containers를 즉시 "Off" 상태로 강제 스냅
    //    SetTrigger 방식이 아니라 Play()로 직접 상태를 지정하므로
    //    다른 트리거와의 경합(race condition) 없이 항상 확실하게 Off로 리셋됨
    public void ForceFieldOff()
    {
        if (fieldAnim == null) return;

        // 혹시 armed 상태로 남아있을 수 있는 다른 트리거들을 모두 리셋
        // (경합으로 인해 나중에 다시 엉뚱한 상태로 튕기는 것을 방지)
        fieldAnim.ResetTrigger("Launch");
        fieldAnim.ResetTrigger("MergeW");
        fieldAnim.ResetTrigger("MergeI");
        fieldAnim.ResetTrigger("EquipW");
        fieldAnim.ResetTrigger("EquipI");
        fieldAnim.ResetTrigger("Off");

        // 트랜지션 평가를 거치지 않고 즉시 Off 상태(내려간 포즈)로 스냅
        fieldAnim.Play("Slot Containers Off", 0, 1f);
        fieldAnim.Update(0f);
    }

    #region 정렬
    void SortSlots(SortType sortType, bool ascending)
    {
        List<CardData> cards = new();
        foreach (KeyValuePair<int, CardSlot> item in mySlots)
        {
            cards.Add(item.Value.GetCardData());
        }

        // 리드 오리(StartingMember.Zero)는 정렬 대상에서 제외하고 따로 보관
        CardData leadCard = cards.Find(x => x.StartingMember == StartingMember.Zero.ToString());
        if (leadCard != null)
        {
            cards.Remove(leadCard);
        }

        switch (sortType)
        {
            case SortType.Level:
                cards.Sort((a, b) => ascending ? a.Level.CompareTo(b.Level) : b.Level.CompareTo(a.Level));
                break;
            case SortType.Grade:
                cards.Sort((a, b) => ascending ? a.Grade.CompareTo(b.Grade) : b.Grade.CompareTo(a.Grade));
                break;
            case SortType.EvoStage:
                cards.Sort((a, b) => ascending ? a.EvoStage.CompareTo(b.EvoStage) : b.EvoStage.CompareTo(a.EvoStage));
                break;
            case SortType.Name:
                cards.Sort((a, b) => ascending
                    ? string.Compare(a.Name, b.Name, System.StringComparison.Ordinal)
                    : string.Compare(b.Name, a.Name, System.StringComparison.Ordinal));
                break;
        }

        // 리드 오리를 맨 앞에 다시 삽입 (정렬 기준과 무관하게 항상 최상단)
        if (leadCard != null)
        {
            cards.Insert(0, leadCard);
        }

        for (int i = 0; i < cards.Count; i++)
        {
            if (mySlots.TryGetValue(cards[i].ID, out CardSlot slot))
            {
                slot.transform.SetSiblingIndex(i);
            }
        }
    }

    public void InitialSortingByGrade()
    {
        currentSortType = SortType.Grade; // ✅ 동기화
        ascending = false;
        SortSlots(SortType.Grade, false);
    }

    void SortBy(SortType sortType)
    {
        if (currentSortType == sortType)
        {
            ascending = !ascending;
        }
        else
        {
            currentSortType = sortType;
            ascending = false;
        }

        SortSlots(sortType, ascending);
    }
    
    public void SortByName() => SortBy(SortType.Name);
    public void SortByGrade() => SortBy(SortType.Grade);
    public void SortByLevel() => SortBy(SortType.Level);
    public void SortByEvoStage() => SortBy(SortType.EvoStage);
    #endregion
}