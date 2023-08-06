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
            cardDictionary.GenCard(item.Type, item.Grade, item.Name);
        }
    }
}
