using System.Collections.Generic;
using UnityEngine;

public class SetToMaxLevel : MonoBehaviour
{
    public void SetAllToMaxLevel()
    {
        CardDataManager cardDataManager = FindObjectOfType<CardDataManager>();
        cardDataManager.SetAllToMaxLevel();
    }
}