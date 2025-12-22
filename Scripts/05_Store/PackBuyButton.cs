using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PackBuyButton : MonoBehaviour
{
    [SerializeField] private Button buyButton;
    [SerializeField] private TextMeshProUGUI productNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    
    private ProductData productData;

    void Awake()
    {
        if (buyButton == null)
            buyButton = GetComponent<Button>();
        
        if (buyButton != null)
            buyButton.onClick.AddListener(OnButtonClick);
    }

    public void SetInfo(ProductData data)
    {
        productData = data;
        
        if (productNameText != null)
            productNameText.text = data.ProductName;
        
        if (priceText != null)
            priceText.text = $"{data.PurchaseCost}";
        
        Logger.Log($"[PackBuyButton] 설정 완료: {data.ProductId}");
    }

    void OnButtonClick()
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