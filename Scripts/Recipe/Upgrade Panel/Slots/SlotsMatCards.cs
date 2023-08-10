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
    List<CardData> matCards;
    #endregion

    #region 유니티 콜백 함수
    void Awake()
    {
        cardDataManager = FindObjectOfType<CardDataManager>();
        cardDictionary = FindObjectOfType<CardsDictionary>();
    }
    #endregion

    private void OnEnable() {
        ClearmatCardsSlots();
    }
    private void OnDisable() {
        ClearmatCardsSlots();
    }
    public void SetMatCards(List<CardData> _matCards)
    {
        if (this.matCards == null) matCards = new List<CardData>();

        foreach (var item in _matCards)
        {
            Debug.Log(item.Name);
        }
        
        this.matCards.AddRange(_matCards);
    }

    // public void InitMatPanel()
    // {
    //     List<Card> myCards = new List<Card>();
    //     myCards.AddRange(slotsAllCards.GetCarsOnSlots()); // 슬롯 위의 카드들

    //     Card cardOnUpSlot = slotUpCard.GetCardToUpgrade(); // 업그레이드 슬롯 위의 카드
    //     string mGrade = cardOnUpSlot.GetCardGrade().ToString();
    //     string mName = cardOnUpSlot.GetCardName();

    //     // 슬롯 위의 카드들 중 업그레이드 슬롯 카드와 같은 이름, 같은 등급을 가진 카드를 추려냄
    //     List<CardData> picked = new List<CardData>();
    //     picked.AddRange(new CardClassifier().GetAvailableCardsForMat(cardDataManager.GetMyCardList(), cardOnUpSlot));
    // }

    #region Refresh
    public void UpdateSlots()
    {
        List<CardData> cardDatas = new List<CardData>();
        List<GameObject> slots = new List<GameObject>();

        cardDatas.AddRange(matCards); // 재료가 될 수 있는 카드 리스트 

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
