using System.Collections.Generic;
using UnityEngine;

public class SlotsMatCards : MonoBehaviour
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
        List<CardData> cardDatas = new List<CardData>();
        List<GameObject> slots = new List<GameObject>();

        cardDatas.AddRange(cardDataManager.GetMyCardList()); // 재료가 될 수 있는 카드 리스트 

        numSlots = cardDatas.Count;

        for (int i = 0; i < numSlots; i++)
        {
            var slot = Instantiate(slotPrefab, transform);
            slot.transform.position = Vector3.zero;
            slot.transform.localScale = Vector3.one;
            slots.Add(slot);
        }

        for (int i = 0; i < cardDatas.Count; i++)
        {
            GameObject newCard =
                cardDictionary.GenCard(cardDatas[i].Type, cardDatas[i].Grade, cardDatas[i].Name);

            newCard.transform.SetParent(slots[i].transform);
            newCard.transform.position = Vector3.zero;
            newCard.transform.localScale = Vector3.one;
        }
    }

    // 패널 최상단 SlotManager의 OnRefresh 유니티 이벤트에 등록되어 있음
    public void ClearmatCardsSlots()
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
}
