using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

// 대장 오리는 playerPref에 저장하자
public class LaunchManager : MonoBehaviour
{
    [SerializeField] CardSlot leadOriSlot;
    [SerializeField] CardDataManager cardDataManager;
    [SerializeField] SetCardDataOnSlot setCardDataOnSlot;
    [SerializeField] StatManager statManager;
    [SerializeField] StartingDataContainer startingDataContainer;
    CardSlotManager cardSlotManager;

    [SerializeField] GameObject fieldSlotPanel; // 패널 켜고 끄기 위해
    [SerializeField] AllField field; // 모든 카드

    [SerializeField] GameObject startButton;
    [SerializeField] GameObject BgToExitField; // 벼경을 터치하면 리드오리 선택 필드를 나가도록

    [SerializeField] StageInfoUI stageInfoUi;
    [SerializeField] StageInfo stageInfo;

    CardData currentLead; // 현재 리드로 선택된 오리
    OriAttribute currentAttr; // 현재 리드로 선택된 오리의 attr

    bool isInitialized; // 한 번 초기화 된 후에는 코루틴으로 리드를 초기화 할 필요가 없으므로 

    // 코루틴으로 리드를 초기화 할 때 얼마만큼 딜레이 할 것인지. (0.3으로 설정되어 있었는데 이렇게 설정한 이유를 모르겠음. 딜레이가 있으면 장비 탭에서 리드 오리의 장비를 교체한 후 론치 패널로 돌아왔을 때 0.3초 후에 장비 그림이 업데이트가 되어서 0으로 바꿔 놓았음)
    float initDelayTime = 0f; 
    
    void OnEnable()
    {
        // fieldSlotPanel.SetActive(false);
        stageInfoUi.PlayFromStart();
        InitLead();

        if (cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager.SettrigerAnim("Off");
    }
    void OnDisable()
    {
        BgToExitField.SetActive(false); // 론치 패널 계층 바깥에 있으므로 따로 비활성화
    }

    // 버튼으로 스테이지 앞 뒤로 갈 수 있도록

    public void UpdateStageInfo()
    {
        InitStageInfo();
    }

    void InitStageInfo()
    {
        int stageNum = FindObjectOfType<PlayerDataManager>().GetCurrentStageNumber();
        Stages currentStage = stageInfo.GetStageInfo(stageNum);
        stageInfoUi.Init(currentStage);
    }

    void InitLead()
    {
        if (isInitialized) initDelayTime = 0;
        isInitialized = true;
        StartCoroutine(InitCo());
    }
    IEnumerator InitCo()
    {
        startButton.SetActive(false);
        yield return new WaitForSeconds(initDelayTime);
        CardData lead = cardDataManager.GetMyCardList().Find(x => x.StartingMember == StartingMember.Zero.ToString());
        SetLead(lead);
        yield return new WaitForSeconds(.03f);
        startButton.SetActive(true); // 리드 카드가 셋업되기 전에 시작 버튼을 누르면 리드가 스프라이트가 없는 채로 시작된다.
        InitStageInfo();
    }

    public void SetAllFieldTypeOf(string oriType, CardData currentLeadOri)
    {
        // fieldSlotPanel.SetActive(true);
        List<CardData> card = new();

        // field 오리만 보여줌
        card = cardDataManager.GetMyCardList().FindAll(x => x.Type == oriType);

        if (cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager.SettrigerAnim("Launch");

        // 지금 리드 오리로 선택되어 있는 오리는 제외하기
        // card.Remove(currentLeadOri);

        field.GenerateAllCardsOfType(card, "Launch");
        // SetHalo(true);

        BgToExitField.SetActive(true);
    }
    void SetLead(CardData lead)
    {
        currentLead = lead;

        // 리드오리 attr update
        currentAttr = statManager.GetLeadAttribute(currentLead);
        
        setCardDataOnSlot.PutCardDataIntoSlot(lead, leadOriSlot);
        
        startingDataContainer.SetLead(lead, currentAttr);
    }

    public void UpdateLead(CardData newLead)
    {
        Debug.Log("Update Lead");
        cardDataManager.UpdateStartingmemberOfCard(currentLead, "N");
        cardDataManager.UpdateStartingmemberOfCard(newLead, "Zero");

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
    // Update Lead 외에도 BG to Exit 버튼을 누르면 호출
    public void CloseField()
    {
        // DOTween.KillAll(true);

        // fieldSlotPanel.SetActive(false);

        cardSlotManager.SettrigerAnim("Off");
    }
}
