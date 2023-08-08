using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SlotManager : MonoBehaviour
{
    SlotsAllCards slotsAllCards;
    SlotsMatCards slotsMatCards;
    SlotUpCard slotUpCard;
    UpgradeSuccessUI upgradeSuccessUI;

    CardDataManager cardDataManager;
    CardsDictionary cardDictionary;

    [SerializeField] UnityEvent OnRefresh;

    bool isCardOnUpSlot; // 업그레이드 슬롯에 카드가 올려져 있음
    

    void Start()
    {
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            child.gameObject.SetActive(true);
        }

        slotsAllCards = GetComponentInChildren<SlotsAllCards>();
        slotsMatCards = GetComponentInChildren<SlotsMatCards>();
        slotUpCard = GetComponentInChildren<SlotUpCard>();
        upgradeSuccessUI = GetComponentInChildren<UpgradeSuccessUI>();

        cardDataManager = FindObjectOfType<CardDataManager>();
        cardDictionary = FindObjectOfType<CardsDictionary>();

        upgradeSuccessUI.gameObject.SetActive(false);
    }

    #region Refresh
    void OnEnable()
    {
        StartCoroutine(RefreshCo());
    }

    // 업그레이드 연출이 끝나면, 업그레이드 패널을 떠났다가 되돌아오면(Disabled, Enabled)
    // 업그레이드 패널을 갱신한다
    IEnumerator RefreshCo()
    {
        yield return new WaitForSeconds(.01f);
        RefreshUpgradePanel();
    }

    public void RefreshUpgradePanel()
    {
        Debug.Log("매니져 리프레쉬");
        // slotsAllCards, slotsMatCards, slotUpCard의 Clear함수들을 가지고 있음
        OnRefresh?.Invoke();
    }
    // MainMenuManger가 Awake에서 모든 패널을 비활성화 시키기 전에 
    // Refresh를 실행하는 것을 방지하기 위해 (NullReference)
    // 0.01초 후에 리프레시를 하도록 했음
    #endregion

    public void GetIntoMatCardsManager()
    {
        slotsAllCards.gameObject.SetActive(false);
        slotsMatCards.gameObject.SetActive(true);
    }

    public void GetIntoMyCardsmanager()
    {
        slotsAllCards.gameObject.SetActive(true);
        slotsMatCards.gameObject.SetActive(false);
    }

    // 업그레이드
    public void UpgradeCard()
    {
        Card cardToUpgrade = slotUpCard.GetCardToUpgrade();
        Card cardToFeed = slotUpCard.GetCardToFeed();

        int newCardGrade = (int)cardToUpgrade.GetCardGrade() + 1;
        string newGrade = ((ItemGrade.grade)newCardGrade).ToString();
        string type = (cardToUpgrade.GetCardType()).ToString();


        cardDataManager.RemoveCardFromMyCardList(cardToUpgrade);// 카드 데이터 삭제
        cardDataManager.RemoveCardFromMyCardList(cardToFeed);
        Destroy(cardToUpgrade.gameObject); // 실제 오브젝트 삭제
        Destroy(cardToFeed.gameObject);

        GameObject newCard = cardDictionary.GenCard(type, newGrade, cardToUpgrade.GetCardName());
        newCard.transform.SetParent(slotUpCard.transform);
        newCard.transform.position = slotUpCard.transform.position;
        newCard.transform.localScale = Vector3.one;

        Card upgraded = newCard.GetComponent<Card>();
        AddCardToList(upgraded);

        InitUpgradeSuccessPanel(upgraded); // 업그레이드 성공 화면으로 넘어감
    }

    void InitUpgradeSuccessPanel(Card upgraded)
    {
        upgradeSuccessUI.gameObject.SetActive(true); // 강화 성공 패널 활성화
        upgradeSuccessUI.SetCard(upgraded); // 강화 성공 카드 초기화
    }

    void AddCardToList(Card _card)
    {
        string type = _card.GetCardType().ToString();
        string grade = _card.GetCardGrade().ToString();
        string Name = _card.GetCardName();
        cardDataManager.AddCardToMyCardsList(type, grade, Name);
    }

    public void CloseUpgradeSuccessPanel()
    {
        upgradeSuccessUI.gameObject.SetActive(false);
    }

}
