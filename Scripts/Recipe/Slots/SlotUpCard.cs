using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SlotUpCard : MonoBehaviour
{
    #region Droppable 관련 변수
    Card cardToUpgrade; // 업그레이드 슬롯에 올라가 있는 카드
    Card cardToFeed; // 재료로 쓸 카드. 지금 드래그 하는 카드
    Transform previousParentOfPointerDrag; // 업그레이드 슬롯에 올려놓은 카드가 되돌아갈 위치
    bool isAvailable; // 카드가 슬롯 위에 올라올 수 있는지 여부 (업그레이드 카드든 재료 카드든)
    #endregion

    #region 참조 변수
    CardsDictionary cardDictionary;
    CardDataManager cardDataManager;
    #endregion

    #region 유니티 이벤트 변수
    [SerializeField] UnityEvent OnCardPlacedInUpgradeSlot; // 슬롯에 카드가 올라가면 재료 카드들만 보여주는 이벤트
    [SerializeField] UnityEvent OnUpgrade; // 업그레이드가 실행되면 업그레이드 연출화면으로 
    #endregion

    #region Unity Callback 함수
    void Awake()
    {
        cardDictionary = FindObjectOfType<CardsDictionary>();
        cardDataManager = FindObjectOfType<CardDataManager>();
    }

    void Update()
    {
        if (GetComponentInChildren<Card>() == null)
        {
            cardToUpgrade = null;
        }
    }
    #endregion

    #region Acquire Card
    // 업그레이드 슬롯위에 카드를 올릴 때
    // 업그레이드를 할 카드인지 재료 카드인지 구분해서 처리
    public void AcquireCard(Card card)
    {
        if (cardToUpgrade == null) // 업그레이드 슬롯이 비어 있다면
        {
            cardToUpgrade = card;
        }
        else
        {
            AcquireMeterial(card); // 비어 있지 않다면 지금 카드는 재료 카드임
            // UpgradeCard();

            OnUpgrade?.Invoke();
        }
    }

    void AcquireMeterial(Card card) // 재료가 되는 카드를 덮으면
    {
        cardToFeed = card;
    }
    #endregion

    #region GetCard
    public Card GetCardToUpgrade()
    {
        return cardToUpgrade;
    }
    public Card GetCardToFeed()
    {
        return cardToFeed;
    }
    #endregion

    #region Check Slot Availability
    public bool IsAvailable(Card card) // Draggable에서 카드를 놓을 수 있는지 여부를 판단
    {
        isAvailable = true;

        if (cardToUpgrade == null) // 슬롯 위에 카드가 없다면 무조건 올릴 수 있다
        {
            // OnCardPlacedInUpgradeSlot?.Invoke(); // MatSlot 활성화
            return true;
        }

        ItemGrade.grade upgradeCardGrade = cardToUpgrade.GetCardGrade();
        ItemGrade.grade feedCardGrade = card.GetCardGrade();
        string upgradeCardName = cardToUpgrade.GetCardName();
        string feedCardName = card.GetCardName();

        if (upgradeCardGrade != feedCardGrade)
        {
            Debug.Log("같은 등급을 합쳐줘야 합니다");
            isAvailable = false;
            return isAvailable;
        }

        if ((int)upgradeCardGrade == 4)
        {
            Debug.Log("전설 등급은 더 이상 강화할 수 없습니다.");
            isAvailable = false;
            return isAvailable;
        }
        if (upgradeCardName != feedCardName)
        {
            Debug.Log("같은 이름의 카드를 합쳐줘야 합니다.");
            isAvailable = false;
            return isAvailable;
        }
        
        return isAvailable;
    }
    #endregion

    #region Droppable 관련 함수
    public void SetPrevParent(Transform prevParent)
    {
        previousParentOfPointerDrag = prevParent;
    }
    #endregion

    #region Refresh
    // 업그레이드 패널 최상단의 SlotManager의 OnRefresh 유니티 이벤트에 등록되어 있음
    public void ClearUpgradeSlot()
    {
        // 데이터는 이미 MyCardsList에 저장되었으니 gameObject는 제거해도 됨
        if (GetComponentInChildren<Card>() == null)
            return;
        Destroy(GetComponentInChildren<Card>().gameObject);
        Debug.Log("업그레이드 슬롯 위의 카드 제거");
    }
    #endregion
}