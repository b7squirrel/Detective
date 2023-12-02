using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

// 대장 오리는 playerPref에 저장하자
public class LaunchManager : MonoBehaviour
{
    [SerializeField] CardSlot leadOriSlot;
    [SerializeField] CardDataManager cardDataManager;
    [SerializeField] SetCardDataOnSlot setCardDataOnSlot;
    [SerializeField] StatManager statManager;
    [SerializeField] CardList cardList;
    [SerializeField] StartingDataContainer startingDataContainer;

    [SerializeField] GameObject fieldSlotPanel; // 패널 켜고 끄기 위해
    [SerializeField] AllField field; // 모든 카드

    [SerializeField] GameObject startButton;

    [SerializeField] StageInfoUI stageInfoUi;
    [SerializeField] StageInfo stageInfo;

    CardData currentLead; // 현재 리드로 선택된 오리
    OriAttribute currentAttr; // 현재 리드로 선택된 오리의 attr

    void OnEnable()
    {
        fieldSlotPanel.SetActive(false);
        InitLead();
    }
    void OnDisable()
    {
        CloseField();
    }

    void InitStageInfo()
    {
        int stageNum = FindObjectOfType<StageManager>().GetCurrentStageNumber();
        Stages currentStage = stageInfo.GetStageInfo(stageNum);
        stageInfoUi.Init(currentStage);
    }

    void InitLead()
    {
        StartCoroutine(InitCo());
    }
    IEnumerator InitCo()
    {
        startButton.SetActive(false);
        yield return new WaitForSeconds(.1f);
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

        // 지금 리드 오리로 선택되어 있는 오리는 제외하기
        card.Remove(currentLeadOri);

        field.GenerateAllCardsOfType(card);
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
        cardDataManager.UpdateStartingmemberOfCard(currentLead, "N");
        cardDataManager.UpdateStartingmemberOfCard(newLead, "Zero");
        
        CloseField();
        SetLead(newLead);
    }
    public void CloseField()
    {
        ClearAllFieldSlots();
        fieldSlotPanel.SetActive(false);
    }
}
