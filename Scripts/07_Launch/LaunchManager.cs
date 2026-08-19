using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchManager : MonoBehaviour
{
    [SerializeField] CardSlot leadOriSlot;
    CardDataManager cardDataManager => CardDataManager.Instance;
    [SerializeField] SetCardDataOnSlot setCardDataOnSlot;
    [SerializeField] StatManager statManager;
    [SerializeField] StartingDataContainer startingDataContainer;
    CardSlotManager cardSlotManager;
    PlayerDataManager playerDataManager;

    [SerializeField] GameObject fieldSlotPanel;
    [SerializeField] AllField field;

    [SerializeField] GameObject startButton;
    [SerializeField] GameObject BgToExitField;
    [SerializeField] GameObject backButton;

    [SerializeField] StageInfoUI stageInfoUi;
    [SerializeField] StageInfo stageInfo;

    CardData currentLead;
    OriAttribute currentAttr;
    Animator panelAnim;

    [Header("일일 보상 버튼")]
    [SerializeField] ButtonBadgeUI dailyButtonUI;

    [Header("리드 오리 안내 문구")]
    [SerializeField] GameObject messageLeadOri; // ⭐ 추가: "message Lead ori" 오브젝트

    [Header("판매 패널")]
    [SerializeField] SellPanelManager sellPanelManager;
    [SerializeField] GameObject sellPanelObject;

    // ⭐ 추가: 동료 오리 슬롯 (최대 4마리). 인스펙터에서 크기 4로 만들고 각 슬롯 오브젝트를 순서대로 드래그
    [Header("동료 오리 슬롯 (최대 4마리)")]
    [SerializeField] CardSlot[] companionSlots = new CardSlot[4];

    // ⭐ 추가: 각 동료 슬롯 아래에 배치한 "비우기" 버튼. companionSlots와 인덱스가 1:1로 대응.
    // 슬롯이 비어있을 땐 숨기고, 카드가 배정되면 보여준다.
    [Header("동료 오리 슬롯 비우기 버튼 (최대 4개)")]
    [SerializeField] GameObject[] companionClearButtons = new GameObject[4];

    // ⭐ 추가: 각 동료 슬롯에 배정된 카드. companionSlots와 인덱스가 1:1로 대응 (빈 슬롯은 null 허용)
    CardData[] companionCardData = new CardData[4];

    // ⭐ 추가: 현재 필드가 열려서 카드를 고르는 중인 슬롯 번호 (0=리드, 1~4=동료, -1=편집 중 아님)
    int editingSlotIndex = -1;

    // ⭐ 추가: 필드 후보 카드의 종(weaponData.Name) 조회용
    CardsDictionary cardsDictionary => CardsDictionary.Instance;

    // ⭐ 추가: 동료 슬롯 인덱스(0~3) ↔ StartingMember 플래그 매핑. 리드가 Zero를 쓰는 것과 같은 방식으로 저장/복원에 사용
    static readonly StartingMember[] CompanionStartingMembers =
    {
        StartingMember.First, StartingMember.Second, StartingMember.Third, StartingMember.Forth
    };

    void Awake()
    {
        // ⭐ 추가: 시작 시 동료 슬롯을 모두 빈 상태로 초기화
        for (int i = 0; i < companionSlots.Length; i++)
        {
            SetCompanionSlotEmptyButClickable(i);
        }
    }

    // ⭐ 추가: EmptySlot()은 카드 그림뿐 아니라 클릭 버튼까지 꺼버리므로,
    // 스쿼드 슬롯(리드/동료)은 비어있어도 항상 탭 가능하도록 버튼을 다시 켜준다.
    // 비어있는 상태이므로 "비우기" 버튼도 함께 숨긴다.
    void SetCompanionSlotEmptyButClickable(int companionIndex)
    {
        if (companionSlots[companionIndex] == null) return;

        companionSlots[companionIndex].EmptySlot();

        CardDisp disp = companionSlots[companionIndex].GetComponent<CardDisp>();
        if (disp != null) disp.SetButtonActive(true);

        SetCompanionClearButtonActive(companionIndex, false);
    }

    // ⭐ 추가: 동료 슬롯 "비우기" 버튼 표시/숨김
    void SetCompanionClearButtonActive(int companionIndex, bool active)
    {
        if (companionClearButtons == null) return;
        if (companionIndex < 0 || companionIndex >= companionClearButtons.Length) return;
        if (companionClearButtons[companionIndex] == null) return;

        companionClearButtons[companionIndex].SetActive(active);
    }

    void OnEnable()
    {
        // stageInfoUi.PlayFromStart();
        Debug.Log($"[LaunchManager] OnEnable 호출됨. 프레임={Time.frameCount}\n{System.Environment.StackTrace}");
        // ⭐ 초기화 대기
        StartCoroutine(InitLead());

        if (cardSlotManager == null)
            cardSlotManager = FindObjectOfType<CardSlotManager>();
        // cardSlotManager.SettrigerAnim("Off");

        if (panelAnim == null) panelAnim = sellPanelObject.GetComponent<Animator>();
        panelAnim.SetTrigger("Up");

        // ⭐ 추가: 튜토리얼 단계 구독 및 현재 상태 즉시 반영
        TutorialManager.OnStepChanged += OnTutorialStepChanged;
        if (TutorialManager.instance != null)
            UpdateLeadOriMessageVisibility(TutorialManager.instance.CurrentStep);
    }

    void OnDisable()
    {
        BgToExitField.SetActive(false);
        startButton.SetActive(false);
        backButton.SetActive(false);

        // ⭐ 추가: 구독 해제
        TutorialManager.OnStepChanged -= OnTutorialStepChanged;
    }

    public void UpdateStageInfo()
    {
        InitStageInfo();
    }

    void InitStageInfo()
    {
        int stageNum = FindObjectOfType<PlayerDataManager>().GetCurrentStageNumber();
        // ✅ currentStage 변수 제거 (Title 사용 없음)
        stageInfoUi.InitStageInfoUI();
    }

    // 개선된 InitLead - GameInitializer 대기
    IEnumerator InitLead()
    {
        startButton.SetActive(false);
        
        // GameInitializer가 모든 초기화를 완료할 때까지 대기
        Logger.Log("[LaunchManager] 게임 초기화 대기 중...");
        yield return new WaitUntil(() => GameInitializer.IsInitialized);
        Logger.Log("[LaunchManager] 게임 초기화 완료, 리드 설정 시작");
        
        // 리드 오리 찾기
        CardData lead = cardDataManager.GetMyCardList().Find(
            x => x.StartingMember == StartingMember.Zero.ToString()
        );
        
        if (lead == null)
        {
            Logger.LogError("[LaunchManager] 리드 오리를 찾을 수 없습니다!");
            yield break;
        }
        
        SetLead(lead);

        // ⭐ 추가: 저장된 동료 오리들도 함께 복원 (StartingMember.First~Forth 플래그로 카드 재조회)
        // 패널이 열릴 때마다 다시 실행되므로, 다른 곳에서 동료의 장비를 바꿨어도 여기서 항상 최신 상태로 갱신됨
        LoadCompanionsFromSave();
        
        // UI 업데이트 대기
        yield return new WaitForSeconds(.03f);
        
        startButton.SetActive(true);
        InitStageInfo();
        
        Logger.Log("[LaunchManager] 리드 초기화 완료");

        // 일일 보상 버튼 UI 업데이트
        UpdateDailyRewardBadge();
    }

    public void UpdateDailyRewardBadge()
    {
        // 일일 보상 버튼 UI 업데이트
        if(playerDataManager == null) playerDataManager = PlayerDataManager.Instance;
        bool shouldShow = !playerDataManager.HasTakenDailyReward();
        dailyButtonUI.ActivateBadge(shouldShow);
    }
    
    /// <summary>
    /// StartingMember.First~Forth 플래그로 저장된 동료 카드를 다시 찾아서 4개 슬롯에 복원한다.
    /// 리드처럼 패널이 열릴 때마다 호출되므로, 장비가 바뀐 뒤에도 항상 최신 상태를 보여준다.
    /// 저장된 카드가 없거나(최초 실행) 판매되어 사라졌다면 해당 슬롯은 자동으로 빈 상태가 된다.
    /// </summary>
    void LoadCompanionsFromSave()
    {
        var myCardList = cardDataManager.GetMyCardList();

        for (int i = 0; i < companionSlots.Length; i++)
        {
            string flag = CompanionStartingMembers[i].ToString();
            CardData savedCard = myCardList.Find(x => x.StartingMember == flag);

            if (savedCard == null)
            {
                companionCardData[i] = null;
                SetCompanionSlotEmptyButClickable(i);
                continue;
            }

            companionCardData[i] = savedCard;
            setCardDataOnSlot.PutCardDataIntoSlot(savedCard, companionSlots[i]);
            SetCompanionClearButtonActive(i, true);
        }

        RefreshCompanionsInContainer();
    }

    void SetLead(CardData lead)
    {
        currentLead = lead;

        // 리드오리 attr update
        currentAttr = statManager.GetLeadAttribute(currentLead);

        setCardDataOnSlot.PutCardDataIntoSlot(lead, leadOriSlot);

        startingDataContainer.SetLead(lead, currentAttr);
    }

    // ⭐ 추가: 외부(GameInitializer 등)에서 장비 지급 완료 후 launch panel을 다시 그리기 위해 호출
    public void RefreshLeadDisplay()
    {
        if (currentLead == null) return;
        SetLead(currentLead);   // ⭐ PutCardDataIntoSlot만이 아니라 startingDataContainer.SetLead()까지 함께 갱신
    }

    // ⭐ 개선된 UpdateLead - 배치 모드 적용
    public void UpdateLead(CardData newLead)
    {
        Logger.Log("[LaunchManager] Update Lead");
        
        // ⭐ 배치 모드로 두 번의 업데이트를 한 번에
        cardDataManager.BeginBatchOperation();
        
        cardDataManager.UpdateStartingmemberOfCard(currentLead, "N");
        cardDataManager.UpdateStartingmemberOfCard(newLead, "Zero");
        
        cardDataManager.EndBatchOperation();
        cardDataManager.RefreshCardList();

        CardSlot currentCardSlot = cardSlotManager.GetSlotByID(currentLead.ID);
        cardSlotManager.UpdateCardDisplay(currentCardSlot.GetCardData());
        cardSlotManager.UpdateCardDisplay(newLead);

        SetLead(newLead);
        StartCoroutine(UpdateLeadCo());
    }

    IEnumerator UpdateLeadCo()
    {
        yield return new WaitForSeconds(.2f);
        CloseField();
        BgToExitField.SetActive(false);
        backButton.SetActive(false);
    }

    public void SetAllFieldTypeOf(string oriType, CardData currentLeadOri)
{
    Debug.Log($"[LaunchManager] SetAllFieldTypeOf 시작. oriType={oriType}, currentLeadOri={(currentLeadOri == null ? "NULL" : currentLeadOri.Name)}");

    List<CardData> card = new();

    // ⭐ cardDataManager null 체크 추가
    if (cardDataManager == null)
    {
        Debug.LogError("[LaunchManager] cardDataManager가 null입니다!");
        return;
    }

    var myCardList = cardDataManager.GetMyCardList();
    if (myCardList == null)
    {
        Debug.LogError("[LaunchManager] cardDataManager.GetMyCardList()가 null입니다!");
        return;
    }

    card = myCardList.FindAll(x => x.Type == oriType);
    Debug.Log($"[LaunchManager] 필터링된 card.Count = {card.Count}");

    // ⭐ 추가: 이미 스쿼드(리드+동료)에 있는 종은 후보에서 제외
    card = FilterOutSquadDuplicates(card);
    Debug.Log($"[LaunchManager] 스쿼드 중복 제외 후 card.Count = {card.Count}");

    if (cardSlotManager == null)
        cardSlotManager = FindObjectOfType<CardSlotManager>();

    Debug.Log("[LaunchManager] SettrigerAnim(\"Launch\") 호출 직전");
    cardSlotManager.SettrigerAnim("Launch");
    Debug.Log("[LaunchManager] SettrigerAnim(\"Launch\") 호출 완료");

    // ⭐ field null 체크 추가
    if (field == null)
    {
        Debug.LogError("[LaunchManager] field(AllField)가 null입니다!");
        return;
    }

    Debug.Log("[LaunchManager] GenerateAllCardsOfType 호출 직전");
    field.GenerateAllCardsOfType(card, "Launch");
    Debug.Log("[LaunchManager] GenerateAllCardsOfType 호출 완료");

    BgToExitField.SetActive(true);
    startButton.SetActive(false);
    backButton.SetActive(true);

    cardSlotManager.InitialSortingByGrade();

    Debug.Log("[LaunchManager] SetAllFieldTypeOf 끝까지 도달함");
}

    public void SetHalo(bool _isActive)
    {
        CardSlot[] _cardSlot = field.GetComponentsInChildren<CardSlot>();
        for (int i = 0; i < _cardSlot.Length; i++)
        {
            if (_cardSlot[i].GetCardData().ID == currentLead.ID)
            {
                _cardSlot[i].GetComponent<CardDisp>().SetHalo(true);
                return;
            }
        }
    }

    // void UpdateDailyRewardBadge()
    // {
    //     PlayerDataManager pdm = PlayerDataManager.Instance;

    //     if (pdm == null || dailyRewardBadge == null) return;

    //     // 받지 않았으면 빨간 점 표시
    //     bool shouldShow = !pdm.HasTakenDailyReward();
    //     dailyRewardBadge.SetActive(shouldShow);
    // }

    public void CloseField()
    {
        cardSlotManager.SettrigerAnim("Off");
        startButton.SetActive(true);
        backButton.SetActive(false);
    }

    // ⭐ 추가: 튜토리얼 완료 전엔 숨기고, 완료된 순간부터는 계속 표시
    void OnTutorialStepChanged(TutorialStep step)
    {
        UpdateLeadOriMessageVisibility(step);
    }

    void UpdateLeadOriMessageVisibility(TutorialStep step)
    {
        if (messageLeadOri == null) return;

        if (step == TutorialStep.Completed)
            messageLeadOri.SetActive(true);
        // Completed가 아니면 아무것도 하지 않음 → 한 번 켜지면 계속 유지되고, 완료 전에는 계속 꺼진 상태
    }

    #region 판매 패널 연결
    public void OnSellPanelButton()
    {
        sellPanelManager.gameObject.SetActive(true);
        sellPanelObject.SetActive(true);
        panelAnim.SetTrigger("Up");
    }

    public void OnReturnFromSellPanel()
    {
        sellPanelManager.gameObject.SetActive(false);
        panelAnim.SetTrigger("Down");
    }
    #endregion

    // ⭐ 추가: 스쿼드 슬롯 (리드 + 동료 4마리) 관련 로직
    #region 스쿼드 슬롯

    /// <summary>
    /// LaunchSlotAction(Up 타입 - 리드/동료 슬롯)을 탭했을 때 호출됨.
    /// slotIndex: 0=리드, 1~4=동료 1~4번
    /// </summary>
    public void OpenPickerForSlot(int slotIndex, CardData currentCardInSlot)
    {
        editingSlotIndex = slotIndex;
        Debug.Log($"[LaunchManager] OpenPickerForSlot 호출됨. slotIndex={slotIndex}");
        SetAllFieldTypeOf("Weapon", currentCardInSlot);
    }

    /// <summary>
    /// LaunchSlotAction(Field 타입 - 필드 안의 카드)에서 카드를 선택했을 때 호출됨.
    /// editingSlotIndex에 따라 리드 또는 동료 슬롯에 배정한다.
    /// </summary>
    public void AssignPickedCard(CardData cardData)
    {
        Debug.Log($"[LaunchManager] AssignPickedCard 호출됨. editingSlotIndex={editingSlotIndex}, cardData={(cardData == null ? "NULL" : cardData.Name)}");

        if (editingSlotIndex == 0)
        {
            UpdateLead(cardData);
        }
        else if (editingSlotIndex >= 1 && editingSlotIndex <= companionSlots.Length)
        {
            AssignCompanion(editingSlotIndex - 1, cardData);
        }
        else
        {
            Logger.LogWarning($"[LaunchManager] 잘못된 editingSlotIndex: {editingSlotIndex}");
        }

        editingSlotIndex = -1;
    }

    void AssignCompanion(int companionIndex, CardData cardData)
    {
        // ⭐ 추가: 리드와 동일한 방식으로 StartingMember 플래그를 갱신해서 저장/복원되게 함
        CardData previousCard = companionCardData[companionIndex];

        cardDataManager.BeginBatchOperation();
        if (previousCard != null)
        {
            cardDataManager.UpdateStartingmemberOfCard(previousCard, "N");
        }
        cardDataManager.UpdateStartingmemberOfCard(cardData, CompanionStartingMembers[companionIndex].ToString());
        cardDataManager.EndBatchOperation();
        cardDataManager.RefreshCardList();

        companionCardData[companionIndex] = cardData;
        setCardDataOnSlot.PutCardDataIntoSlot(cardData, companionSlots[companionIndex]);

        // ⭐ 추가: 카드가 배정됐으니 "비우기" 버튼 표시
        SetCompanionClearButtonActive(companionIndex, true);

        RefreshCompanionsInContainer();
        StartCoroutine(AssignCompanionCo());
    }

    IEnumerator AssignCompanionCo()
    {
        yield return new WaitForSeconds(.2f);
        CloseField();
        BgToExitField.SetActive(false);
        backButton.SetActive(false);
    }

    /// <summary>
    /// 동료 슬롯을 비움 (선택 취소용). 필요 시 슬롯의 "제거" 버튼 등에 연결해서 사용.
    /// </summary>
    public void ClearCompanionSlot(int companionIndex)
    {
        if (companionIndex < 0 || companionIndex >= companionCardData.Length) return;

        // ⭐ 추가: 슬롯을 비울 때 카드에 남아있는 StartingMember 플래그도 함께 해제
        CardData cardToRemove = companionCardData[companionIndex];
        if (cardToRemove != null)
        {
            cardDataManager.UpdateStartingmemberOfCard(cardToRemove, "N");
            cardDataManager.RefreshCardList();
        }

        companionCardData[companionIndex] = null;
        SetCompanionSlotEmptyButClickable(companionIndex);
        RefreshCompanionsInContainer();
    }

    /// <summary>
    /// SellPanelManager가 오리 카드를 판매 처리한 직후 호출.
    /// 판매된 카드가 동료 슬롯에 있었다면 즉시 비운다.
    /// (StartingMember 플래그는 카드 자체가 삭제되면서 함께 사라지므로 별도로 해제할 필요 없음)
    /// </summary>
    public void HandleCardSold(CardData soldCard)
    {
        if (soldCard == null) return;

        bool changed = false;
        for (int i = 0; i < companionCardData.Length; i++)
        {
            if (companionCardData[i] == null) continue;
            if (companionCardData[i].ID != soldCard.ID) continue;

            companionCardData[i] = null;
            SetCompanionSlotEmptyButClickable(i);
            changed = true;
        }

        if (changed) RefreshCompanionsInContainer();
    }

    void RefreshCompanionsInContainer()
    {
        List<CardData> validCompanions = new List<CardData>();
        for (int i = 0; i < companionCardData.Length; i++)
        {
            if (companionCardData[i] != null) validCompanions.Add(companionCardData[i]);
        }
        startingDataContainer.SetCompanions(validCompanions);
    }

    /// <summary>
    /// 이미 스쿼드(리드+동료)에 포함된 종(weaponData.Name)을 후보 목록에서 제외한다.
    /// </summary>
    List<CardData> FilterOutSquadDuplicates(List<CardData> cards)
    {
        if (startingDataContainer == null || cardsDictionary == null) return cards;

        HashSet<string> squadNames = startingDataContainer.GetSquadWeaponNames();
        if (squadNames == null || squadNames.Count == 0) return cards;

        List<CardData> filtered = new List<CardData>();
        for (int i = 0; i < cards.Count; i++)
        {
            WeaponData wd = cardsDictionary.GetWeaponItemData(cards[i]).weaponData;
            if (wd != null && squadNames.Contains(wd.Name)) continue; // 이미 스쿼드에 있는 종은 숨김
            filtered.Add(cards[i]);
        }
        return filtered;
    }

    #endregion
}