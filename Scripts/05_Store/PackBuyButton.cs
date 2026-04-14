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

        // 1. 버튼 눌림 애니메이션
        if (anim != null)
            anim.SetTrigger("Pressed");

        // 2. FG 즉시 활성화 (다른 버튼 입력 차단)
        if (fg != null)
            fg.SetActive(true);

        // 3. 애니메이션이 끝날 때까지 대기 (0.1초)
        yield return new WaitForSeconds(0.1f);

        // 4. 화면 전환 요청
        Logger.Log($"[PackBuyButton] 클릭: {productData.ProductId}");
        ShopManager.Instance.PurchaseProduct(productData.ProductId);

        isProcessing = false;
    }
}