using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 팩 구매 버튼
/// 각 행을 Left / Dots / Right TMP 세 개로 분리해서 오른쪽 정렬을 처리합니다.
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

    [Header("Row_Duck")]
    [SerializeField] private TextMeshProUGUI duckLeftText;   // "고급 또는 전설 오리 카드"
    [SerializeField] private TextMeshProUGUI duckDotsText;   // "····················"
    [SerializeField] private TextMeshProUGUI duckRightText;  // "1 매"

    [Header("Row_Item")]
    [SerializeField] private TextMeshProUGUI itemLeftText;
    [SerializeField] private TextMeshProUGUI itemDotsText;
    [SerializeField] private TextMeshProUGUI itemRightText;  // "2 매"

    [Header("Row_Gold")]
    [SerializeField] private TextMeshProUGUI goldLeftText;
    [SerializeField] private TextMeshProUGUI goldDotsText;
    [SerializeField] private TextMeshProUGUI goldRightText;  // "10,000개"

    [Header("점선 문자 — 씬에서 길이 조정")]
    [SerializeField] private string dotsString = "····················";

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

        // ─── 점선 (모든 행 동일) ───
        SetDots();

        if (data.ProductId == "pack_001") // 초보자 팩
        {
            // Row_Duck
            string duckGrade = epicC + orC + legendaryC;
            SetRow(
                duckLeftText,
                string.Format(g.packDuckCardLine, duckGrade),
                duckRightText,
                $"1{unit}"
            );

            // Row_Item
            SetRow(
                itemLeftText,
                string.Format(g.packItemCardLine, epicC),
                itemRightText,
                $"2{unit}"
            );

            // Row_Gold
            SetRow(
                goldLeftText,
                g.coin,
                goldRightText,
                $"{data.RewardGold:N0}{coinUnit}"
            );
        }
        else if (data.ProductId == "pack_003") // 전문가 팩
        {
            // Row_Duck
            string duckGrade = legendaryC + orC + mythicC;
            SetRow(
                duckLeftText,
                string.Format(g.packDuckCardLine, duckGrade),
                duckRightText,
                $"1{unit}"
            );

            // Row_Item
            SetRow(
                itemLeftText,
                string.Format(g.packItemCardLine, legendaryC),
                itemRightText,
                $"2{unit}"
            );

            // Row_Gold
            SetRow(
                goldLeftText,
                g.coin,
                goldRightText,
                $"{data.RewardGold:N0}{coinUnit}"
            );
        }
    }

    /// <summary>
    /// Left/Right 텍스트를 한 번에 세팅합니다.
    /// Left는 Rich Text(색상 태그 포함), Right는 흰색 단순 텍스트.
    /// </summary>
    void SetRow(TextMeshProUGUI left, string leftContent,
                TextMeshProUGUI right, string rightContent)
    {
        if (left  != null) left.text  = leftContent;
        if (right != null) right.text = $"<color={COLOR_WHITE}>{rightContent}</color>";
    }

    /// <summary>
    /// 점선 텍스트를 모든 행에 동일하게 세팅합니다.
    /// dotsString은 인스펙터에서 조정하세요.
    /// </summary>
    void SetDots()
    {
        string d = $"<color={COLOR_WHITE}>{dotsString}</color>";
        if (duckDotsText != null) duckDotsText.text = d;
        if (itemDotsText != null) itemDotsText.text = d;
        if (goldDotsText != null) goldDotsText.text = d;
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