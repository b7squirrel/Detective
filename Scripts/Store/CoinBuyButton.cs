using UnityEngine;

public class CoinBuyButton : MonoBehaviour
{
    [SerializeField] RectTransform CoinPoint;
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

        // 먼저 실제 데이터에 크리스탈을 모두 추가 (UI 업데이트 없이)
        if(playerDataManager == null) playerDataManager = FindObjectOfType<PlayerDataManager>();
        int currentValue = playerDataManager.GetCurrentCoinNumber();
        playerDataManager.SetCoinNumberAsSilent(currentValue + coinNums);

        gemCollectFX.PlayGemCollectFX(CoinPoint, coinNums, false);
    }
}
