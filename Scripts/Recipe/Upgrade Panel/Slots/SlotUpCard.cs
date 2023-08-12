using UnityEngine;

public class SlotUpCard : MonoBehaviour
{
    #region Droppable 관련 변수
    Card cardToUpgrade; // 업그레이드 슬롯에 올라가 있는 카드
    Card cardToFeed; // 재료로 쓸 카드. 지금 드래그 하는 카드
    #endregion

    #region 참조 변수
    CardsDictionary cardDictionary;
    CardDataManager cardDataManager;
    [SerializeField] UpgradeSuccessUI upgradeSuccessUI;
    [SerializeField] SlotManager slotManager;
    #endregion

    #region Unity Callback 함수

    void Awake()
    {
        cardDictionary = FindObjectOfType<CardsDictionary>();
        cardDataManager = FindObjectOfType<CardDataManager>();
    }

    void OnEnable()
    {
        upgradeSuccessUI.gameObject.SetActive(false);
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
            // 재료카드 패널 열기. SlotManager, SlotAllCards의 함수들 등록
            slotManager.GetIntoMatCardsManager();
        }
        else
        {
            AcquireMeterial(card); // 비어 있지 않다면 지금 카드는 재료 카드임
            UpgradeCard();
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
        bool isAvailable = true;

        if (cardToUpgrade == null) // 슬롯 위에 카드가 없다면 무조건 올릴 수 있다
        {
            if(card.GetCardGrade() == ItemGrade.grade.Legendary)
            {
                Debug.Log("전설 등급은 더 이상 강화할 수 없습니다.");
                isAvailable = false;
                return isAvailable;
            }
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
        
        if (upgradeCardName != feedCardName)
        {
            Debug.Log("같은 이름의 카드를 합쳐줘야 합니다.");
            isAvailable = false;
            return isAvailable;
        }
        
        return isAvailable;
    }
    #endregion

    #region 업그레이드
    public void UpgradeCard()
    {
        int newCardGrade = (int)cardToUpgrade.GetCardGrade() + 1;
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
        InitUpgradeSuccessPanel(upgraded);
    }

    void InitUpgradeSuccessPanel(Card upgraded)
    {
        upgradeSuccessUI.gameObject.SetActive(true); // 강화 성공 패널 활성화
        upgradeSuccessUI.SetCard(upgraded); // 강화 성공 카드 초기화
    }
    #endregion

    #region Refresh
    public void ClearUpgradeSlot()
    {
        // 데이터는 이미 MyCardsList에 저장되었으니 gameObject는 제거해도 됨
        if (GetComponentInChildren<Card>() == null)
            return;
        Destroy(GetComponentInChildren<Card>().gameObject);
    }
    #endregion
}