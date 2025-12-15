using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 상점 상품 구매 버튼 (UI 표시 + 클릭 이벤트만 담당)
/// 실제 구매 로직은 ShopManager가 처리
/// </summary>
public class CoinBuyButton : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] Image productImage;
    [SerializeField] TextMeshProUGUI rewardText;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] Sprite productSprite;
    
    [Header("FX 위치")]
    [SerializeField] RectTransform coinPoint;
    
    private ProductData productData;
    
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
            Logger.LogWarning("[CoinBuyButton] ProductData가 null입니다.");
            return;
        }
        
        // 이미지
        if (productImage != null && productSprite != null)
        {
            productImage.sprite = productSprite;
        }
        
        // 보상 텍스트
        if (rewardText != null)
        {
            rewardText.text = productData.RewardGold.ToString();
        }
        
        // 비용 텍스트
        if (costText != null)
        {
            costText.text = productData.PurchaseCost.ToString();
        }
    }
    
    /// <summary>
    /// 버튼 클릭 시 호출 (Inspector에서 Button.onClick에 연결)
    /// </summary>
    public void OnPurchaseButtonClicked()
    {
        if (productData == null)
        {
            Logger.LogError("[CoinBuyButton] ProductData가 설정되지 않았습니다.");
            return;
        }
        
        if (ShopManager.Instance == null)
        {
            Logger.LogError("[CoinBuyButton] ShopManager를 찾을 수 없습니다.");
            return;
        }
        
        // ⭐ ShopManager에게 구매 요청 + FX 위치 전달
        ShopManager.Instance.PurchaseProduct(productData.ProductId, coinPoint);
    }
    
    /// <summary>
    /// FX 재생 위치 반환
    /// </summary>
    public RectTransform GetCoinPoint() => coinPoint;
}