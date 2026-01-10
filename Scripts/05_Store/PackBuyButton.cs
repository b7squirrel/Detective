using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 팩 구매 버튼 (Inspector에서 Button.onClick에 연결하는 방식)
/// </summary>
public class PackBuyButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI priceText;
    
    private ProductData productData;

    /// <summary>
    /// 상품 정보 설정 (ShopUI에서 호출)
    /// </summary>
    public void SetInfo(ProductData data)
    {
        productData = data;
        
        // if (productNameText != null)
        //     productNameText.text = data.ProductName;
            
        if (priceText != null)
        {
            // IAP인 경우 달러 표시
            if (data.PurchaseType == PurchaseType.IAP)
            {
                float dollars = data.PurchaseCost / 100f;
                priceText.text = $"${dollars:F2}";
            }
            else
            {
                priceText.text = data.PurchaseCost.ToString();
            }
        }
            
        Logger.Log($"[PackBuyButton] 설정 완료: {data.ProductId}");
    }

    /// <summary>
    /// 버튼 클릭 시 호출 (Inspector에서 Button.onClick에 연결)
    /// </summary>
    public void OnPurchaseButtonClicked()
    {
        if (productData == null)
        {
            Logger.LogError("[PackBuyButton] ProductData가 없습니다.");
            return;
        }

        Logger.Log($"[PackBuyButton] 클릭: {productData.ProductId}");

        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.PurchaseProduct(productData.ProductId);
        }
        else
        {
            Logger.LogError("[PackBuyButton] ShopManager를 찾을 수 없습니다.");
        }
    }
}