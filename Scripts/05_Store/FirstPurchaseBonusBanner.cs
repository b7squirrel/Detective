using UnityEngine;

public class FirstPurchaseBonusBanner : MonoBehaviour
{
    void OnEnable()
    {
        RefreshVisibility();
        GemCollectFX.OnAllGemsCollected += RefreshVisibility;
    }

    void OnDisable()
    {
        GemCollectFX.OnAllGemsCollected -= RefreshVisibility;
    }

    void RefreshVisibility()
    {
        bool alreadyClaimed = PlayerDataManager.Instance != null &&
                               PlayerDataManager.Instance.HasClaimedFirstCristalBonus();
        gameObject.SetActive(!alreadyClaimed);
    }
}