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
    [SerializeField] CardList cardList;
    [SerializeField] StartingDataContainer startingDataContainer;
    CardSlotManager cardSlotManager;

    [SerializeField] GameObject fieldSlotPanel; // 패널 켜고 끄기 위해
    [SerializeField] AllField field; // 모든 카드

    [SerializeField] GameObject startButton;

    [SerializeField] StageInfoUI stageInfoUi;
    [SerializeField] StageInfo stageInfo;

    CardData currentLead; // 현재 리드로 선택된 오리
    OriAttribute currentAttr; // 현재 리드로 선택된 오리의 attr

    bool isInitialized; // 한 번 초기화 된 후에는 코루틴으로 리드를 초기화 할 필요가 없으므로 
    float initDelayTime = .3f; // 코루틴으로 리드를 초기화 할 때 얼마만큼 딜레이 할 것인지.
    
    void OnEnable()
    {
        fieldSlotPanel.SetActive(false);
        stageInfoUi.PlayFromStart();
        InitLead();
        CloseField();

        if (cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager.SettrigerAnim("Off");
        Debug.Log("출동탭에서 Off를 트리거 했습니다.");
    }
    void OnDisable()
    {
        CloseField();
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
    public void ClearAllFieldSlots()
    {
        field.ClearSlots();
    }

    public void SetAllFieldTypeOf(string oriType, CardData currentLeadOri)
    {
        fieldSlotPanel.SetActive(true);
        List<CardData> card = new();

        // field 오리만 보여줌
        card = cardDataManager.GetMyCardList().FindAll(x => x.Type == oriType);

        if (cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager.SettrigerAnim("Launch");
        Debug.Log($"출동탭에서 Launch가 트리거 되었습니다.");

        // 지금 리드 오리로 선택되어 있는 오리는 제외하기
        //card.Remove(currentLeadOri);

        field.GenerateAllCardsOfType(card);
        SetHalo(true);
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
        
        CloseField();
        SetLead(newLead);
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
    public void CloseField()
    {
        ClearAllFieldSlots();
        DOTween.KillAll(true);

        fieldSlotPanel.SetActive(false);

        
    }

    public CardData GetLeadCardData()
    {
        return currentLead;
    }
}
