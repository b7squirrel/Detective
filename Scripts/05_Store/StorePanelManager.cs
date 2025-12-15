using UnityEngine;

public class StorePanelManager : MonoBehaviour
{
    CardSlotManager cardSlotManager;
    void OnEnable()
    {
        if(cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager.SettrigerAnim("Off"); // 필드가 필요하지 않으니 Off시키기
    }
}
