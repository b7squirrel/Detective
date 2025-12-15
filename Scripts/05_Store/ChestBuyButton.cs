using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChestBuyButton : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] Image productImage;
    [SerializeField] TextMeshProUGUI rewardText;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] Sprite productSprite;
    private ProductData productData;

    [Header("FX 위치")]
    [SerializeField] RectTransform gemPoint;

    /// <summary>
    /// 상품 정보 설정 (ShopUI에서 호출)
    /// </summary>
    public void SetInfo(ProductData data)
    {
        productData = data;
        UpdateUI();
    }
    
    /// <summary>
    /// UI 업데이트
    /// </summary>
    void UpdateUI()
    {
        if (productData == null)
        {
            Logger.LogWarning("[GemBuyButton] ProductData가 null입니다.");
            return;
        }
        
        Logger.Log($"[ChestBuyButton] 버튼 업데이트: {productData.ProductName}");
        Logger.Log($"[ChestBuyButton] - PurchaseType: {productData.PurchaseType}");
        Logger.Log($"[ChestBuyButton] - PurchaseCost: {productData.PurchaseCost}");
        Logger.Log($"[ChestBuyButton] - RewardCristal: {productData.RewardCristal}");
        
        // 이미지
        if (productImage != null && productSprite != null)
        {
            productImage.sprite = productSprite;
        }
        
        // // 보상 텍스트 (크리스탈 개수)
        // if (rewardText != null)
        // {
        //     rewardText.text = productData.RewardCristal.ToString();
        //     Logger.Log($"[GemBuyButton] 보상 텍스트 설정: {productData.RewardCristal}");
        // }
        
        // 비용 텍스트
        if (costText != null)
        {
            if (productData.PurchaseType == PurchaseType.IAP)
            {
                // ⭐ 센트 단위를 달러로 변환 (99 → $0.99)
                float dollars = productData.PurchaseCost / 100f;
                costText.text = $"${dollars:F2}";
                
                Logger.Log($"[GemBuyButton] IAP 가격 설정: {productData.PurchaseCost} 센트 → ${dollars:F2}");
            }
            else
            {
                costText.text = productData.PurchaseCost.ToString();
                Logger.Log($"[GemBuyButton] 일반 가격 설정: {productData.PurchaseCost}");
            }
        }
    }
    
    /// <summary>
    /// 버튼 클릭 시 호출 (Inspector에서 Button.onClick에 연결)
    /// </summary>
    public void OnPurchaseButtonClicked()
    {
        if (productData == null)
        {
            Logger.LogError("[GemBuyButton] ProductData가 설정되지 않았습니다.");
            return;
        }
        
        if (ShopManager.Instance == null)
        {
            Logger.LogError("[GemBuyButton] ShopManager를 찾을 수 없습니다.");
            return;
        }
        
        Logger.Log($"[GemBuyButton] 구매 버튼 클릭: {productData.ProductId}");
        
        // ⭐ ShopManager에게 구매 요청 + FX 위치 전달
        ShopManager.Instance.PurchaseProduct(productData.ProductId, gemPoint);
    }
}
