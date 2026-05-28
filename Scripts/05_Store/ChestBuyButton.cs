using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public enum ChestType { Duck, Item, Other }

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
    [SerializeField] GameObject fg;

    [Header("튜토리얼")]
    [SerializeField] ChestType chestType = ChestType.Other;

    private ProductData productData;
    private Animator animator;
    private bool isProcessing = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetInfo(ProductData data)
    {
        productData = data;
        UpdateUI();
    }

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
            productImage.sprite = productSprite;

        if (costText != null)
        {
            if (productData.PurchaseType == PurchaseType.IAP)
            {
                float dollars = productData.PurchaseCost / 100f;
                costText.text = $"${dollars:F2}";
            }
            else
            {
                costText.text = $"{productData.PurchaseCost:N0}";
            }
        }
    }

    public void OnPurchaseButtonClicked()
    {
        if (isProcessing) return;

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

        // 2. FG 활성화 (터치 차단)
        if (fg != null) fg.SetActive(true);

        // 3. 애니메이션 대기
        yield return new WaitForSeconds(0.1f);

        if (TutorialManager.instance?.CurrentStep == TutorialStep.Step1_ShopUnlocked
            && ShopTutorialController.instance != null)
        {
            ShopTutorialController.instance.OnGachaOpened(chestType);
        }

        Debug.Log($"[ChestBuy] PurchaseProduct 호출: {Time.time}");

        // 경고 팝업이 닫힐 때 isProcessing을 해제하는 콜백
        System.Action onWarningClosed = () =>
        {
            isProcessing = false;
        };

        bool success = ShopManager.Instance.PurchaseProduct(productData.ProductId, gemPoint, onWarningClosed);

        if (!success)
        {
            // FG는 다음 프레임에 끔
            // (Warning BG가 켜진 후에 끄면 자연스러운 전환)
            StartCoroutine(DeactivateFGNextFrame());
            isProcessing = false;  // 단, isProcessing은 바로 콜백으로 처리
            // isProcessing은 경고 팝업이 닫힐 때 onWarningClosed 콜백에서 해제
        }
        // 성공 시: isProcessing과 FG 모두 GachaPanelManager가 닫힐 때 ResetState()에서 해제
    }

    IEnumerator DeactivateFGNextFrame()
    {
        yield return null;  // Warning BG가 켜진 후 한 프레임 대기
        if (fg != null) fg.SetActive(false);
    }

    // 가챠 패널이 닫힐 때 GachaPanelManager에서 호출
    public void ResetState()
    {
        isProcessing = false;

        // ✅ 튜토리얼 중에는 fg를 ShopTutorialController가 관리하므로 건드리지 않음
        bool inShopTutorial = TutorialManager.instance?.CurrentStep == TutorialStep.Step1_ShopUnlocked;
        if (fg != null && !inShopTutorial)
            fg.SetActive(false);
    }
}