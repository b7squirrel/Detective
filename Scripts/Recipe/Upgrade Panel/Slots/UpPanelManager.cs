using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpPanelManager : MonoBehaviour
{
    #region 카드 관련 변수
    [field: SerializeField]
    CardData CardToUpgrade { get; set; } // 업그레이드 슬롯에 올라가 있는 카드
    CardData cardToFeed; // 재료로 쓸 카드. 지금 드래그 하는 카드

    bool isMergeDone; // 머지 된 후에는 OnEnable에서 강제로 Weapon으로 초기화 되는 것을 피하려고
    bool activated; // 다른 탭에 갔다가 오는지 체크하기 위해
    string currentTabType; // 탭이 바뀌는지 바뀌지 않는지 체크하기 위해
    bool isUpSlotCanceled; // 업 슬롯에 있던 카드를 클릭해서 취소하면
    #endregion

    #region 참조 변수
    CardDataManager cardDataManager;
    CardsDictionary cardsDictionary;
    CardList cardList;
    SetCardDataOnSlot setCardDataOnSlot; // CardData를 Slot에 넣음
    UpPanelUI upPanelUI; // UI 관련 클래스
    MainMenuManager mainMenuManager; // 탭을 아래로 내리기 위해

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

    [SerializeField] UpTabManager upTabManager; // 탭을 업데이트 하기 위한 참조
    [SerializeField] GameObject blockTouchPanel; // 합성 연출 동안 터치가 안되도록 하기
    #endregion

    #region Unity Callback 함수
    void Awake()
    {
        setCardDataOnSlot = GetComponent<SetCardDataOnSlot>();
        cardDataManager = FindObjectOfType<CardDataManager>();
        cardsDictionary = FindObjectOfType<CardsDictionary>();
        cardList = FindObjectOfType<CardList>();
        upPanelUI = GetComponent<UpPanelUI>();
        mainMenuManager = FindObjectOfType<MainMenuManager>();

        upTabManager = GetComponentInChildren<UpTabManager>();

        upCardSlot.EmptySlot();
        matCardSlot.EmptySlot();

        blockTouchPanel.SetActive(false);

        CloseAskUnequipPopup();
    }
    void OnEnable()
    {
        activated = true;
        currentTabType = "Weapon";
        GetIntoAllField("Weapon");
    }
    #endregion

    #region upField, matField 상태 전환
    public void GetIntoMatField()
    {
        ClearAllFieldSlots();

        allField.gameObject.SetActive(false);
        Debug.Log("All Field Activated");
        matField.gameObject.SetActive(true);
        StartCoroutine(GenMatCardListCo());
    }
    IEnumerator GenMatCardListCo()
    {
        yield return new WaitForSeconds(.15f);
        matField.GenerateMatCardsList(CardToUpgrade);
    }

    /// <summary>
    /// Characters 혹은 Items 탭을 보여준다
    /// </summary>
    public void GetIntoAllField(string _thisCardType)
    {
        // 새로 패널로 들어오거나, 머지를 했거나 탭이 바뀌었다면 리프레시
        if (!isMergeDone &&
            currentTabType == _thisCardType &&
            !activated && !isUpSlotCanceled) return;
        SetUpSlotCanceled(false);

        currentTabType = _thisCardType;
        SetMergeDoneState(false);
        activated = false;

        ClearAllFieldSlots(); // allField, matField의 슬롯들을 모두 파괴
        allField.gameObject.SetActive(true);
        matField.gameObject.SetActive(false);

        upCardSlot.EmptySlot();
        matCardSlot.EmptySlot();
        upSuccessSlot.EmptySlot();

        upPanelUI.UpSlotCanceled();
        upPanelUI.ResetScrollContent();

        // 카드타입 인자를 비워 놓는다면 up slot에 올라가 있는 카드 타입들만 필드에 진열하기
        // 머지에 성공하면 항상 ""를 인자로 가지고 온다.
        if (_thisCardType == "")
        {
            allField.GenerateAllCardsOfType(GetMyCardsListOnCardType(upCardSlot.GetCardData().Type));
            upTabManager.SetTab(upCardSlot.GetCardData().Type);
            SetMergeDoneState(false);
        }
        else
        {
            List<CardData> cards = GetMyCardsListOnCardType(_thisCardType);
            allField.GenerateAllCardsOfType(cards);
            upTabManager.SetTab(_thisCardType);
        }

        upPanelUI.Init();
    }

    public void SetUpSlotCanceled(bool _isCanceled)
    {
        isUpSlotCanceled = _isCanceled;
    }

    // 머지 후 탭 해서 계속하기 버튼을 클릭하면 실행
    public void SetMergeDoneState(bool _isMergeDone)
    {
        isMergeDone = _isMergeDone;
    }

    /// <summary>
    /// 인자로 넘겨받은 카드와 같은 타입의 카드만 추려내기
    /// </summary>
    List<CardData> GetMyCardsListOnCardType(string _cardType)
    {
        List<CardData> myCardsList = cardDataManager.GetMyCardList();
        List<CardData> pickedCardsList = new();
        for (int i = 0; i < myCardsList.Count; i++)
        {
            if (myCardsList[i].Type == _cardType)
            {
                pickedCardsList.Add(myCardsList[i]);
            }
        }
        return pickedCardsList;
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
        if (_cardData.Level != StaticValues.MaxLevel) // 최고 레벨이 아니면 합성 불가
        {
            Debug.Log("최고 레벨의 카드만 합성이 가능합니다");
            return;
        }

        if (upCardSlot.IsEmpty) // 슬롯 위에 카드가 없고 최고 레벨이라면 무조건 올릴 수 있다
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
        // 리드 오리는 무조건 재료가 될 수 없음
        if(_cardData.StartingMember == StartingMember.Zero.ToString())
        {
            Debug.Log("리드 오리는 재료 카드로 사용할 수 없습니다.");
            return;
        }

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
        if (CardToUpgrade.Level != StaticValues.MaxLevel)
        {
            Debug.Log("최고 레벨의 카드만 합성이 가능합니다");
            return;
        }
        if(CardToUpgrade.EvoStage != _cardData.EvoStage)
        {
            Debug.Log("같은 합성 등급의 카드만 합성이 가능합니다.");
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
            else // 장비가 두 개 이상 장착된 상태라면 (오리 카드는 장착이 하나도 없는 경우는 없음)
            {
                isEquipped = true;
            }
        }
        else // 장비 카드라면
        {
            EquipmentCard equipCard = cardList.FindEquipmentCard(_cardData);

            // 장비가 장착되어 있지 않다면 무조건 Acquire
            if (equipCard.IsEquipped == false)
            {
                AcquireCard(_cardData);
                return;
            }

            // 다른 오리에 장착되어 있지만 그것이 필수 장비라면
            if (equipCard.IsEquipped)
            {
                if (_cardData.EquipmentType == equipCard.EquippedWho.EssentialEquip)
                {
                    AcquireCard(_cardData); // 그냥 슬롯에 카드를 올리기
                    return;
                }
            }
            // 필수 오리가 아니면서 다른 오리에 장착되어 있다면
            isEquipped = true;
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
        // isGradeUp이 필요하지 않은 것 같다. 정리할 것. 등급이 올라갈 일은 없음. 다른 등급은 뽑기로만 획득 가능함.

        bool isGradeUp = false; // 등급이 올라갔다면 타이틀 리본에 반짝 이펙트를 주기 위해
        int newCardGrade = CardToUpgrade.Grade;
        int newCardEvoStage = CardToUpgrade.EvoStage + 1;
        CardToUpgrade.PassiveSkill = UnityEngine.Random.Range(1, StaticValues.MaxSkillNumbers + 1); // 스킬을 랜덤하게 다시 부여

        if (newCardEvoStage > StaticValues.MaxEvoStage - 1) // Evo 레벨이 최고 레벨을 초과하면
        {
            newCardGrade++; // 다음 등급으로

            if (newCardGrade > StaticValues.MaxGrade - 1) // Grade가 최고 등급을 초과하면
            {
                // Grade와 EvoStage를 최고 등급으로 되돌림
                newCardGrade = StaticValues.MaxGrade - 1;
                newCardEvoStage = StaticValues.MaxEvoStage - 1;
            }
            else
            {
                newCardEvoStage = 0; // 다음 등급이 되면 evo 레벨은 초기화
                isGradeUp = true;
            }
        }

        // 생성된 카드를 내 카드 리스트에 저장
        //CardData newCardData = GenUpgradeCardData(CardToUpgrade.Name, newCardGrade);
        CardToUpgrade.Grade = newCardGrade;
        CardToUpgrade.EvoStage = newCardEvoStage;
        CardToUpgrade.Level = 1;

        cardDataManager.RemoveCardFromMyCardList(cardToFeed);

        cardList.InitCardList(); // 장비 슬롯들 업데이트

        // 합성 연출 후 강화 성공 패널로
        matField.gameObject.SetActive(false);

        StartCoroutine(UpgradeUICo(CardToUpgrade, isGradeUp));
    }

    IEnumerator UpgradeUICo(CardData upgradedCardData, bool isGradeUp)
    {
        // 합성 연출 동안 터치가 안되도록 하기
        blockTouchPanel.SetActive(true);

        // 아래 탭들을 밑으로 내리기. 중간에 다른 탭으로 이동할 수 없도록
        mainMenuManager.SetActiveBottomTabs(false);

        // 강화 연출 UI
        upPanelUI.MergingCardsUI();
        upPanelUI.OffUpgradeConfirmationUI();

        yield return new WaitForSeconds(.15f);  // 1.5초 동안 두 카드가 가운데로 모인 후
        upCardSlot.EmptySlot();
        matCardSlot.EmptySlot();
        ClearAllFieldSlots();
        upPanelUI.DeactivateSpecialSlots();
        upPanelUI.OpenUpgradeSuccessPanel(upgradedCardData, isGradeUp);

        // 다시 터치가 가능하도록
        blockTouchPanel.SetActive(false);

        // 합성 성공 패널이 활성화 된 후에 실행되어야 슬롯이 empty된 상태로 끝나지 않게 된다
        setCardDataOnSlot.PutCardDataIntoSlot(upgradedCardData, upSuccessSlot);
    }

    // // 아이디를 발급 받지 않은 card data 생성
    // public CardData GenUpgradeCardData(string _cardName, int _grade)
    // {
    //     List<CardData> newCard = new();
    //     newCard.AddRange(cardsDictionary.GetCardPool());

    //     List<CardData> sameNameCardData = newCard.FindAll(x => x.Name == _cardName);
    //     Debug.Log("찾는 카드 = " + _cardName);
    //     CardData picked = sameNameCardData.Find(x => x.Grade == _grade);
    //     if (picked == null)
    //     {
    //         Debug.Log("검색된 카드가 없습니ㅏㄷ");
    //     }

    //     return picked;
    // }
    #endregion

    #region Refresh All, Mat Field
    public void ClearAllFieldSlots()
    {
        allField.ClearSlots();
        matField?.ClearSlots();
    }
    #endregion
}