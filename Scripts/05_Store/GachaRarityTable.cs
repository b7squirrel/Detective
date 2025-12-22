using System.Collections.Generic;
using UnityEngine;

public class GachaRarityTable : MonoBehaviour
{
    [SerializeField] private TextAsset gachaRarityTableCSV;
    
    // ⭐ int로 변경
    private Dictionary<string, Dictionary<int, int>> rarityTableData = new Dictionary<string, Dictionary<int, int>>();

    public class RarityData
    {
        public string GachaTableId;
        public string SlotType;
        public int Rarity;
        public int Probability;  // ⭐ float → int
    }

    void Awake()
    {
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

        // 첫 줄(헤더) 건너뛰기
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
                    Probability = ParseInt(GetField(fields, 3))  // ⭐ int로 파싱
                };

                // 테이블 키 생성
                string key = $"{data.GachaTableId}_{data.SlotType}";

                // 딕셔너리에 추가
                if (!rarityTableData.ContainsKey(key))
                {
                    rarityTableData[key] = new Dictionary<int, int>();
                }

                rarityTableData[key][data.Rarity] = data.Probability;
                parsedCount++;

                // ⭐ 디버깅용 로그 (처음 몇 개만)
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

    // ⭐ 반환 타입을 Dictionary<int, int>로 변경
    public Dictionary<int, int> GetProbabilities(string gachaTableId, string slotType)
    {
        string key = $"{gachaTableId}_{slotType}";

        if (rarityTableData.TryGetValue(key, out var probs))
        {
            return probs;
        }

        return null;
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