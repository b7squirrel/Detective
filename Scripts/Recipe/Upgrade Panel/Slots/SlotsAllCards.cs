using System.Collections.Generic;
using UnityEngine;
using System;

public class SlotsAllCards : MonoBehaviour
{
    #region 참조 변수
    CardDataManager cardDataManager;
    CardsDictionary cardDictionary;
    #endregion

    #region 슬롯 생성 관련 변수
    int numSlots;
    [SerializeField] GameObject slotPrefab;
    #endregion

    #region 유니티 콜백 함수
    void Awake()
    {
        cardDataManager = FindObjectOfType<CardDataManager>();
        cardDictionary = FindObjectOfType<CardsDictionary>();
    }
    #endregion

    #region Refresh
    public void UpdateSlots()
    {
        List<CardData> cardDatas = new();
        List<GameObject> slots = new();

        cardDatas.AddRange(cardDataManager.GetMyCardList()); // 재료가 될 수 있는 카드들의 리스트

        numSlots = cardDatas.Count;

        // 슬롯 생성
        for (int i = 0; i < numSlots; i++)
        {
            var slot = Instantiate(slotPrefab, transform);
            slot.transform.position = Vector3.zero;
            slot.transform.localScale = Vector3.one;
            slots.Add(slot);
        }

        // 카드 생성
        List<GameObject> cards = new();

        for (int i = 0; i < numSlots; i++)
        {
            GameObject newCard =
                cardDictionary.GenCard(cardDatas[i].Type, cardDatas[i].Grade, cardDatas[i].Name);

            cards.Add(newCard);
        }

        // 내림차순으로 카드 정렬 
        cards.Sort((a,b) => 
        {
            int indexA = (int)a.GetComponent<Card>().GetCardGrade();
            int indexB = (int)b.GetComponent<Card>().GetCardGrade();
            return indexA.CompareTo(indexB);
        });

        cards.Reverse();

        // 내림차순 순서대로 슬롯에 배치
        for(int i = 0; i < numSlots; i++)
        {
            cards[i].transform.SetParent(slots[i].transform);
            cards[i].transform.position = Vector3.zero;
            cards[i].transform.localScale = .6f * Vector3.one;
        }
    }

    //패널 최상단 SlotManager의 OnRefresh 유니티 이벤트에 등록되어 있음
    public void ClearMyCardsSlots()
    {
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            Destroy(child.gameObject);
        }

        UpdateSlots();
    }
    #endregion

    #region 내가 가진 모든 카드들 얻어오기
    public List<Card> GetCarsOnSlots()
    {
        List<Card> cardsOnSlots = new();

        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            
            Card _card = child.GetComponentInChildren<Card>();

            if(_card == null)
            continue;

            cardsOnSlots.Add(_card);
        }

        return cardsOnSlots;
    }
    #endregion
}
