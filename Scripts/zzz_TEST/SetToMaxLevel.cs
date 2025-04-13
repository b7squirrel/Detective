using System.Collections.Generic;
using UnityEngine;

public class SetToMaxLevel : MonoBehaviour
{
    CardSlotManager cardSlotManager;
    public void SetAllToMaxLevel()
    {
        CardDataManager cardDataManager = FindObjectOfType<CardDataManager>();
        cardDataManager.SetAllToMaxLevel();

        if(cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager.UpdateAllCardSlotDisplay();
    }
}