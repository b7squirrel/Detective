using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    List<Card> cards;
    List<Transform> matSlots;
    List<CardData> cardData;
    [SerializeField] Transform upgradeSlot;

    public List<Card> UpdateCardsList()
    {
        if(cards == null) cards = new List<Card>();
        if(matSlots == null) matSlots = new List<Transform>();

        cards.Clear();
        matSlots.Clear();

        foreach (var item in matSlots) // matSlot에 있는 카드들을 리스트에 추가
        {
            if(item.GetComponentInChildren<Card>() != null)
            {
                cards.Add(item.GetComponentInChildren<Card>());
            }
        }

        // upgradeSlot에 있는 카드를 리스트에 추가
        if(upgradeSlot.GetComponentInChildren<Card>() != null)
        {
            cards.Add(upgradeSlot.GetComponentInChildren<Card>());
        }

        return cards;
    }
}
