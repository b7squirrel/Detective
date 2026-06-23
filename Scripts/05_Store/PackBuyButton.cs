using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 팩 구매 버튼
/// 팩 구성 설명을 통합 텍스트 박스 하나로 표시합니다.
/// LocalizationManager를 통해 다국어를 지원합니다.
/// </summary>
public class PackBuyButton : MonoBehaviour
{
    // ─── MyGrade.GradeColors 기준 hex ───
    const string COLOR_EPIC      = "#00CCFF";
    const string COLOR_LEGENDARY = "#B24CFF";
    const string COLOR_MYTHIC    = "#FFCC00";
    const string COLOR_WHITE     = "white";

    [Header("가격 텍스트")]
    [SerializeField] private TextMeshProUGUI priceText;

    [Header("팩 구성 설명 (통합 텍스트 박스 1개)")]
    [Tooltip("Rich Text가 활성화되어 있어야 합니다.")]
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("텍스트 정렬 위치 (px) — 씬에서 조정")]
    [Tooltip("점선 시작 위치")]
    [SerializeField] private float posDots = 380f;
    [Tooltip("수량 텍스트 위치")]
    [SerializeField] private float posCount = 460f;

    [Header("화면 전환")]
    [SerializeField] GameObject fg;

    [Header("FX 위치")]
    [SerializeField] RectTransform gemPoint;

    private ProductData productData;
    private Animator anim;
    private bool isProcessing = false;

    void Awake()
    {
        anim = GetComponent<Animator>();
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

        // ─── 가격 ───
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

        UpdateDescription(data);
        Logger.Log($"[PackBuyButton] 설정 완료: {data.ProductId}");
    }

    void UpdateDescription(ProductData data)
    {
        if (descriptionText == null) return;

        var g = LocalizationManager.Game;
        if (g == null) return;

        // ─── 등급명 ───
        string epic      = g.gradeNames[MyGrade.Epic];
        string legendary = g.gradeNames[MyGrade.Legendary];
        string mythic    = g.gradeNames[MyGrade.Mythic];

        // ─── 색상 태그 적용 ───
        string epicC      = $"<color={COLOR_EPIC}>{epic}</color>";
        string legendaryC = $"<color={COLOR_LEGENDARY}>{legendary}</color>";
        string mythicC    = $"<color={COLOR_MYTHIC}>{mythic}</color>";
        string orC        = $"<color={COLOR_WHITE}>{g.packOr}</color>";

        // ─── 단위 ───
        string unit     = g.packCountUnit;  // " 매" 또는 ""
        string coinUnit = g.packCoinUnit;   // "개" 또는 ""

        // ─── pos 태그 (인스펙터에서 조정) ───
        string dots1 = $"<pos={posDots}>·····";
        string dots2 = $"<pos={posDots}>··········";
        string dots3 = $"<pos={posDots}>···············";
        string cnt   = $"<pos={posCount}>";

        string line1, line2, line3;

        if (data.ProductId == "pack_001") // 초보자 팩
        {
            string duckGrade = epicC + orC + legendaryC;
            string duckLine  = string.Format(g.packDuckCardLine, duckGrade);
            string itemLine  = string.Format(g.packItemCardLine, epicC);

            line1 = $"<color={COLOR_WHITE}>{duckLine}{dots1}{cnt}1{unit}</color>";
            line2 = $"<color={COLOR_WHITE}>{itemLine}{dots2}{cnt}2{unit}</color>";
            line3 = $"<color={COLOR_WHITE}>{g.coin}{dots3}{cnt}{data.RewardGold:N0}{coinUnit}</color>";
        }
        else if (data.ProductId == "pack_003") // 전문가 팩
        {
            string duckGrade = legendaryC + orC + mythicC;
            string duckLine  = string.Format(g.packDuckCardLine, duckGrade);
            string itemLine  = string.Format(g.packItemCardLine, legendaryC);

            line1 = $"<color={COLOR_WHITE}>{duckLine}{dots1}{cnt}1{unit}</color>";
            line2 = $"<color={COLOR_WHITE}>{itemLine}{dots2}{cnt}2{unit}</color>";
            line3 = $"<color={COLOR_WHITE}>{g.coin}{dots3}{cnt}{data.RewardGold:N0}{coinUnit}</color>";
        }
        else return;

        descriptionText.text = $"{line1}\n{line2}\n{line3}";
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

        if (productData.RewardGold > 0 && gemPoint != null)
        {
            GachaPanelManager gachaPanelManager = FindObjectOfType<GachaPanelManager>(true);
            if (gachaPanelManager != null)
            {
                gachaPanelManager.RegisterGoldFX(gemPoint, productData.RewardGold);
                Logger.Log($"[PackBuyButton] 골드 FX 등록: {productData.RewardGold}골드");
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