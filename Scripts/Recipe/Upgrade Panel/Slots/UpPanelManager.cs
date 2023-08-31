using System;
using System.Collections;
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
    DisplayCardOnSlot displayCardOnSlot; // 슬롯 위에 있는 카드 Display
    UpPanelUI upPanelUI; // UI 관련 클래스
    [SerializeField] GameObject upgradeSuccessPanel; 

    // 카드들이 보여지는 Field
    [SerializeField] AllField allField;
    [SerializeField] MatField matField;

    // 업그레이드 슬롯, 재료 슬롯
    [SerializeField] CardSlot upCardSlot;
    [SerializeField] CardSlot matCardSlot;
    #endregion

    #region Unity Callback 함수

    void Awake()
    {
        displayCardOnSlot = GetComponent<DisplayCardOnSlot>();
        cardDataManager = FindObjectOfType<CardDataManager>();
        upPanelUI = GetComponent<UpPanelUI>();

        ClearAllFieldSlots();
        upCardSlot.EmptySlot();
        matCardSlot.EmptySlot();

        upgradeSuccessPanel.SetActive(false);
    }
    #endregion

    #region upField, matField 상태 전환
    public void GetIntoMatField(CardData cardOnUpSlot)
    {
        allField.gameObject.SetActive(false);
        matField.gameObject.SetActive(true);
        matField.GenerateMatCardsList(cardOnUpSlot);
    }

    public void GetIntoAllField()
    {
        ClearAllFieldSlots(); // allField, matField의 슬롯들을 모두 파괴
        allField.gameObject.SetActive(true);
        matField.gameObject.SetActive(false);
    }

    public void BackToMatField()
    {
        // slotsAllCards.gameObject.SetActive(false);
        // slotsMatCards.gameObject.SetActive(true);

        // // MatCards 리스트 복구
        // List<CardData> matCardDataList = new();
        // matCardDataList.AddRange(slotsMatCards.GetCardDatas());
        // slotsMatCards.ClearmatCardsSlots(); // 재료 슬롯들의 카드만 갱신
        // slotsMatCards.SetMatCards(matCardDataList);

        // slotUpCardUI.ActivateDarkPanel(false);
    }
    #endregion

    #region Acquire Card
    public void AcquireCard(CardData cardData)
    {
        // 최고 레벨 카드는 올릴 수 없음
        if (cardData.Grade == "Legendary")
        {
            Debug.Log("최고 레벨 카드는 업그레이드 할 수 없습니다");
            return;
        }

        // 업그레이드 카드의 경우
        if (upCardSlot.IsEmpty) // 슬롯 위에 카드가 없다면 무조건 올릴 수 있다
        {
            CardToUpgrade = cardData;

            // upSlot을 옆으로 이동 시키고 matSlot 나타나게 하기
            upPanelUI.UpCardAcquiredUI();

            // 재료카드 패널 열기
            GetIntoMatField(cardData);

            // 업그레이드 슬롯 위 카드 표시
            displayCardOnSlot.DispCardOnSlot(cardData, upCardSlot);
            return;
        }

        
        // 재료카드에 이미 다른 카드가 올라가 있는 경우
        if (matCardSlot.IsEmpty == false)
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
        displayCardOnSlot.DispCardOnSlot(cardData, matCardSlot);
        upPanelUI.UpgradeConfirmationUI(); // 합성 확인 창 UI

        matField.gameObject.SetActive(false);
    }
    #endregion

    #region 업그레이드
    public void UpgradeCard()
    {
        int newCardGrade = new GradeConverter().ConvertStringToInt(CardToUpgrade.Grade) + 1;
        if (newCardGrade > 4) { Debug.Log("업그레이드 된 카드가 최고등급을 넘습니다. 확인 할 것"); }

        string newGrade = ((Grade)newCardGrade).ToString();
        string type = CardToUpgrade.Type;

        // 생성된 카드를 내 카드 리스트에 저장
        CardData newCardData = cardDataManager.GenNewCardData(type, newGrade, CardToUpgrade.Name);
        cardDataManager.AddCardToMyCardsList(newCardData);
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

        yield return new WaitForSeconds(.16f);
        upCardSlot.EmptySlot();
        matCardSlot.EmptySlot();
        upCardSlot.gameObject.SetActive(false);
        matCardSlot.gameObject.SetActive(false);
        OpenUpgradeSuccessPanel(upgradedCardData);
    }
    #endregion

    #region Upgrade Success Panel
    public void OpenUpgradeSuccessPanel(CardData cardData)
    {
        upgradeSuccessPanel.gameObject.SetActive(true);
        CardSlot successCardSlot = upgradeSuccessPanel.GetComponentInChildren<CardSlot>();
        displayCardOnSlot.DispCardOnSlot(cardData, successCardSlot);
    }

    public void CloseUpgradeSuccessUI()
    {
        upgradeSuccessPanel.gameObject.SetActive(false);
        upCardSlot.gameObject.SetActive(true);
        matCardSlot.gameObject.SetActive(true);
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