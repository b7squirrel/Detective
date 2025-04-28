using System.Collections.Generic;
using UnityEngine;

public class AllField : MonoBehaviour
{
    #region 참조 변수
    CardSlotManager cardSlotManager;
    [SerializeField] StartingDataContainer startingDataContainer; // 리드 오리 카드 데이터를 얻어오기 위해
    #endregion

    #region 슬롯 생성 관련 변수
    List<CardData> slotsOnField = new();
    #endregion

    #region Refresh
    public void GenerateAllCardsOfType(List<CardData> cardList, string tab)
    {
        ClearSlots();

        List<CardData> cardDatas = new();
        List<GameObject> slots = new();

        cardDatas.AddRange(cardList); // 재료가 될 수 있는 카드들의 리스트

        // 카드 데이터 정렬
        List<CardData> cardDataSorted = new();
        cardDataSorted.AddRange(cardDatas);

        // 내림차순으로 카드 정렬 
        cardDataSorted.Sort((a, b) =>
        {
            return new Sort().ByGrade(a, b);
        });

        cardDataSorted.Reverse();

        // 목록에 리드 오리가 있다면 리드 오리를 가장 앞에 배치
        if (startingDataContainer == null) startingDataContainer = FindObjectOfType<StartingDataContainer>();
        CardData lead = startingDataContainer.GetPlayerCardData();
        foreach (var item in cardList)
        {
            if (item.ID == lead.ID)
            {
                cardDataSorted.Remove(lead);
                cardDataSorted.Insert(0, lead);
                break;
            }
        }

        if (cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();

        if(cardDataSorted == null) return;
        foreach (var item in cardDataSorted)
        {
            slotsOnField.Add(item);
            cardSlotManager.SetSlotActive(item.ID, true);
        }

        Debug.Log($"{tab}에서 호출되었고, {cardDataSorted.Count}개의 슬롯이 작업되었습니다.");
    }

    public void ClearSlots()
    {
        if (cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        if (slotsOnField.Count > 0)
        {
            foreach (var item in slotsOnField)
            {
                cardSlotManager.SetSlotActive(item.ID, false);
            }
        }
        slotsOnField.Clear();
    }
    #endregion
}
