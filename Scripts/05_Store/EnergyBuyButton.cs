using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 크리스탈로 번개 소량충전 구매 버튼 (energy_001, energy_002 공용)
/// </summary>
public class EnergyBuyButton : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] Image productImage;
    [SerializeField] TextMeshProUGUI rewardText;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] Sprite productSprite;

    [Header("FX 위치")]
    [SerializeField] RectTransform energyPoint;

    private ProductData productData;
    Animator anim;

    public void SetInfo(ProductData data)
    {
        productData = data;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (productData == null)
        {
            Logger.LogWarning("[EnergyBuyButton] ProductData가 null입니다.");
            return;
        }

        if (productImage != null && productSprite != null)
            productImage.sprite = productSprite;

        // 보상 텍스트 (번개 개수)
        if (rewardText != null)
            rewardText.text = $"x{productData.RewardEnergy:N0}";

        // 비용 텍스트 (크리스탈)
        if (costText != null)
            costText.text = $"{productData.PurchaseCost:N0}";
    }

    public void OnPurchaseButtonClicked()
    {
        if (productData == null)
        {
            Logger.LogError("[EnergyBuyButton] ProductData가 설정되지 않았습니다.");
            return;
        }

        ShopManager shopManager = FindObjectOfType<ShopManager>();
        if (shopManager == null)
        {
            Logger.LogError("[EnergyBuyButton] ShopManager를 찾을 수 없습니다.");
            return;
        }

        if (anim == null) anim = GetComponent<Animator>();
        if (anim != null) anim.SetTrigger("Pressed");

        shopManager.PurchaseProduct(productData.ProductId, energyPoint);
    }

    public RectTransform GetEnergyPoint() => energyPoint;
}