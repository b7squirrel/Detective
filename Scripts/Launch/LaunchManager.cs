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
        
        setCardDataOnSlot.PutCardDataIntoSlot(lead, leadOriSlot);
    }
    public void ClearAllFieldSlots()
    {
        field.ClearSlots();
    }

    public void SetAllFieldTypeOf(string oriType, CardData currentLeadOri)
    {
        fieldSlotPanel.SetActive(true);
        ClearAllFieldSlots();
        List<CardData> card = new();

        // field 오리만 보여줌
        card = cardDataManager.GetMyCardList().FindAll(x => x.Type == oriType); 

        // 지금 리드 오리로 선택되어 있는 오리는 제외하기
        card.Remove(currentLeadOri);

        foreach (var item in card)
        {
            Debug.Log(item.Name);
        }
        field.GenerateAllCardsOfType(card);
    }
}
