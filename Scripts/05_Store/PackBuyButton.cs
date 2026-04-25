using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 팩 구매 버튼 (Inspector에서 Button.onClick에 연결하는 방식)
/// </summary>
public class PackBuyButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI priceText;

    [Header("화면 전환")]
    [SerializeField] GameObject fg;  // 인스펙터에서 연결

    private ProductData productData;
    private Animator anim;
    private bool isProcessing = false;  // 중복 클릭 방지

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    /// <summary>
    /// 상품 정보 설정 (ShopUI에서 호출)
    /// </summary>
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

    /// <summary>
    /// 버튼 클릭 시 호출 (Inspector에서 Button.onClick에 연결)
    /// </summary>
    public void OnPurchaseButtonClicked()
    {
        if (isProcessing) return;  // 이미 처리 중이면 무시

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
        ShopManager.Instance.PurchaseProduct(productData.ProductId);

        // ✅ 수정: isProcessing만 리셋 (fg는 ResetState에서 처리)
        // 가챠 패널이 열리는 경우 ResetState()가 GachaPanelManager에서 호출됨
        isProcessing = false;
    }

    // ✅ 추가: 가챠 패널이 닫힐 때 호출
    public void ResetState()
    {
        isProcessing = false;
        if (fg != null) fg.SetActive(false);
    }
}