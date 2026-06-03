using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 팩 구매 버튼
/// </summary>
public class PackBuyButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI priceText;

    [Header("화면 전환")]
    [SerializeField] GameObject fg;

    // ⭐ 골드 FX 시작 위치
    [Header("FX 위치")]
    [SerializeField] RectTransform gemPoint;

    private ProductData productData;
    private Animator anim;
    private bool isProcessing = false;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void SetInfo(ProductData data)
    {
        productData = data;
        if (priceText != null)
        {
            if (data.PurchaseType == PurchaseType.IAP)
            {
                float dollars = data.PurchaseCost / 100f;
                priceText.text = $"${dollars:F2}";
            }
            else
            {
                priceText.text = $"{data.PurchaseCost:N0}";
            }
        }
        Logger.Log($"[PackBuyButton] 설정 완료: {data.ProductId}");
    }

    public void OnPurchaseButtonClicked()
    {
        if (isProcessing) return;

        if (productData == null)
        {
            Logger.LogError("[PackBuyButton] ProductData가 없습니다.");
            return;
        }

        if (ShopManager.Instance == null)
        {
            Logger.LogError("[PackBuyButton] ShopManager를 찾을 수 없습니다.");
            return;
        }

        StartCoroutine(PurchaseSequence());
    }

    private IEnumerator PurchaseSequence()
    {
        isProcessing = true;

        if (anim != null) anim.SetTrigger("Pressed");
        if (fg != null) fg.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        Logger.Log($"[PackBuyButton] 클릭: {productData.ProductId}");

        // ⭐ 골드 보상이 있는 팩이면 GachaPanelManager에 FX 파라미터 미리 등록
        if (productData.RewardGold > 0 && gemPoint != null)
        {
            GachaPanelManager gachaPanelManager = FindObjectOfType<GachaPanelManager>(true);
            if (gachaPanelManager != null)
            {
                gachaPanelManager.RegisterGoldFX(gemPoint, productData.RewardGold);
                Logger.Log($"[PackBuyButton] 골드 FX 등록: {productData.RewardGold}골드, 위치: {gemPoint.name}");
            }
        }

        ShopManager.Instance.PurchaseProduct(productData.ProductId, gemPoint);

        isProcessing = false;
    }

    public void ResetState()
    {
        isProcessing = false;
        if (fg != null) fg.SetActive(false);
    }
}