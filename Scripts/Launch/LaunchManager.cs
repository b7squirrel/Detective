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

    [SerializeField] GameObject fieldSlotPanel; // 패널 켜고 끄기 위해
    [SerializeField] AllField field; // 모든 카드

    [SerializeField] GameObject startButton;

    [SerializeField] StageInfoUI stageInfoUi;
    [SerializeField] StageInfo stageInfo;

    [SerializeField] CanvasGroup BlackFadeIn; // 처음 시작할 때 화면이 준비될 동안 암전
    [SerializeField] float fadeSpeed;
    bool onStart = true; // 처음 시작할 때만 검은색에서 페이드인 되도록
    bool shouldFadeIn; // 페이드인이 완료되거나 중간에 중단해야 할 때, 페이드인코루틴을 빠져나오기 위한 플래그.
    Coroutine fadeInCoroutine;

    CardData currentLead; // 현재 리드로 선택된 오리
    OriAttribute currentAttr; // 현재 리드로 선택된 오리의 attr

    void OnEnable()
    {
        if(onStart)
        {
            BlackFadeIn.gameObject.SetActive(true);
            BlackFadeIn.alpha = 1f;
            //BlackFadeIn.DOFade(0, 4f).SetId(BlackFadeIn.gameObject.name);
            fadeInCoroutine = StartCoroutine(FadeInCo());
            onStart = false;
        }

        fieldSlotPanel.SetActive(false);
        InitLead();
    }
    void OnDisable()
    {
        StopFadeIn(fadeInCoroutine);
        CloseField();
    }

    IEnumerator FadeInCo()
    {
        shouldFadeIn = true;
        while(shouldFadeIn)
        {
            if (BlackFadeIn.alpha <= .01)
            {
                BlackFadeIn.alpha = 0f;
                shouldFadeIn = false;
            }
            BlackFadeIn.alpha = Mathf.Lerp(BlackFadeIn.alpha, 0, fadeSpeed * Time.deltaTime);
            yield return null;
        }
    }
    void StopFadeIn(Coroutine co)
    {
        BlackFadeIn.alpha = 0;
        StopCoroutine(co);
    }
    void InitStageInfo()
    {
        int stageNum = FindObjectOfType<PlayerDataManager>().GetCurrentStageNumber();
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
        yield return new WaitForSeconds(.03f);
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
        DOTween.KillAll(true);

        fieldSlotPanel.SetActive(false);
    }
}
