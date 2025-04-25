using UnityEngine;

public class AchievementsPanelManager : MonoBehaviour
{
    CardSlotManager cardSlotManager;

    void OnEnable()
    {
        if(cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager.SettrigerAnim("Off");
    }

    void Update()
    {
        
    }
}
