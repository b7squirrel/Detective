using UnityEngine;

public class CoinBuyButton : MonoBehaviour
{
    [SerializeField] RectTransform CoinPoint;
    [SerializeField] int m_productId;

    [Header("CoinNums는 나중에 없애기")]
    [SerializeField] int CoinNums;
    GemCollectFX gemCollectFX;
    TextEditorStore textEditorStore;
    PlayerDataManager playerDataManager;
    public void CoinCollectFX(int index)
    {
        if(gemCollectFX == null) gemCollectFX = FindObjectOfType<GemCollectFX>();
        if(textEditorStore == null) textEditorStore = FindObjectOfType<TextEditorStore>();

        int coinNums = textEditorStore.GetCoinNum(index);
        // float cristalCost = textEditorStore.GetCristalCost(index);

        // 먼저 실제 데이터에 골드를 모두 추가 (UI 업데이트 없이)
        if(playerDataManager == null) playerDataManager = FindObjectOfType<PlayerDataManager>();
        int currentValue = playerDataManager.GetCurrentCoinNumber();
        playerDataManager.SetCoinNumberAsSilent(currentValue + coinNums);

        gemCollectFX.PlayGemCollectFX(CoinPoint, coinNums, false);
    }
}
