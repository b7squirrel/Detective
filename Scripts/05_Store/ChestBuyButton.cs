using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public enum ChestType { Duck, Item, Other }

/// <summary>
/// 상자 구매 버튼 (1뽑 / 10뽑)
/// 설명은 1뽑 버튼에만 연결합니다.
/// 10뽑 버튼은 가격(costText)만 연결하면 됩니다.
/// </summary>
public class ChestBuyButton : MonoBehaviour
{
    const string COLOR_RARE      = "#80FF00";
    const string COLOR_LEGENDARY = "#B24CFF";
    const string COLOR_WHITE     = "white";

    [Header("UI Components")]
    [SerializeField] Image productImage;
    [SerializeField] Sprite productSprite;
    [SerializeField] TextMeshProUGUI costText;

    [Header("설명 행 — 1뽑 버튼에만 연결")]
    [Tooltip("Row_Single > LeftText")]
    [SerializeField] TextMeshProUGUI singleLeftText;
    [Tooltip("Row_Single > RightText")]
    [SerializeField] TextMeshProUGUI singleRightText;

    [Tooltip("Row_Ten_Normal > LeftText")]
    [SerializeField] TextMeshProUGUI tenNormalLeftText;
    [Tooltip("Row_Ten_Normal > RightText")]
    [SerializeField] TextMeshProUGUI tenNormalRightText;

    [Tooltip("Row_Ten_Guarantee > LeftText")]
    [SerializeField] TextMeshProUGUI tenGuarLeftText;
    [Tooltip("Row_Ten_Guarantee > RightText")]
    [SerializeField] TextMeshProUGUI tenGuarRightText;

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

    void Start()
    {
        LocalizationManager.OnLanguageChanged += RefreshDescription;
    }

    void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= RefreshDescription;
    }

    void RefreshDescription()
    {
        if (productData != null)
            UpdateDescription(productData);
    }

    public void SetInfo(ProductData data)
    {
        productData = data;

        if (productImage != null && productSprite != null)
            productImage.sprite = productSprite;

        if (costText != null)
        {
            if (data.PurchaseType == PurchaseType.IAP)
                costText.text = $"${data.PurchaseCost / 100f:F2}";
            else
                costText.text = $"{data.PurchaseCost:N0}";
        }

        UpdateDescription(data);
        Logger.Log($"[ChestBuyButton] 설정 완료: {data.ProductId}");
    }

    void UpdateDescription(ProductData data)
    {
        // 설명 필드가 없으면 10뽑 버튼 — 가격만 처리
        if (singleLeftText == null) return;

        var g = LocalizationManager.Game;
        if (g == null) return;

        string unit       = g.packCountUnit;                    // " 매" / ""
        string rareC      = $"<color={COLOR_RARE}>{g.gradeNames[MyGrade.Rare]}</color>";
        string legendaryC = $"<color={COLOR_LEGENDARY}>{g.gradeNames[MyGrade.Legendary]}</color>";
        string above      = $"<color={COLOR_WHITE}>{g.chestAbove}</color>";
        string w          = COLOR_WHITE;

        // ─── GameTexts에서 레이블 가져오기 ───
        string label1x   = $"<color={w}>{g.chestLabel1x}  </color>";
        string label10x  = $"<color={w}>{g.chestLabel10x} </color>";
        string labelPlus = $"<color={w}>   {g.chestLabelPlus}  </color>";

        // 카드 이름
        string cardName = (data.ProductId == "chest_001" || data.ProductId == "chest_002")
            ? g.duckCard
            : g.itemCard;

        // ─── Row_Single ───
        if (singleLeftText  != null)
            singleLeftText.text  = $"{label1x}{rareC}{above} <color={w}>{cardName}</color>";
        if (singleRightText != null)
            singleRightText.text = $"<color={w}>1{unit}</color>";

        // ─── Row_Ten_Normal ───
        if (tenNormalLeftText  != null)
            tenNormalLeftText.text  = $"{label10x}{rareC}{above} <color={w}>{cardName}</color>";
        if (tenNormalRightText != null)
            tenNormalRightText.text = $"<color={w}>9{unit}</color>";

        // ─── Row_Ten_Guarantee ───
        if (tenGuarLeftText  != null)
            tenGuarLeftText.text  = $"{labelPlus}{legendaryC}{above} <color={w}>{cardName}</color>";
        if (tenGuarRightText != null)
            tenGuarRightText.text = $"<color={w}>1{unit}</color>";
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

        if (animator != null) animator.SetTrigger("Pressed");
        if (fg != null) fg.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        if (TutorialManager.instance?.CurrentStep == TutorialStep.Step1_ShopUnlocked
            && ShopTutorialController.instance != null)
        {
            ShopTutorialController.instance.OnGachaOpened(chestType);
        }

        System.Action onWarningClosed = () => { isProcessing = false; };

        bool success = ShopManager.Instance.PurchaseProduct(
            productData.ProductId, gemPoint, onWarningClosed);

        if (!success)
        {
            StartCoroutine(DeactivateFGNextFrame());
            isProcessing = false;
        }
    }

    IEnumerator DeactivateFGNextFrame()
    {
        yield return null;
        if (fg != null) fg.SetActive(false);
    }

    public void ResetState()
    {
        isProcessing = false;

        bool inShopTutorial =
            TutorialManager.instance?.CurrentStep == TutorialStep.Step1_ShopUnlocked;
        if (fg != null && !inShopTutorial)
            fg.SetActive(false);
    }
}