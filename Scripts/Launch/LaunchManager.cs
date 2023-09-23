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
    string firstOriID;

    [SerializeField] GameObject fieldSlotPanel; // 패널 켜고 끄기 위해
    [SerializeField] AllField field; // 모든 카드

    CardData currentLead; // 현재 리드로 선택된 오리

    void OnEnable()
    {
        fieldSlotPanel.SetActive(false);
        LoadLeadOri();
    }

    public void LoadLeadOri()
    {
        StartCoroutine(loadCo());
    }
    IEnumerator loadCo()
    {
        yield return new WaitForSeconds(.01f);
        CardData lead = cardDataManager.GetMyCardList().Find(x => x.startingMember == "1");
        currentLead = lead;
        
        setCardDataOnSlot.PutCardDataIntoSlot(lead, leadOriSlot);
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

    public void UpdateLead(CardData newLead)
    {
        cardDataManager.UpdateStartingmemberOfCard(newLead, "1");
        cardDataManager.UpdateStartingmemberOfCard(currentLead, "N");
        currentLead = newLead;
        CloseField();
        LoadLeadOri();
    }
    public void CloseField()
    {
        ClearAllFieldSlots();
        fieldSlotPanel.SetActive(false);
    }
}
