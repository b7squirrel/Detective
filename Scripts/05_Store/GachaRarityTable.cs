using System.Collections.Generic;
using UnityEngine;

public class GachaRarityTable : SingletonBehaviour<GachaRarityTable>
{
    [SerializeField] private TextAsset gachaRarityTableCSV;

    private Dictionary<string, Dictionary<int, int>> rarityTableData = new Dictionary<string, Dictionary<int, int>>();

    public class RarityData
    {
        public string GachaTableId;
        public string SlotType;
        public int Rarity;
        public int Probability;
    }

    protected override void Init()
    {
        base.Init();

        if (gachaRarityTableCSV != null)
        {
            ParseCSV(gachaRarityTableCSV.text);
        }
        else
        {
            Logger.LogError("[GachaRarityTable] CSV 파일이 할당되지 않았습니다.");
        }
    }

    private void ParseCSV(string csvText)
    {
        if (string.IsNullOrEmpty(csvText))
        {
            Logger.LogError("[GachaRarityTable] CSV 텍스트가 비어있습니다.");
            return;
        }

        string normalizedText = csvText
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

        string[] lines = normalizedText.Split('\n');
        Logger.Log($"[GachaRarityTable] 총 {lines.Length}개 라인 읽음");

        int parsedCount = 0;
        int skippedCount = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if (string.IsNullOrEmpty(line))
            {
                skippedCount++;
                continue;
            }

            string[] fields = line.Split(',');

            if (fields.Length < 4)
            {
                Logger.LogWarning($"[GachaRarityTable] 라인 {i}: 필드 부족 ({fields.Length}/4)");
                skippedCount++;
                continue;
            }

            try
            {
                RarityData data = new RarityData
                {
                    GachaTableId = GetField(fields, 0),
                    SlotType = GetField(fields, 1),
                    Rarity = GetRarityValue(GetField(fields, 2)),
                    Probability = ParseInt(GetField(fields, 3))
                };

                string key = $"{data.GachaTableId}_{data.SlotType}";

                if (!rarityTableData.ContainsKey(key))
                {
                    rarityTableData[key] = new Dictionary<int, int>();
                }

                rarityTableData[key][data.Rarity] = data.Probability;
                parsedCount++;

                if (i <= 5)
                {
                    Logger.Log($"[GachaRarityTable] Parsed: {data.GachaTableId}/{data.SlotType}/{GetRarityName(data.Rarity)} = {data.Probability}");
                }
            }
            catch (System.Exception e)
            {
                Logger.LogError($"[GachaRarityTable] 라인 {i} 파싱 오류: {e.Message}");
                skippedCount++;
            }
        }

        Logger.Log($"[GachaRarityTable] 파싱 완료: {parsedCount}개 성공, {skippedCount}개 건너뜀");
        Logger.Log($"[GachaRarityTable] 총 {rarityTableData.Count}개 테이블 로드됨");
    }

    public Dictionary<int, int> GetProbabilities(string gachaTableId, string slotType)
    {
        string key = $"{gachaTableId}_{slotType}";

        if (rarityTableData.TryGetValue(key, out var probs))
        {
            return probs;
        }

        return null;
    }

    /// <summary>
    /// 등급별 확률을 %로 변환하여 반환합니다. (등급 순으로 정렬됨)
    /// </summary>
    public List<(int rarity, float percent)> GetProbabilityPercentages(string gachaTableId, string slotType)
    {
        var probs = GetProbabilities(gachaTableId, slotType);
        if (probs == null || probs.Count == 0) return null;

        int totalWeight = 0;
        foreach (var weight in probs.Values)
            totalWeight += weight;

        if (totalWeight <= 0) return null;

        var result = new List<(int rarity, float percent)>();
        foreach (var kvp in probs)
        {
            float percent = (kvp.Value / (float)totalWeight) * 100f;
            result.Add((kvp.Key, percent));
        }

        result.Sort((a, b) => a.rarity.CompareTo(b.rarity));
        return result;
    }

    /// <summary>
    /// 해당 상품/슬롯 조합의 확률 테이블이 존재하는지 확인합니다.
    /// </summary>
    public bool HasSlotType(string gachaTableId, string slotType)
    {
        string key = $"{gachaTableId}_{slotType}";
        return rarityTableData.ContainsKey(key);
    }

    private string GetField(string[] fields, int index)
    {
        if (index < fields.Length)
        {
            return fields[index].Trim();
        }
        return string.Empty;
    }

    private int GetRarityValue(string rarityName)
    {
        switch (rarityName.ToLower())
        {
            case "common": return 0;
            case "rare": return 1;
            case "epic": return 2;
            case "legendary": return 3;
            case "mythic": return 4;
            default:
                Logger.LogWarning($"[GachaRarityTable] 알 수 없는 등급: {rarityName}");
                return 0;
        }
    }

    private int ParseInt(string value)
    {
        if (int.TryParse(value, out int result))
            return result;

        Logger.LogWarning($"[GachaRarityTable] 정수 파싱 실패: '{value}'");
        return 0;
    }

    private string GetRarityName(int rarity)
    {
        switch (rarity)
        {
            case 0: return "Common";
            case 1: return "Rare";
            case 2: return "Epic";
            case 3: return "Legendary";
            case 4: return "Mythic";
            default: return "Unknown";
        }
    }
}