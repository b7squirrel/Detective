using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SlotUpCard : MonoBehaviour
{
    #region 카드 관련 변수
    Card cardToUpgrade; // 업그레이드 슬롯에 올라가 있는 카드
    Card cardToFeed; // 재료로 쓸 카드. 지금 드래그 하는 카드
    #endregion

    #region 참조 변수
    CardsDictionary cardDictionary;
    CardDataManager cardDataManager;
    [SerializeField] SlotManager slotManager;
    #endregion

    #region 액션 이벤트 - SlotUpCardUI
    public event Action<Card> OnCardAcquiredOnUpSlotUI;
    public event Action<Card> OnCardAcquiredOnMatSlotUI;
    public event Action OnRefreshUI; // 슬롯을 비우는 등의 리프레시 UI
    public event Action OnUpdateUI; // 매 프레임 업데이트 되는 UI
    public event Action OnUpgradeConfirmation; // 합성 확인 창 UI
    public event Action OnMerging; // 합성 효과 UI
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
            return; // 업그레이드 슬롯에 카드가 없다면 아무것도 안함
        }

        OnUpdateUI?.Invoke();
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
            cardToUpgrade.transform.SetParent(transform);
            cardToUpgrade.GetComponent<RectTransform>().localScale = Vector3.one;

            // onCardAcquired
            OnCardAcquiredOnUpSlotUI?.Invoke(card);

            // 재료카드 패널 열기. SlotManager, SlotAllCards의 함수들 등록
            slotManager.GetIntoMatCards();
        }
        else
        {
            cardToFeed = card;; // 비어 있지 않다면 지금 카드는 재료 카드임

            cardToFeed.transform.SetParent(transform);
            cardToFeed.GetComponent<RectTransform>().localScale = Vector3.one * .6f;

            OnCardAcquiredOnMatSlotUI?.Invoke(card);
            OnUpgradeConfirmation?.Invoke();
        }
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
    public SlotType GetSlotType(Card card) // Draggable에서 카드를 놓을 수 있는지 여부를 판단
    {

        // 업그레이드 카드의 경우
        if (cardToUpgrade == null) // 슬롯 위에 카드가 없다면 무조건 올릴 수 있다
        {
            return SlotType.upSlot;
        }

        // 재료카드에 카드가 올라가 있는 경우
        if(cardToFeed != null)
        {
            return SlotType.none;
        }

        // 재료 카드의 경우
        ItemGrade.grade upgradeCardGrade = cardToUpgrade.GetCardGrade();
        ItemGrade.grade feedCardGrade = card.GetCardGrade();
        string upgradeCardName = cardToUpgrade.GetCardName();
        string feedCardName = card.GetCardName();

        if (upgradeCardGrade != feedCardGrade)
        {
            Debug.Log("같은 등급을 합쳐줘야 합니다");
            return SlotType.none;
        }

        if (upgradeCardName != feedCardName)
        {
            Debug.Log("같은 이름의 카드를 합쳐줘야 합니다.");
            return SlotType.none;
        }

        return SlotType.matSlot;
    }
    #endregion

    #region 업그레이드
    public void UpgradeCard()
    {
        int newCardGrade = (int)cardToUpgrade.GetCardGrade() + 1;
        if (newCardGrade > 5) newCardGrade = 5;

        string newGrade = ((ItemGrade.grade)newCardGrade).ToString();
        string type = (cardToUpgrade.GetCardType()).ToString();

        // 업그레이드에 쓰인 카드 삭제
        cardDataManager.RemoveCardFromMyCardList(cardToUpgrade);// 카드 데이터 삭제
        cardDataManager.RemoveCardFromMyCardList(cardToFeed);
        Destroy(cardToUpgrade.gameObject); // 실제 오브젝트 삭제
        Destroy(cardToFeed.gameObject);

        // 업그레이드로 생성된 카드 생성
        GameObject newCard = cardDictionary.GenCard(type, newGrade, cardToUpgrade.GetCardName());
        newCard.transform.SetParent(transform);
        newCard.transform.position = transform.position;
        newCard.transform.localScale = Vector3.one;

        // 생성된 카드를 내 카드 리스트에 저장
        Card upgraded = newCard.GetComponent<Card>();
        cardDataManager.AddCardToMyCardsList(upgraded);

        // 강화 성공 패널
        StartCoroutine(UpgradeUICo(upgraded));
    }

    IEnumerator UpgradeUICo(Card upgradedCard)
    {
        OnMerging?.Invoke();
        yield return new WaitForSeconds(.16f);
        slotManager.OpenUpgradeSuccesUI(upgradedCard);
    }
    #endregion

    #region Refresh
    public void ClearUpgradeSlot()
    {
        // 데이터는 이미 MyCardsList에 저장되었으니 gameObject는 제거해도 됨
        if (GetComponentInChildren<Card>() == null)
            return;
        Destroy(GetComponentInChildren<Card>().gameObject);

        OnRefreshUI?.Invoke();

        if(cardToUpgrade != null)
        {
            GameObject cardPrefab = cardToUpgrade.gameObject;
            cardToUpgrade = null;
            Destroy(cardPrefab);
        }
        
        if(cardToFeed != null)
        {
            GameObject cardPrefab = cardToFeed.gameObject;
            cardToFeed = null;
            Destroy(cardPrefab);
        }
    }
    #endregion
}