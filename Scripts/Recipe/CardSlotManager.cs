using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSlotManager : MonoBehaviour
{
    public static CardSlotManager instance;
    CardDataManager cardDataManager;
    CardList cardList;

    #region 참조 변수
    [SerializeField] SetCardDataOnSlot setCardDataOnSLot;
    [SerializeField] SlotPool slotPool;
    #endregion

    [Header("필드 제어")]
    [SerializeField] Animator fieldAnim; // 탭마다 다른 필드의 형태를 애니메이터로 제어

    #region 슬롯 생성 관련 변수
    Dictionary<int, CardSlot> mySlots = new Dictionary<int, CardSlot>(); // 내가 가지고 있는 모든 슬롯을 ID로 검색하기 위해서
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

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        StartCoroutine(DelayInitCo());
    }
    IEnumerator DelayInitCo()
    {
        yield return new WaitForSeconds(1f);
        InitSlots();
    }

    /// <summary>
    /// 최초에는 그냥 Card data manager에 접근해서 내가 가진 모든 카드를 얻어와서 슬롯을 생성
    /// my slots 딕셔너리에 id를 키로 해서 저장
    /// </summary>
    void InitSlots()
    {
        if (cardDataManager == null) cardDataManager = FindObjectOfType<CardDataManager>();
        List<CardData> myAllCardDatas = new();
        myAllCardDatas.AddRange(cardDataManager.GetMyCardList());

        foreach (var item in myAllCardDatas)
        {
            GenSlots(item);
        }
    }
    void GenSlots(CardData cardData)
    {
        // 생성할 때만 cardSlot에 접근해서 getcomponent를 하니까 그냥 하자
        var slot = Instantiate(slotPrefab, presentSlotField);
        slot.transform.localScale = slotSize * Vector2.one;
        mySlots.Add(cardData.ID, slot.GetComponent<CardSlot>());

        // 카드 디스플레이 업데이트
        UpdateCardDisplay(cardData);
    }
    public void SetSlotActive(int cardID, bool _active)
    {
        mySlots[cardID].gameObject.SetActive(_active);
    }
    public void DestroySlot(int cardID)
    {
        GameObject slotToDestroy = mySlots[cardID].gameObject;
        mySlots.Remove(cardID); // 딕셔너리에서 제거
        Destroy(slotToDestroy); // 실제 슬롯 오브젝트 제거
    }
    public void AddSlot(CardData card)
    {
        // 빈 슬롯을 생성해서 필드에 배치, 최초에는 카드 데이터가 필요함
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
            // 카드가 없으면 안되는데 임시로 카드를 생성하게 했음. 왜 없는 경우가 생기는지 찾아야 함
            AddSlot(card);
            Debug.Log("Weapon 슬롯 플에 해당 Card Data {card}에 해당하는 슬롯이 없습니다. 에러입니다.");
        }
    }
    /// <summary>
    /// 프레젠테이션 필드의 슬롯들을 모두 비활성화
    /// 이전 탭의 슬롯들이 남아있지 않도록 하기 위해
    /// 데이터 변화 없이 슬롯들만 필드에서 비활성화
    /// </summary>
    public void ClearPresentationField()
    {
        foreach (var item in mySlots)
        {
            item.Value.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 디버깅 용도. 카드의 레벨을 모두 최고 레벨로 올린 후 디스플레이 업데이트를 위해
    /// </summary>
    public void UpdateAllCardSlotDisplay()
    {
        List<CardData> mCards = cardDataManager.GetMyCardList();
        foreach (var item in mCards)
        {
            UpdateCardDisplay(item);
        }
    }

    /// <summary>
    /// 디버그 용도. 카드 초기화 버튼에 연결되어 있음. 모든 슬롯들을 파괴하고 딕셔너리에서 제거하기
    /// </summary>
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
            if(item == null) continue;
            GameObject.Destroy(item);
        }
        slotsToDestroy.Clear();
        mySlots.Clear();

        // 리드 카드(시작 멤버)를 찾아 슬롯 초기화
        CardData leadCard = cardDataManager.GetMyCardList()
            .Find(x => x.StartingMember == StartingMember.Zero.ToString());

        if (leadCard != null)
        {
            AddSlot(leadCard);
            AddItemSlotOf(leadCard);
        }
        else
        {
            Debug.LogWarning("리드 카드를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 오리 카드를 생성한 후에 오리가 장착하고 있는 필수 아이템을 검색해서 슬롯으로 추가
    /// </summary>
    public void AddItemSlotOf(CardData oriCard)
    {
        // AddCardSlot(defaultEquip);
        if(cardList == null) cardList = FindObjectOfType<CardList>();
        List<CardData> equipCardDatas = cardList.GetEquipCardDataOf(oriCard);

        foreach (var item in equipCardDatas)
        {
            AddSlot(item);
        }
    }
    
    public void SettrigerAnim(string trigger)
    {
        fieldAnim.SetTrigger(trigger);
    }
}
