using System.Collections.Generic;
using UnityEngine;

public class SlotsMatCards : MonoBehaviour
{
    #region 참조 변수
    CardsDictionary cardDictionary;
    #endregion

    #region 슬롯 생성 관련 변수
    int numSlots;
    [SerializeField] GameObject slotPrefab;
    List<Card> matCards;
    List<CardData> matCardsData;
    #endregion

    #region 유니티 콜백 함수
    void Awake()
    {
        cardDictionary = FindObjectOfType<CardsDictionary>();
    }
    #endregion
    
    #region MatCards 관련
    public void SetMatCards(List<Card> _matCards)
    {
        // 재료 Card
        if (matCards == null) matCards = new();
        matCards.Clear();
        matCards.AddRange(_matCards);

        // 재료 CardData
        if(matCardsData == null) matCardsData = new();
        matCardsData.Clear();
        foreach (var item in matCards)
        {
            matCardsData.Add(item.GetCardData());
        }

        UpdateSlots();
    }

    public List<Card> GetMMatCards()
    {
        return matCards;
    }
    public List<CardData> GetCardDatas()
    {
        return matCardsData;
    }
    #endregion

    #region Refresh
    public void UpdateSlots()
    {
        List<CardData> cardDatas = new();
        List<GameObject> slots = new();

        cardDatas.AddRange(matCardsData); // 재료가 될 수 있는 카드 리스트 

        numSlots = cardDatas.Count;

        // 재료 카드 갯수만큼 슬롯 생성
        for (int i = 0; i < numSlots; i++)
        {
            var slot = Instantiate(slotPrefab, transform);
            slot.transform.position = Vector3.zero;
            slot.transform.localScale = Vector3.one;
            slots.Add(slot);
        }

        // 재료 카드 생성. 슬롯위에 배치
        for (int i = 0; i < cardDatas.Count; i++)
        {
            GameObject newCard =
                cardDictionary.GenCard(cardDatas[i]);

            newCard.transform.SetParent(slots[i].transform);
            newCard.transform.position = Vector3.zero;
            newCard.transform.localScale = .6f * Vector3.one;
        }
    }
    
    public void ClearmatCardsSlots()
    {
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            Destroy(child.gameObject);
        }
    }
    #endregion
}
