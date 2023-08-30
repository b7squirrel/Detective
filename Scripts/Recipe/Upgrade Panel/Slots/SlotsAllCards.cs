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

    void OnEnable()
    {
        UpdateSlots();
    }
    void OnDisable()
    {
        ClearMyCardsSlots();
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
            slot.transform.localScale = .6f * Vector3.one;
            slots.Add(slot);
        }

        // 카드 데이터 정렬
        List<CardData> cardDataSorted = new();
        cardDataSorted.AddRange(cardDatas);

        // 내림차순으로 카드 정렬 
        cardDataSorted.Sort((a, b) =>
        {
            int indexA = new GradeConverter().ConvertStringToInt(a.Grade);
            int indexB = new GradeConverter().ConvertStringToInt(b.Grade);
            return indexA.CompareTo(indexB);
        });

        cardDataSorted.Reverse();

        // 카드 Display
        List<GameObject> cards = new();

        for (int i = 0; i < numSlots; i++)
        {
            if (cardDatas[i].Type == CardType.Weapon.ToString())
            {
                WeaponData wData = cardDictionary.GetWeaponData(cardDatas[i]);
                WeaponItemData data = new(wData, null);
                slots[i].GetComponent<CardSlot>().SetWeaponCard(cardDatas[i], data.weaponData, TargetSlot.MatSlot);
                
                if(wData == null)
                Debug.Log("slot all cards we = Null");
            }
            else
            {
                Item iData = cardDictionary.GetItemData(cardDatas[i]);
                WeaponItemData data = new WeaponItemData(null, iData);
                slots[i].GetComponent<CardSlot>().SetItemCard(cardDatas[i], data.itemData, TargetSlot.MatSlot);
            }
        }
    }

    public void ClearMyCardsSlots()
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
