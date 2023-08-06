using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatSlotManager : MonoBehaviour
{
    CardDataManager cardDataManager;
    CardsDictionary cardDictionary;
    int numSlots;
    List<CardData> cardDatas;

    void Awake()
    {
        cardDataManager = FindObjectOfType<CardDataManager>();
        cardDictionary = FindObjectOfType<CardsDictionary>();
    }

    void OnEnable()
    {
        UpdateSlots();
    }

    void Update()
    {

    }

    void UpdateSlots()
    {
        if (cardDatas == null)
        {
            cardDatas = new List<CardData>();
        }

        cardDatas.Clear();
        cardDatas = cardDataManager.GetMyCardList();
        foreach (var item in cardDatas)
        {
            GameObject newCard = cardDictionary.GenCard(item.Type, item.Grade, item.Name);
            newCard.transform.SetParent(transform);
            newCard.transform.position = Vector3.zero;
            newCard.transform.localScale = Vector3.one;
            Debug.Log(item.Name + "가 생성되었습니다");

        }
    }
}
