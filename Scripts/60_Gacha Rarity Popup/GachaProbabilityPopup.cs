using UnityEngine;
using System.Collections.Generic;

public class GachaProbabilityPopup : SingletonBehaviour<GachaProbabilityPopup>
{
    [Header("공통 UI")]
    [SerializeField] private GameObject darkBG;
    [SerializeField] private GameObject closeButton;

    [Header("스크롤 콘텐츠")]
    [SerializeField] private Transform contentContainer; // ScrollView > Content

    [Header("프리팹")]
    [SerializeField] private ProbabilitySectionUI sectionPrefab;
    [SerializeField] private RarityRowUI rowPrefab;

    private List<ProbabilitySectionUI> spawnedSections = new List<ProbabilitySectionUI>();

    protected override void Init()
    {
        base.Init();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 가챠 테이블이 있는 모든 상품의 확률을 순회하여 팝업 전체를 채웁니다.
    /// 상점의 "확률 정보" 버튼 하나에서 이 메서드를 호출하면 됩니다.
    /// </summary>
    public void ShowAll()
    {
        if (ProductDataTable.Instance == null || GachaRarityTable.Instance == null)
        {
            Logger.LogError("[GachaProbabilityPopup] 필요한 데이터 테이블을 찾을 수 없습니다.");
            return;
        }

        gameObject.SetActive(true);

        ClearSections();

        var allProducts = ProductDataTable.Instance.GetAllProducts();
        foreach (var product in allProducts)
        {
            if (string.IsNullOrEmpty(product.GachaTableId))
                continue;

            BuildSectionFor(product);
        }
    }

    void BuildSectionFor(ProductData product)
    {
        string tableId = product.GachaTableId;

        var normalProbs = GachaRarityTable.Instance.GetProbabilityPercentages(tableId, "Normal");
        bool hasGuarantee = GachaRarityTable.Instance.HasSlotType(tableId, "Guarantee");

        if (normalProbs == null)
        {
            Logger.LogWarning($"[GachaProbabilityPopup] 확률 정보 없음: {product.ProductId} ({tableId})");
            return;
        }

        ProbabilitySectionUI section = Instantiate(sectionPrefab, contentContainer);
        section.SetTitle(GetLocalizedProductName(product.ProductId));
        section.SetHasGuarantee(hasGuarantee);
        section.SetShowNormalSubLabel(hasGuarantee);

        foreach (var (rarity, percent) in normalProbs)
            SpawnRow(section.NormalRowContainer, rarity, percent);

        if (hasGuarantee)
        {
            var guarProbs = GachaRarityTable.Instance.GetProbabilityPercentages(tableId, "Guarantee");
            foreach (var (rarity, percent) in guarProbs)
                SpawnRow(section.GuaranteeRowContainer, rarity, percent);
        }

        spawnedSections.Add(section);
    }

    string GetLocalizedProductName(string productId)
    {
        var g = LocalizationManager.Game;
        if (g == null) return productId;

        switch (productId)
        {
            case "pack_001": return g.beginnerPack;
            case "pack_003": return g.ExpertPack;
            case "chest_001": return $"{g.chestLabel1x} {g.duckCard}";
            case "chest_002": return $"{g.chestLabel10x} {g.duckCard}";
            case "chest_003": return $"{g.chestLabel1x} {g.itemCard}";
            case "chest_004": return $"{g.chestLabel10x} {g.itemCard}";
            case "chest_005": return g.luckyBox;
            default:
                Logger.LogWarning($"[GachaProbabilityPopup] 로컬라이징 매핑 없음: {productId}");
                return productId;
        }
    }

    void SpawnRow(Transform container, int rarity, float percent)
    {
        RarityRowUI row = Instantiate(rowPrefab, container);
        row.SetInfo(rarity, percent);
    }

    void ClearSections()
    {
        foreach (var section in spawnedSections)
        {
            if (section != null) Destroy(section.gameObject);
        }
        spawnedSections.Clear();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}