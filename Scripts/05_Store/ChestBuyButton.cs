using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

// ✅ 추가: 어떤 종류의 뽑기 버튼인지 구분
public enum ChestType { Duck, Item, Other }

/// <summary>
/// 상자 구매 버튼 (Inspector에서 Button.onClick에 연결하는 방식)
/// </summary>
public class ChestBuyButton : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] Image productImage;
    [SerializeField] TextMeshProUGUI rewardText;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] Sprite productSprite;

    [Header("FX 위치")]
    [SerializeField] RectTransform gemPoint;

    [Header("화면 전환")]
    [SerializeField] GameObject fg;  // 인스펙터에서 연결

    [Header("튜토리얼")]
    [SerializeField] ChestType chestType = ChestType.Other; // 인스펙터에서 설정

    private ProductData productData;
    private Animator animator;
    private bool isProcessing = false;  // 중복 클릭 방지

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

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
            Logger.LogWarning("[ChestBuyButton] ProductData가 null입니다.");
            return;
        }

        Logger.Log($"[ChestBuyButton] 버튼 업데이트: {productData.ProductName}");
        Logger.Log($"[ChestBuyButton] - PurchaseType: {productData.PurchaseType}");
        Logger.Log($"[ChestBuyButton] - PurchaseCost: {productData.PurchaseCost}");

        if (productImage != null && productSprite != null)
        {
            productImage.sprite = productSprite;
        }

        if (costText != null)
        {
            if (productData.PurchaseType == PurchaseType.IAP)
            {
                float dollars = productData.PurchaseCost / 100f;
                costText.text = $"${dollars:F2}";
                Logger.Log($"[ChestBuyButton] IAP 가격 설정: {productData.PurchaseCost} 센트 → ${dollars:F2}");
            }
            else
            {
                costText.text = $"{productData.PurchaseCost:N0}";
                Logger.Log($"[ChestBuyButton] 일반 가격 설정: {productData.PurchaseCost}");
            }
        }
    }

    /// <summary>
    /// 버튼 클릭 시 호출 (Inspector에서 Button.onClick에 연결)
    /// </summary>
    public void OnPurchaseButtonClicked()
    {
        if (isProcessing) return;  // 이미 처리 중이면 무시

        if (productData == null)
        {
            Logger.LogError("[ChestBuyButton] ProductData가 설정되지 않았습니다.");
            return;
        }

        if (ShopManager.Instance == null)
        {
            Logger.LogError("[ChestBuyButton] ShopManager를 찾을 수 없습니다.");
            return;
        }

        StartCoroutine(PurchaseSequence());
    }

    private IEnumerator PurchaseSequence()
    {
        if (isProcessing)
        {
            Debug.Log("[ChestBuy] 이미 처리 중 — 중복 호출 차단");
            yield break;
        }

        isProcessing = true;
        Debug.Log($"[ChestBuy] PurchaseSequence 시작: {productData.ProductId}, Time: {Time.time}");

        // 1. 버튼 눌림 애니메이션
        if (animator != null) animator.SetTrigger("Pressed");

        // 2. FG 즉시 활성화 (다른 버튼 입력 차단)
        if (fg != null) fg.SetActive(true);

        // 3. 애니메이션이 끝날 때까지 대기 (0.1초)
        yield return new WaitForSeconds(0.1f);

        // // 4. 화면 전환 요청
        // Logger.Log($"[ChestBuyButton] 구매 버튼 클릭: {productData.ProductId}");
        // ShopManager.Instance.PurchaseProduct(productData.ProductId, gemPoint);

        if (TutorialManager.instance?.CurrentStep == TutorialStep.Step1_ShopUnlocked
        && ShopTutorialController.instance != null)
        {
            ShopTutorialController.instance.OnGachaOpened(chestType);
        }

        Debug.Log($"[ChestBuy] PurchaseProduct 호출: {Time.time}");
        ShopManager.Instance.PurchaseProduct(productData.ProductId, gemPoint);

        // ✅ isProcessing은 가챠 패널이 닫힌 후에 리셋되어야 함
        // 지금은 여기서 바로 false로 바꾸지 않음
        // isProcessing = false; ← 제거
    }

    // ✅ 추가: 가챠 패널이 닫힐 때 호출 (GachaPanelManager에서 호출)
    public void ResetState()
    {
        isProcessing = false;
        if (fg != null) fg.SetActive(false);
    }
}