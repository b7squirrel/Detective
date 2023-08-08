using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCardsSlotManager : MonoBehaviour
{
    CardDataManager cardDataManager;
    CardsDictionary cardDictionary;
    UpgradePanelSlotsManager upgradePanelSlotsManager;
    [SerializeField] UpgradeSuccessUI upgradeSuccessUI; // 왜인지 모르겠지만 FIndObjedt..가 안됨
    int numSlots;
    [SerializeField] GameObject slotPrefab;

    void Awake()
    {
        cardDataManager = FindObjectOfType<CardDataManager>();
        cardDictionary = FindObjectOfType<CardsDictionary>();
        upgradePanelSlotsManager = GetComponentInParent<UpgradePanelSlotsManager>();
    }

    public void UpdateSlots()
    {
        List<CardData> cardDatas = new List<CardData>();
        List<GameObject> slots = new List<GameObject>();

        cardDatas.AddRange(cardDataManager.GetMyCardList());

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

    //패널 최상단 UpgradePanelSlotsManager의 OnRefresh 유니티 이벤트에 등록되어 있음
    public void ClearMyCardsSlots()
    {
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            Destroy(child.gameObject);
        }

        upgradeSuccessUI.gameObject.SetActive(false);
        UpdateSlots();
    }
}
