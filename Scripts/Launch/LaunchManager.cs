using System;
using System.Collections;
using System.Collections.Generic;
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

    CardData currentLead; // 현재 리드로 선택된 오리
    OriAttribute currentAttr; // 현재 리드로 선택된 오리의 attr

    void OnEnable()
    {
        fieldSlotPanel.SetActive(false);
        InitLead();
    }

    public void InitLead()
    {
        StartCoroutine(InitCo());
    }
    IEnumerator InitCo()
    {
        yield return new WaitForSeconds(.05f);
        CardData lead = cardDataManager.GetMyCardList().Find(x => x.StartingMember == StartingMember.Zero.ToString());
        SetLead(lead);
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
        cardDataManager.UpdateStartingmemberOfCard(newLead, "1");
        
        CloseField();
        SetLead(newLead);
    }
    public void CloseField()
    {
        ClearAllFieldSlots();
        fieldSlotPanel.SetActive(false);
    }
}
