using System.Collections.Generic;
using UnityEngine;

public class LuckyBoxTable : SingletonBehaviour<LuckyBoxTable>
{
    [SerializeField] private TextAsset luckyBoxTableCSV;

    [Header("디버그")]
    [Tooltip("체크하면 확률 무시하고 아래 productId가 100% 당첨됩니다. GameConfig.isDebugMode가 꺼져 있으면 무시됩니다.")]
    [SerializeField] private bool debugForceProduct = false;
    [SerializeField] private string debugForcedProductId;

    private List<(string productId, int weight)> entries = new();

    protected override void Init()
    {
        base.Init();

        if (luckyBoxTableCSV != null)
        {
            ParseCSV(luckyBoxTableCSV.text);
        }
        else
        {
            Logger.LogError("[LuckyBoxTable] CSV 파일이 할당되지 않았습니다.");
        }
    }

    private void ParseCSV(string csvText)
    {
        entries.Clear();

        string normalizedText = csvText.Replace("\r\n", "\n").Replace("\r", "\n");
        string[] lines = normalizedText.Split('\n');

        for (int i = 1; i < lines.Length; i++) // 0번째는 헤더
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] fields = line.Split(',');
            if (fields.Length < 2) continue;

            string productId = fields[0].Trim();
            if (!int.TryParse(fields[1].Trim(), out int weight)) continue;

            entries.Add((productId, weight));
        }

        Logger.Log($"[LuckyBoxTable] {entries.Count}개 상품 로드됨");
    }

    /// <summary>
    /// 가중치 기반으로 상품 하나를 뽑아 ProductData를 반환합니다.
    /// 디버그 모드에서 강제 당첨이 설정되어 있으면 그 상품을 100% 반환합니다.
    /// </summary>
    public ProductData RollProduct()
    {
        // ⭐ 디버그 강제 당첨 (GameConfig.isDebugMode일 때만 유효)
        bool isDebugMode = GameConfig.Instance != null && GameConfig.Instance.isDebugMode;
        if (isDebugMode && debugForceProduct && !string.IsNullOrEmpty(debugForcedProductId))
        {
            ProductData forced = ProductDataTable.Instance.GetProductById(debugForcedProductId);
            if (forced != null)
            {
                Logger.LogWarning($"[LuckyBoxTable] ⚠️ 디버그 강제 당첨: {debugForcedProductId}");
                return forced;
            }
            else
            {
                Logger.LogError($"[LuckyBoxTable] 디버그 강제 당첨 productId '{debugForcedProductId}'를 찾을 수 없습니다. 정상 추첨으로 진행합니다.");
            }
        }

        if (entries.Count == 0)
        {
            Logger.LogError("[LuckyBoxTable] 등록된 상품이 없습니다.");
            return null;
        }

        int totalWeight = 0;
        foreach (var e in entries) totalWeight += e.weight;

        int randomValue = Random.Range(0, totalWeight);
        int current = 0;

        foreach (var e in entries)
        {
            current += e.weight;
            if (randomValue < current)
            {
                return ProductDataTable.Instance.GetProductById(e.productId);
            }
        }

        // 이론상 도달하지 않지만 안전망
        return ProductDataTable.Instance.GetProductById(entries[0].productId);
    }
}