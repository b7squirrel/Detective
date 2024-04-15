using System.Collections.Generic;
using UnityEngine;

public class SetToMaxLevel : MonoBehaviour
{
    public void SetAllToMaxLevel()
    {
        List<CardData> cards = FindObjectOfType<CardDataManager>().GetMyCardList();
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].Level = 30;
        }
    }
}