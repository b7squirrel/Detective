using UnityEngine;

public class GemBuyButton : MonoBehaviour
{
    [SerializeField] RectTransform gemPoint;
    [SerializeField] int m_productId;

    [Header("gemNums는 나중에 없애기")]
    [SerializeField] int gemNums;
    GemCollectFX gemCollectFX;
    TextEditorStore textEditorStore;
    PlayerDataManager playerDataManager;
    public void GemCollectFX(int index)
    {
        if(gemCollectFX == null) gemCollectFX = FindObjectOfType<GemCollectFX>();
        if(textEditorStore == null) textEditorStore = FindObjectOfType<TextEditorStore>();

        int cristalNums = textEditorStore.GetCristalNums(index);
        // float cristalCost = textEditorStore.GetCristalCost(index);

        // 먼저 실제 데이터에 크리스탈을 모두 추가 (UI 업데이트 없이)
        if(playerDataManager == null) playerDataManager = FindObjectOfType<PlayerDataManager>();
        int currentValue = playerDataManager.GetCurrentCristalNumber();
        playerDataManager.SetCristalNumberAsSilent(currentValue + cristalNums);

        gemCollectFX.PlayGemCollectFX(gemPoint, cristalNums, true);
    }
}
