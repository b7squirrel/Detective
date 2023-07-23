using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TempCardGen : MonoBehaviour
{
    [SerializeField] CardSo cardSo;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform[] slots;

    public void GenCard()
    {
        Transform newCard = Instantiate(cardPrefab, slots[0]).transform;
        RectTransform card = newCard.GetComponent<RectTransform>();
        RectTransform slot = slots[0].GetComponent<RectTransform>();
        card = slot;

        newCard.GetComponent<Card>().SetCardData(cardSo);
    }
}
