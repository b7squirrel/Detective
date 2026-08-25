using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    // ⭐ 변경: 동료 슬롯을 3스테이지마다 하나씩 활성화 (크리스탈 구매 방식은 제거)
    // 하이어라키에서 슬롯 하나(카드+비우기 버튼 포함)를 통째로 감싼 wrapper 오브젝트를 순서대로 연결
    [Header("동료 슬롯 스테이지 해금")]
    [SerializeField] GameObject[] companionSlotWrappers = new GameObject[4]; // companionSlots와 1:1 대응하는 상위 wrapper
    [SerializeField] int[] companionSlotUnlockStages = { 3, 6, 9, 12 }; // 이 스테이지를 클리어하면 해당 슬롯이 나타남 (companionIndex 순서)
    [SerializeField] GameObject companionSlotUnlockAnnouncement; // 첫 번째 동료 슬롯이 해금됐을 때 딱 한 번 보여주는 안내 오버레이
    [SerializeField] GameObject[] companionSlotBadges = new GameObject[4]; // 슬롯별 "새로 해금됨" 빨간 점(Badge Image). companionSlotWrappers와 1:1 대응

    // ⭐ 추가: 동료 슬롯 첫 해금 안내 팝업이 뜰 때, Companion Slot 1의 Empty Slot 1을 하이라이트하기 위한 참조
    [Header("동료 슬롯 첫 해금 안내 - 하이라이트")]
    [SerializeField] TutorialHighlight tutorialHighlight;
    [SerializeField] RectTransform companionSlot1EmptySlotRect; // Companion Slot 1 > Empty Slot 1의 RectTransform
    [SerializeField] GameObject companionSlotHighlightFg; // 하이라이트 중 클릭 차단용 오버레이 (없으면 비워둬도 됨)

    // ⭐ 추가: 레이아웃 그룹이 걸려있는 상위 오브젝트(예: "Companion Slots"). SetActive 직후 이 레이아웃이
    //          아직 재배치되지 않은 상태일 수 있어, 하이라이트 좌표 계산 전에 이 대상을 강제로 리빌드한다.
    [SerializeField] RectTransform companionSlotsLayoutRoot; // Companions > Companion Slots

    // ⭐ 추가: 선택 가능한 카드가 없을 때 보여주는 경고 팝업
    [Header("선택 가능한 카드 없음 경고")]
    [SerializeField] GameObject lackOfCardWarning; // "Lack of Card Warning" 오브젝트 (PanelTween 보유)

    void Awake()
    {
        // ⭐ 추가: 시작 시 동료 슬롯을 모두 빈 상태로 초기화
        for (int i = 0; i < companionSlots.Length; i++)
        {
            SetCompanionSlotEmptyButClickable(i);
        }

        // ⭐ 변경: 클리어한 스테이지 수에 맞춰 동료 슬롯을 순차적으로 활성화
        // ⭐ 수정: Awake() 시점은 캔버스/레이아웃이 아직 배치되기 전이라 하이라이트 좌표가 깨지므로,
        //          여기서는 wrapper 활성화만 하고 안내 팝업/하이라이트는 절대 띄우지 않는다.
        //          (안내는 레이아웃이 안정된 InitLead()의 RefreshCompanionSlotStageUnlocks() 호출에서만 처리)
        RefreshCompanionSlotStageUnlocks(allowShowAnnouncement: false);
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

        // ⭐ 변경: 패널이 열릴 때마다 클리어 스테이지 수를 다시 확인해서 동료 슬롯을 갱신 (새로 클리어했으면 바로 반영됨)
        RefreshCompanionSlotStageUnlocks();
        
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

        // ⭐ 추가: 고를 수 있는 카드가 없으면 안내 팝업 표시, 있으면(혹시 이전에 떠 있었다면) 닫음
        UpdateLackOfCardWarning(card.Count == 0);

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

        // ⭐ 추가: 필드를 닫을 때 "선택 가능한 카드 없음" 경고도 필드와 함께 바로(트윈 없이) 닫는다.
        if (lackOfCardWarning != null && lackOfCardWarning.activeSelf)
            lackOfCardWarning.SetActive(false);
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
        // ⭐ 추가: 첫 번째 동료 슬롯 해금 안내(팝업+하이라이트)가 떠 있는 상태에서
        //          바로 그 슬롯(슬롯 1번)을 탭했다면, 안내를 닫아준다.
        if (slotIndex == 1)
        {
            HideCompanionSlotUnlockAnnouncementIfShowing();
        }

        // ⭐ 추가: 동료 슬롯을 탭해서 열면, 새로 해금됐다는 배지를 확인한 것으로 처리해서 지움
        if (slotIndex >= 1 && slotIndex <= companionSlotBadges.Length)
        {
            int companionIndex = slotIndex - 1;
            if (playerDataManager == null) playerDataManager = PlayerDataManager.Instance;

            if (playerDataManager != null && !playerDataManager.IsCompanionSlotBadgeSeen(companionIndex))
            {
                playerDataManager.SetCompanionSlotBadgeSeen(companionIndex);

                if (companionSlotBadges[companionIndex] != null)
                    companionSlotBadges[companionIndex].SetActive(false);
            }
        }

        editingSlotIndex = slotIndex;
        Debug.Log($"[LaunchManager] OpenPickerForSlot 호출됨. slotIndex={slotIndex}");
        SetAllFieldTypeOf("Weapon", currentCardInSlot);
    }

    // ⭐ 추가: 첫 번째 동료 슬롯 해금 안내 팝업이 현재 떠 있으면 PanelTween.HideWithScale()로 닫고,
    //          하이라이트 오버레이도 함께 숨긴다. (팝업이 안 떠 있으면 아무 것도 하지 않음 — 평소 슬롯 탭에는 영향 없음)
    void HideCompanionSlotUnlockAnnouncementIfShowing()
    {
        if (companionSlotUnlockAnnouncement != null && companionSlotUnlockAnnouncement.activeSelf)
        {
            PanelTween tween = companionSlotUnlockAnnouncement.GetComponent<PanelTween>();
            if (tween != null)
                tween.HideWithScale();
            else
                companionSlotUnlockAnnouncement.SetActive(false); // 폴백
        }

        if (tutorialHighlight != null)
            tutorialHighlight.Hide();
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

    // ⭐ 변경: 동료 슬롯 스테이지 해금 (크리스탈 구매 방식에서 변경)
    #region 동료 슬롯 스테이지 해금

    /// <summary>
    /// 클리어한 최고 스테이지(GetCurrentStageNumber() - 1)를 기준으로,
    /// 각 동료 슬롯 wrapper 오브젝트를 통째로 활성화/비활성화한다.
    /// 별도 저장값 없이 항상 현재 진행도로 계산하므로 상태가 어긋날 일이 없다.
    /// </summary>
    void RefreshCompanionSlotStageUnlocks(bool allowShowAnnouncement = true)
    {
        if (playerDataManager == null) playerDataManager = PlayerDataManager.Instance;
        int clearedStage = playerDataManager != null ? playerDataManager.GetCurrentStageNumber() - 1 : 0;

        for (int i = 0; i < companionSlotWrappers.Length; i++)
        {
            bool unlocked = i < companionSlotUnlockStages.Length && clearedStage >= companionSlotUnlockStages[i];

            if (companionSlotWrappers[i] != null)
                companionSlotWrappers[i].SetActive(unlocked);

            // ⭐ 추가: 첫 번째 동료 슬롯이 해금된 상태라면, 아직 안 보여줬을 때 딱 한 번 안내 오버레이를 띄움
            // ⭐ 수정: allowShowAnnouncement가 false면(예: Awake() 호출) 절대 안내를 띄우지 않음 —
            //          레이아웃이 준비되지 않은 시점에 띄우면 하이라이트 좌표가 깨지고, "봤음" 처리가 먼저 되어버려
            //          이후 제대로 된 타이밍의 호출이 아예 무시되기 때문
            if (i == 0 && unlocked && allowShowAnnouncement)
            {
                ShowFirstCompanionSlotAnnouncementIfNeeded();
            }

            // ⭐ 추가: 해금됐지만 아직 확인 안 한 슬롯엔 빨간 점 배지 표시
            if (companionSlotBadges != null && i < companionSlotBadges.Length && companionSlotBadges[i] != null)
            {
                bool showBadge = unlocked && playerDataManager != null && !playerDataManager.IsCompanionSlotBadgeSeen(i);
                companionSlotBadges[i].SetActive(showBadge);
            }
        }
    }

    /// <summary>
    /// 첫 번째 동료 슬롯 해금 안내를 아직 한 번도 보여준 적 없다면 표시하고, 영구적으로 "봤음" 처리한다.
    /// </summary>
    void ShowFirstCompanionSlotAnnouncementIfNeeded()
    {
        if (playerDataManager == null) playerDataManager = PlayerDataManager.Instance;
        if (playerDataManager == null) return;
        if (playerDataManager.HasShownFirstCompanionSlotAnnouncement()) return;

        ShowCompanionSlotUnlockAnnouncement();

        playerDataManager.SetFirstCompanionSlotAnnouncementShown(true);
    }

    // ⭐ 수정: SetActive 직접 호출 대신 PanelTween.ShowWithScale()로 등장시키고,
    //          동시에 Companion Slot 1 > Empty Slot 1을 하이라이트한다.
    //          닫힐 때는 팝업 자체의 "Button Close"에 연결된 PanelTween.HidePanel()이 처리한다.
    void ShowCompanionSlotUnlockAnnouncement()
    {
        if (companionSlotUnlockAnnouncement == null) return;

        PanelTween tween = companionSlotUnlockAnnouncement.GetComponent<PanelTween>();
        if (tween != null)
            tween.ShowWithScale();
        else
            companionSlotUnlockAnnouncement.SetActive(true); // PanelTween이 없을 때를 대비한 폴백

        if (tutorialHighlight == null)
        {
            Debug.LogWarning("[LaunchManager] tutorialHighlight가 인스펙터에 연결되어 있지 않습니다! 하이라이트를 건너뜁니다.");
        }
        else if (companionSlot1EmptySlotRect == null)
        {
            Debug.LogWarning("[LaunchManager] companionSlot1EmptySlotRect가 인스펙터에 연결되어 있지 않습니다! 하이라이트를 건너뜁니다.");
        }
        else
        {
            // ✅ 추가: 방금 SetActive(true)된 Companion Slot Wrapper 때문에 "Companion Slots"의
            //          레이아웃 그룹이 아직 재배치되지 않았을 수 있으므로, 좌표 계산 전에 강제로 즉시 갱신
            Canvas.ForceUpdateCanvases();
            if (companionSlotsLayoutRoot != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(companionSlotsLayoutRoot);
            else
                Debug.LogWarning("[LaunchManager] companionSlotsLayoutRoot가 비어있습니다. 레이아웃이 아직 재배치되지 않았을 수 있습니다.");

            // ⭐ 디버그: 실제로 호출되는지, Empty Slot 1이 활성 상태인지, 좌표가 정상인지 확인
            Debug.Log($"[LaunchManager] 하이라이트 시도. target={companionSlot1EmptySlotRect.name}, " +
                      $"activeInHierarchy={companionSlot1EmptySlotRect.gameObject.activeInHierarchy}, " +
                      $"position={companionSlot1EmptySlotRect.position}, " +
                      $"sizeDelta={companionSlot1EmptySlotRect.sizeDelta}");

            tutorialHighlight.HighlightUI(companionSlot1EmptySlotRect, companionSlotHighlightFg);

            Debug.Log($"[LaunchManager] HighlightUI 호출 완료. tutorialHighlight.gameObject.activeSelf={tutorialHighlight.gameObject.activeSelf}");
        }
    }

    #endregion

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
    /// 단, 지금 편집 중인 슬롯(editingSlotIndex) 자신이 들고 있는 종은 제외하지 않는다 — 
    /// 자기 슬롯을 다시 눌렀을 때 지금 꽂혀있는 카드와 같은 종의 다른 카드(등급 등)도 보여야 하므로.
    /// </summary>
    List<CardData> FilterOutSquadDuplicates(List<CardData> cards)
    {
        if (cardsDictionary == null) return cards;

        HashSet<string> squadNames = GetSquadWeaponNamesExcludingSlot(editingSlotIndex);
        if (squadNames == null || squadNames.Count == 0) return cards;

        List<CardData> filtered = new List<CardData>();
        for (int i = 0; i < cards.Count; i++)
        {
            WeaponData wd = cardsDictionary.GetWeaponItemData(cards[i]).weaponData;
            if (wd != null && squadNames.Contains(wd.Name)) continue; // 다른 슬롯이 이미 가진 종은 숨김
            filtered.Add(cards[i]);
        }
        return filtered;
    }

    /// <summary>
    /// slotIndex(0=리드, 1~4=동료)로 지정된 슬롯 자신은 빼고, 나머지 스쿼드 슬롯들이 가진 종(weaponData.Name)을 모은다.
    /// StartingDataContainer의 companions 리스트는 빈 슬롯을 건너뛰고 압축되어 있어 슬롯 번호와 인덱스가 안 맞을 수 있으므로,
    /// 여기서는 LaunchManager가 직접 들고 있는 companionCardData(빈 슬롯도 자리를 유지함)와 currentLead를 기준으로 계산한다.
    /// </summary>
    HashSet<string> GetSquadWeaponNamesExcludingSlot(int slotIndex)
    {
        HashSet<string> names = new HashSet<string>();

        if (slotIndex != 0 && currentLead != null)
        {
            WeaponData leadWd = cardsDictionary.GetWeaponItemData(currentLead).weaponData;
            if (leadWd != null) names.Add(leadWd.Name);
        }

        for (int i = 0; i < companionCardData.Length; i++)
        {
            int thisSlotIndex = i + 1; // companionCardData[0] = 슬롯 1번, ... companionCardData[3] = 슬롯 4번
            if (thisSlotIndex == slotIndex) continue; // 자기 자신은 제외 대상에서 뺌
            if (companionCardData[i] == null) continue;

            WeaponData wd = cardsDictionary.GetWeaponItemData(companionCardData[i]).weaponData;
            if (wd != null) names.Add(wd.Name);
        }

        return names;
    }

    /// <summary>
    /// show가 true면 PanelTween.ShowWithScale()로 등장, false면 (떠 있을 때만) 즉시 숨김.
    /// 완전히 닫는 처리는 CloseField()에서 필드와 함께 즉시 처리한다.
    /// </summary>
    void UpdateLackOfCardWarning(bool show)
    {
        if (lackOfCardWarning == null) return;

        if (show)
        {
            PanelTween tween = lackOfCardWarning.GetComponent<PanelTween>();
            if (tween != null)
                tween.ShowWithScale();
            else
                lackOfCardWarning.SetActive(true); // 폴백
        }
        else
        {
            if (lackOfCardWarning.activeSelf)
                lackOfCardWarning.SetActive(false);
        }
    }

    #endregion
}