using UnityEngine;
using UnityEngine.UI;

public class StorePanelManager : MonoBehaviour
{
    [SerializeField] CardSlotManager cardSlotManager;
    [SerializeField] ScrollRect scrollRect;
    void OnEnable()
    {
        if(cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager.SettrigerAnim("Off"); // 필드가 필요하지 않으니 Off시키기

        if (scrollRect == null) scrollRect = GetComponentInChildren<ScrollRect>();
        scrollRect.verticalNormalizedPosition = 1f;
    }
}
