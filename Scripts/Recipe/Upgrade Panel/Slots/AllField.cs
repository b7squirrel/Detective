using System.Collections.Generic;
using UnityEngine;

public class AllField : MonoBehaviour
{
    #region 참조 변수
    CardSlotManager cardSlotManager;
    [SerializeField] StartingDataContainer startingDataContainer; // 리드 오리 카드 데이터를 얻어오기 위해
    #endregion

    #region Refresh
    public void GenerateAllCardsOfType(List<CardData> cardList, string tab)
    {
        ClearSlots();

        // List<CardData> cardDatas = new();

        // cardDatas.AddRange(cardList); // 재료가 될 수 있는 카드들의 리스트

        // // 카드 데이터 정렬
        // List<CardData> cardDataSorted = SortByGrade(cardDatas);

        // // 목록에 리드 오리가 있다면 리드 오리를 가장 앞에 배치
        // if (startingDataContainer == null) startingDataContainer = FindObjectOfType<StartingDataContainer>();
        // CardData lead = startingDataContainer.GetPlayerCardData();
        // foreach (var item in cardList)
        // {
        //     if (item.ID == lead.ID)
        //     {
        //         cardDataSorted.Remove(lead);
        //         cardDataSorted.Insert(0, lead);
        //         break;
        //     }
        // }

        foreach (var item in cardList)
        {
            cardSlotManager.SetSlotActive(item.ID, true);
        }
    }
    public List<CardData> SortByGrade(List<CardData> cardDatas)
    {
        // 카드 데이터 정렬
        List<CardData> cardDataSorted = new();
        cardDataSorted.AddRange(cardDatas);

        // 내림차순으로 카드 정렬 
        cardDataSorted.Sort((a, b) =>
        {
            return new Sort().ByGrade(a, b);
        });

        cardDataSorted.Reverse();
        return cardDataSorted;
    }

    public void ClearSlots()
    {
        if (cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager.ClearPresentationField();
    }
    #endregion
}
