using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatSlotManager : MonoBehaviour
{
    CardDataManager cardDataManager;
    CardsDictionary cardDictionary;
    int numSlots;
    List<CardData> cardDatas;
    [SerializeField] GameObject slotPrefab;
    List<GameObject> slots;

    void Awake()
    {
        cardDataManager = FindObjectOfType<CardDataManager>();
        cardDictionary = FindObjectOfType<CardsDictionary>();
    }

    void OnEnable()
    {
        UpdateSlots();
    }

    void UpdateSlots()
    {
        if (cardDatas == null) cardDatas = new List<CardData>();

        if(slots == null) slots = new List<GameObject>();
        

        cardDatas.Clear();
        cardDatas = cardDataManager.GetMyCardList();

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

            Debug.Log(cardDatas[i].Name + "가 생성되었습니다");
        }


        
    }
}
