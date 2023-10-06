using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpPanelManager : MonoBehaviour
{
    #region 카드 관련 변수
    [field: SerializeField]
    CardData CardToUpgrade { get; set; } // 업그레이드 슬롯에 올라가 있는 카드
    CardData cardToFeed; // 재료로 쓸 카드. 지금 드래그 하는 카드
    #endregion

    #region 참조 변수
    CardDataManager cardDataManager;
    CardsDictionary cardsDictionary;
    CardList cardList;
    SetCardDataOnSlot setCardDataOnSlot; // CardData를 Slot에 넣음
    UpPanelUI upPanelUI; // UI 관련 클래스

    // 카드들이 보여지는 Field
    [SerializeField] AllField allField;
    [SerializeField] MatField matField;

    // 업그레이드 슬롯, 재료 슬롯
    [SerializeField] CardSlot upCardSlot;
    [SerializeField] CardSlot matCardSlot;
    [SerializeField] CardSlot upSuccessSlot;

    // 다른 카드에 장착되어 있거나, 이미 장착을 하고 있을 때
    [SerializeField] GameObject askUnequipPopup;
    CardData pendingCardData; // 장착 해제 팝업이 띄워져 있는 동안 카드 데이터를 임시 저장
    #endregion

    #region Unity Callback 함수
    void Awake()
    {
        setCardDataOnSlot = GetComponent<SetCardDataOnSlot>();
        cardDataManager = FindObjectOfType<CardDataManager>();
        cardsDictionary = FindObjectOfType<CardsDictionary>();
        cardList = FindObjectOfType<CardList>();
        upPanelUI = GetComponent<UpPanelUI>();

        upCardSlot.EmptySlot();
        matCardSlot.EmptySlot();
        GetIntoAllField();

        CloseAskUnequipPopup();
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
    public void CheckIsEquipped(CardData _cardData)
    {
        // 업그레이드 카드의 경우
        if (upCardSlot.IsEmpty) // 슬롯 위에 카드가 없다면 무조건 올릴 수 있다
        {
            CardToUpgrade = _cardData;

            // upSlot을 옆으로 이동 시키고 matSlot 나타나게 하기
            upPanelUI.UpCardAcquiredUI();

            // 재료카드 패널 열기
            GetIntoMatField();

            // 업그레이드 슬롯 위 카드 표시
            setCardDataOnSlot.PutCardDataIntoSlot(CardToUpgrade, upCardSlot);
            return;
        }

        // 재료카드에 이미 다른 카드가 올라가 있는 경우
        if (matCardSlot.IsEmpty == false)
            return;

        // 재료 카드의 경우
        if (CardToUpgrade.Grade != _cardData.Grade)
        {
            Debug.Log("같은 등급을 합쳐줘야 합니다");
            return;
        }
        if (CardToUpgrade.Name != _cardData.Name)
        {
            Debug.Log("같은 이름의 카드를 합쳐줘야 합니다.");
            return;
        }

        // 장착을 하고 있는지, 장착이 되어 있는지 확인
        bool isEquipped;

        if (_cardData.Type == CardType.Weapon.ToString())
        {
            CharCard charCard = cardList.FindCharCard(_cardData);

            // 그런데 만약 필수 장비 하나만 장착되어 있다면 장착 해제가 필요없다
            int essentialIndex = -1; // 가능하지 않은 수로 초기화

            List<EquipmentCard> equippedCards = new();
            for (int i = 0; i < 4; i++)
            {
                if (charCard.equipmentCards[i] == null)
                    continue;
                equippedCards.Add(charCard.equipmentCards[i]);

                if (charCard.equipmentCards[i].CardData.EquipmentType == _cardData.EssentialEquip)
                {
                    essentialIndex = i;
                }
            }

            // 필수 장비 하나만 장착된 상태라면
            if (equippedCards.Count == 1 && essentialIndex != -1)
            {
                AcquireCard(_cardData); // 슬롯에 카드를 올리기
                return;
            }
            else
            {
                isEquipped = charCard.IsEquipped;
            }
        }
        else
        {
            EquipmentCard equipCard = cardList.FindEquipmentCard(_cardData);

            // 장비가 필수 카드라면
            if (equipCard.CardData.EquipmentType == equipCard.EquippedWho.EquipmentType)
            {
                AcquireCard(_cardData); // 그냥 슬롯에 카드를 올리기
                return;
            }

            isEquipped = equipCard.IsEquipped;
        }

        // 장착을 하고 있다면 장착을 해제할지 물어보는 팝업창을 띄움
        if (isEquipped)
        {
            pendingCardData = _cardData;
            AskUnequip();
        }
        else
        {
            AcquireCard(_cardData);
            return;
        }

        // 장착을 하고 있지 않다면 Acquire Card 실행
    }
    void AcquireCard(CardData cardData)
    {
        // 최고 레벨 카드는 올릴 수 없음
        // if (cardData.Grade == "Legendary")
        // {
        //     Debug.Log("최고 레벨 카드는 업그레이드 할 수 없습니다");
        //     return;
        // }


        // 나머지는 재료 카드일 경우 뿐이다.
        cardToFeed = cardData; ; // 비어 있지 않다면 지금 카드는 재료 카드임
        upPanelUI.MatCardAcquiredUI();
        setCardDataOnSlot.PutCardDataIntoSlot(cardData, matCardSlot);
        GetIntoConfirmation(); // 합성 확인 화면으로

        matField.gameObject.SetActive(false);
    }

    void AskUnequip()
    {
        askUnequipPopup.SetActive(true);
    }
    // 버튼 이벤트
    public void AcceptUnequip()
    {
        // 장비 해제
        if (pendingCardData.Type == CardType.Weapon.ToString())
        {
            int numberOfEquipments = 0;
            CharCard charCard = cardList.FindCharCard(pendingCardData);
            for (int i = 0; i < 4; i++)
            {
                if (charCard.equipmentCards[i] == null)
                    continue;
                // 필수 장비는 해제하지 않고 건너뜀
                if (charCard.equipmentCards[i].CardData.EquipmentType == charCard.CardData.EssentialEquip)
                    continue;
                cardList.UnEquip(pendingCardData, charCard.equipmentCards[i]);
                numberOfEquipments++;
            }
            Debug.Log(pendingCardData.Name + "에 장착되어 있던 " + numberOfEquipments + "개의 장비가 해제 되었습니다.");
        }
        else
        {
            EquipmentCard _equipmentCard = cardList.FindEquipmentCard(pendingCardData);

            if (_equipmentCard.EquippedWho.EssentialEquip == _equipmentCard.CardData.EquipmentType)
                return;

            cardList.UnEquip(_equipmentCard.EquippedWho, _equipmentCard);

            Debug.Log(pendingCardData.Name + "을 " + _equipmentCard.EquippedWho.Name + " 에서 해제했습니다.");
        }

        AcquireCard(pendingCardData);
        pendingCardData = null;
        CloseAskUnequipPopup();
    }
    public void CloseAskUnequipPopup()
    {
        askUnequipPopup.SetActive(false);
        pendingCardData = null;
    }
    #endregion

    #region 업그레이드
    public void UpgradeCard()
    {
        int newCardGrade = new Convert().GradeToInt(CardToUpgrade.Grade) + 1;
        if (newCardGrade > 4) { newCardGrade = 4; } // 전설 등급은 합성하면 전설 등급

        string newGrade = ((Grade)newCardGrade).ToString();

        // 생성된 카드를 내 카드 리스트에 저장
        CardData newCardData = GenUpgradeCardData(CardToUpgrade.Name, newGrade);
        newCardData.ID = CardToUpgrade.ID;
        cardDataManager.AddUpgradedCardToMyCardList(newCardData);

        cardDataManager.RemoveCardFromMyCardList(CardToUpgrade);// 카드 데이터 삭제
        cardDataManager.RemoveCardFromMyCardList(cardToFeed);

        cardList.InitCardList(); // 장비 슬롯들 업데이트

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
        upPanelUI.OpenUpgradeSuccessPanel(upgradedCardData);

        // 합성 성공 패널이 활성화 된 후에 실행되어야 슬롯이 empty된 상태로 끝나지 않게 된다
        setCardDataOnSlot.PutCardDataIntoSlot(upgradedCardData, upSuccessSlot);
    }

    // 아이디를 발급 받지 않은 card data 생성
    public CardData GenUpgradeCardData(string _cardName, string _grade)
    {
        List<CardData> newCard = new();
        newCard.AddRange(cardsDictionary.GetCardPool());
        foreach (var item in newCard)
        {
            Debug.Log(item.Name);
        }

        List<CardData> sameNameCardData = newCard.FindAll(x => x.Name == _cardName);
        Debug.Log("찾는 카드 = " + _cardName);
        CardData picked = sameNameCardData.Find(x => x.Grade == _grade);
        if (picked == null)
        {
            Debug.Log("검색된 카드가 없습니ㅏㄷ");
        }

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