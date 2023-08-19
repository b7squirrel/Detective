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
        if (this.matCards == null) matCards = new List<Card>();

        this.matCards.Clear();

        this.matCards.AddRange(_matCards);

        UpdateSlots();
    }

    public List<Card> GetMMatCards()
    {
        return matCards;
    }
    #endregion

    #region Refresh
    public void UpdateSlots()
    {
        List<Card> cards = new();
        List<GameObject> slots = new();

        cards.AddRange(matCards); // 재료가 될 수 있는 카드 리스트 

        numSlots = cards.Count;

        // 재료 카드 갯수만큼 슬롯 생성
        for (int i = 0; i < numSlots; i++)
        {
            var slot = Instantiate(slotPrefab, transform);
            slot.transform.position = Vector3.zero;
            slot.transform.localScale = Vector3.one;
            slots.Add(slot);
        }

        // 재료 카드 생성. 슬롯위에 배치
        for (int i = 0; i < cards.Count; i++)
        {
            string _type = cards[i].GetCardType().ToString();
            string _grade = cards[i].GetCardGrade().ToString();
            string _name = cards[i].GetCardName();

            GameObject newCard =
                cardDictionary.GenCard(_type, _grade, _name);

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
