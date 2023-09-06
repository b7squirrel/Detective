using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MatField : MonoBehaviour
{
    #region 참조 변수
    CardsDictionary cardDictionary;
    CardDataManager cardDataManager;
    #endregion

    #region 슬롯 생성 관련 변수
    int numSlots;
    [SerializeField] GameObject slotPrefab;
    List<CardData> matCardsData;
    #endregion

    #region 유니티 콜백 함수
    void Awake()
    {
        cardDictionary = FindObjectOfType<CardsDictionary>();
        cardDataManager = FindObjectOfType<CardDataManager>();
    }
    
    void Disabled()
    {
        ClearSlots();
    }
    #endregion

    #region MatCards 관련

    public void GenerateMatCardsList(CardData cardDataOnUpSlot)
    {
        if(cardDictionary == null) cardDictionary = FindObjectOfType<CardsDictionary>();
        if(cardDataManager == null) cardDataManager = FindObjectOfType<CardDataManager>();

        // 슬롯 위의 CardData들 (= MyCardsList)
        List<CardData> myCardData = new();
        myCardData.AddRange(cardDataManager.GetMyCardList());
        myCardData.Remove(cardDataOnUpSlot); // 업그레이드 슬롯 위의 카드는 빼기

        // 슬롯 위의 카드들 중 업그레이드 슬롯 카드와 같은 이름, 같은 등급을 가진 카드를 추려냄
        List<CardData> picked = new();
        picked.AddRange(new CardClassifier().GetCardsAvailableForMat(myCardData, cardDataOnUpSlot));

        SetMatCards(picked);
    }

    public void SetMatCards(List<CardData> _matCardDatas)
    {
        // 재료 CardData
        if (matCardsData == null) matCardsData = new();
        matCardsData.Clear();
        foreach (CardData item in _matCardDatas)
        {
            matCardsData.Add(item);
        }
        UpdateSlots();
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
            slot.transform.localScale = .6f * Vector3.one;
            slots.Add(slot);
        }

        // 재료 카드 생성. 슬롯위에 배치
        for (int i = 0; i < cardDatas.Count; i++)
        {
            if (cardDatas[i].Type == CardType.Weapon.ToString())
            {
                WeaponData wData = cardDictionary.GetWeaponData(cardDatas[i]);
                slots[i].GetComponent<CardSlot>().SetWeaponCard(cardDatas[i], wData, TargetSlot.MatSlot);
                slots[i].transform.localScale = new Vector2(0, 0);
                slots[i].transform.DOScale(new Vector2(.5f, .5f), .2f).SetEase(Ease.OutBack);
            }
            else
            {
                Item iData = cardDictionary.GetItemData(cardDatas[i]);
                slots[i].GetComponent<CardSlot>().SetItemCard(cardDatas[i], iData, TargetSlot.MatSlot);
                slots[i].transform.localScale = new Vector2(0, 0);
                slots[i].transform.DOScale(new Vector2(.5f, .5f), .2f).SetEase(Ease.OutBack);
            }
        }
    }

    public void ClearSlots()
    {
        int childCount = transform.childCount;
        if(childCount == 0) return;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            Destroy(child.gameObject);
        }
    }
    #endregion
}
