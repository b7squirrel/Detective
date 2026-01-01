using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchManager : MonoBehaviour
{
    [SerializeField] CardSlot leadOriSlot;
    [SerializeField] CardDataManager cardDataManager;
    [SerializeField] SetCardDataOnSlot setCardDataOnSlot;
    [SerializeField] StatManager statManager;
    [SerializeField] StartingDataContainer startingDataContainer;
    CardSlotManager cardSlotManager;

    [SerializeField] GameObject fieldSlotPanel;
    [SerializeField] AllField field;

    [SerializeField] GameObject startButton;
    [SerializeField] GameObject BgToExitField;

    [SerializeField] StageInfoUI stageInfoUi;
    [SerializeField] StageInfo stageInfo;

    CardData currentLead;
    OriAttribute currentAttr;

    void OnEnable()
    {
        // stageInfoUi.PlayFromStart();
        
        // ⭐ 초기화 대기
        StartCoroutine(InitLead());

        if (cardSlotManager == null) 
            cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager.SettrigerAnim("Off");
    }
    
    void OnDisable()
    {
        BgToExitField.SetActive(false);
        startButton.SetActive(false);
    }

    public void UpdateStageInfo()
    {
        InitStageInfo();
    }

    void InitStageInfo()
    {
        int stageNum = FindObjectOfType<PlayerDataManager>().GetCurrentStageNumber();
        Stages currentStage = stageInfo.GetStageInfo(stageNum);
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
        
        // UI 업데이트 대기
        yield return new WaitForSeconds(.03f);
        
        startButton.SetActive(true);
        InitStageInfo();
        
        Logger.Log("[LaunchManager] 리드 초기화 완료");
    }
    
    void SetLead(CardData lead)
    {
        currentLead = lead;

        // 리드오리 attr update
        currentAttr = statManager.GetLeadAttribute(currentLead);

        setCardDataOnSlot.PutCardDataIntoSlot(lead, leadOriSlot);

        startingDataContainer.SetLead(lead, currentAttr);
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
    }

    public void SetAllFieldTypeOf(string oriType, CardData currentLeadOri)
    {
        List<CardData> card = new();

        card = cardDataManager.GetMyCardList().FindAll(x => x.Type == oriType);

        if (cardSlotManager == null) 
            cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager.SettrigerAnim("Launch");

        field.GenerateAllCardsOfType(card, "Launch");

        BgToExitField.SetActive(true);
        startButton.SetActive(false);

        cardSlotManager.InitialSortingByGrade();
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
    }
}