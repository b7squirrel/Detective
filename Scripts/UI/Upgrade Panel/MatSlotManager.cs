using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatSlotManager : MonoBehaviour
{
    CardDataManager cardDataManager;
    CardsDictionary cardDictionary;
    int numSlots;
    [SerializeField] GameObject slotPrefab;

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
        int childCount = transform.childCount;
        for (int i = childCount -1; i >=0; i--)
        {
            Transform child = transform.GetChild(i);
            Destroy(child.gameObject);
        }
    }

    void UpdateSlots()
    {
        List<CardData> cardDatas = new List<CardData>();
        List<GameObject> slots = new List<GameObject>();

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
