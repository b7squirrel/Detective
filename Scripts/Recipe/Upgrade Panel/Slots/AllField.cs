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

        Debug.Log($"[AllField] GenerateAllCardsOfType 호출됨. tab={tab}, cardList.Count={cardList.Count}, IDs=[{string.Join(",", cardList.ConvertAll(c => c.ID))}]");

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
