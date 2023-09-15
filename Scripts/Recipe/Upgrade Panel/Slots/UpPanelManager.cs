using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpPanelManager : MonoBehaviour
{
    #region 카드 관련 변수
    [field : SerializeField]
    CardData CardToUpgrade { get; set; } // 업그레이드 슬롯에 올라가 있는 카드
    CardData cardToFeed; // 재료로 쓸 카드. 지금 드래그 하는 카드
    #endregion

    #region 참조 변수
    CardDataManager cardDataManager;
    CardsDictionary cardsDictionary;
    DisplayCardOnSlot displayCardOnSlot; // 슬롯 위에 있는 카드 Display
    UpPanelUI upPanelUI; // UI 관련 클래스

    // 카드들이 보여지는 Field
    [SerializeField] AllField allField;
    [SerializeField] MatField matField;

    // 업그레이드 슬롯, 재료 슬롯
    [SerializeField] CardSlot upCardSlot;
    [SerializeField] CardSlot matCardSlot;
    [SerializeField] CardSlot upSuccessSlot;
    #endregion

    #region Unity Callback 함수
    void Awake()
    {
        displayCardOnSlot = GetComponent<DisplayCardOnSlot>();
        cardDataManager = FindObjectOfType<CardDataManager>();
        cardsDictionary = FindObjectOfType<CardsDictionary>();
        upPanelUI = GetComponent<UpPanelUI>();

        upCardSlot.EmptySlot();
        matCardSlot.EmptySlot();
        GetIntoAllField();
    }
    void OnEnable()
    {
        GetIntoAllField();
    }
    #endregion

    #region upField, matField 상태 전환
    public void GetIntoMatField()
    {
        ClearAllFieldSlots();
        
        allField.gameObject.SetActive(false);
        matField.gameObject.SetActive(true);
        StartCoroutine(GenMatCardListCo());
    }
    IEnumerator GenMatCardListCo()
    {
        yield return new WaitForSeconds(.15f);
        matField.GenerateMatCardsList(CardToUpgrade);
    }

    public void GetIntoAllField()
    {
        ClearAllFieldSlots(); // allField, matField의 슬롯들을 모두 파괴
        allField.gameObject.SetActive(true);
        matField.gameObject.SetActive(false);

        upCardSlot.EmptySlot();
        matCardSlot.EmptySlot();
        upSuccessSlot.EmptySlot();

        upPanelUI.UpSlotCanceled();
        upPanelUI.ResetScrollContent();
        allField.GenerateAllCardsOfType(cardDataManager.GetMyCardList());

        upPanelUI.Init();
    }

    public void GetIntoConfirmation()
    {
        ClearAllFieldSlots();
        upPanelUI.UpgradeConfirmationUI(); // 합성 확인 창 UI
    }

    public void BackToMatField()
    {
        ClearAllFieldSlots();
        
        allField.gameObject.SetActive(false);
        matField.gameObject.SetActive(true);
        StartCoroutine(GenMatCardListCo());

        matCardSlot.EmptySlot();

        upPanelUI.OffUpgradeConfirmationUI();
        upPanelUI.MatSlotCanceled();
    }
    #endregion

    #region Acquire Card
    public void AcquireCard(CardData cardData)
    {
        // 최고 레벨 카드는 올릴 수 없음
        // if (cardData.Grade == "Legendary")
        // {
        //     Debug.Log("최고 레벨 카드는 업그레이드 할 수 없습니다");
        //     return;
        // }

        // 업그레이드 카드의 경우
        if (upCardSlot.IsEmpty()) // 슬롯 위에 카드가 없다면 무조건 올릴 수 있다
        {
            CardToUpgrade = cardData;

            // upSlot을 옆으로 이동 시키고 matSlot 나타나게 하기
            upPanelUI.UpCardAcquiredUI();

            // 재료카드 패널 열기
            GetIntoMatField();

            // 업그레이드 슬롯 위 카드 표시
            displayCardOnSlot.DispCardOnSlot(CardToUpgrade, upCardSlot);
            return;
        }

        
        // 재료카드에 이미 다른 카드가 올라가 있는 경우
        if (matCardSlot.IsEmpty() == false)
            return;

        // 재료 카드의 경우
        if (CardToUpgrade.Grade != cardData.Grade)
        {
            Debug.Log("같은 등급을 합쳐줘야 합니다");
            return;
        }
        if (CardToUpgrade.Name != cardData.Name)
        {
            Debug.Log("같은 이름의 카드를 합쳐줘야 합니다.");
            return;
        }
        // 나머지는 재료 카드일 경우 뿐이다.
        cardToFeed = cardData; ; // 비어 있지 않다면 지금 카드는 재료 카드임
        upPanelUI.MatCardAcquiredUI();
        displayCardOnSlot.DispCardOnSlot(cardData, matCardSlot);
        GetIntoConfirmation(); // 합성 확인 화면으로

        matField.gameObject.SetActive(false);
    }
    #endregion

    #region 업그레이드
    public void UpgradeCard()
    {
        int newCardGrade = new GradeConverter().ConvertStringToInt(CardToUpgrade.Grade) + 1;
        if (newCardGrade > 4) { newCardGrade = 4; } // 전설 등급은 합성하면 전설 등급

        string newGrade = ((Grade)newCardGrade).ToString();

        // 생성된 카드를 내 카드 리스트에 저장
        CardData newCardData = GenUpgradeCardData(CardToUpgrade.Name, newGrade);
        cardDataManager.AddUpgradedCardToMyCardList(CardToUpgrade.ID, newCardData);

        cardDataManager.RemoveCardFromMyCardList(CardToUpgrade);// 카드 데이터 삭제
        cardDataManager.RemoveCardFromMyCardList(cardToFeed);

        // 합성 연출 후 강화 성공 패널로
        matField.gameObject.SetActive(false);
        StartCoroutine(UpgradeUICo(newCardData));
    }

    IEnumerator UpgradeUICo(CardData upgradedCardData)
    {
        // 강화 연출 UI
        upPanelUI.MergingCardsUI();
        upPanelUI.OffUpgradeConfirmationUI();

        yield return new WaitForSeconds(.15f);
        upCardSlot.EmptySlot();
        matCardSlot.EmptySlot();
        ClearAllFieldSlots();
        upPanelUI.DeactivateSpecialSlots();
        upPanelUI.OpenUpgradeSuccessPanel(upgradedCardData, displayCardOnSlot);
    }

    public CardData GenUpgradeCardData(string _cardName, string _grade)
    {
        List<CardData> newCard = new();
        newCard.AddRange(cardsDictionary.GetCardPool());

        List<CardData> sameNameCardData = newCard.FindAll(x => x.Name == _cardName);
        CardData picked = sameNameCardData.Find(x => x.Grade == _grade);

        return picked;
    }
    #endregion

    #region Refresh All, Mat Field
    public void ClearAllFieldSlots()
    {
        allField.ClearSlots();
        matField?.ClearSlots();
    }
    #endregion
}