using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GachaRarityTable : MonoBehaviour
{
    [SerializeField] private string csvFileName = "gachaRarityTable.csv";
    [SerializeField] private string csvFolderPath = "DataTable";

    // Key: "gachaTableId/slotType" (예: "single_duck/Normal")
    // Value: Dictionary<등급, 확률>
    private Dictionary<string, Dictionary<int, float>> rarityTableData;

    void Awake()
    {
        LoadRarityTable();
    }

    void LoadRarityTable()
    {
        rarityTableData = new Dictionary<string, Dictionary<int, float>>();

        string resourcePath = $"{csvFolderPath}/{Path.GetFileNameWithoutExtension(csvFileName)}";
        Logger.Log($"[GachaRarityTable] CSV 로드 시도: Resources/{resourcePath}");

        TextAsset csvFile = Resources.Load<TextAsset>(resourcePath);
        
        if (csvFile == null)
        {
            Logger.LogError($"[GachaRarityTable] CSV 파일을 찾을 수 없습니다: Resources/{resourcePath}");
            Logger.LogError("[GachaRarityTable] Resources/DataTable/gachaRarityTable.csv 파일을 생성하세요!");
            return;
        }

        ParseCSV(csvFile.text);
        Logger.Log($"[GachaRarityTable] 확률 테이블 로드 완료");
    }

    void ParseCSV(string csvText)
    {
        if (string.IsNullOrEmpty(csvText))
        {
            Logger.LogError("[GachaRarityTable] CSV 텍스트가 비어있습니다.");
            return;
        }

        string normalizedText = csvText.Replace("\r\n", "\n").Replace("\r", "\n");
        string[] lines = normalizedText.Split('\n');
        
        Logger.Log($"[GachaRarityTable] 총 {lines.Length}개 라인 읽음");

        // 첫 줄(헤더) 건너뛰기
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            
            if (string.IsNullOrEmpty(line))
                continue;

            string[] fields = line.Split(',');
            
            if (fields.Length < 4)
            {
                Logger.LogWarning($"[GachaRarityTable] 라인 {i}: 필드 부족 ({fields.Length}/4)");
                continue;
            }

            try
            {
                string gachaTableId = GetField(fields, 0);
                string slotType = GetField(fields, 1);
                string rarityStr = GetField(fields, 2);
                float probability = ParseFloat(GetField(fields, 3));

                // 등급 문자열을 숫자로 변환
                int rarity = RarityStringToInt(rarityStr);

                // Key 생성: "gachaTableId/slotType"
                string key = $"{gachaTableId}/{slotType}";

                if (!rarityTableData.ContainsKey(key))
                {
                    rarityTableData[key] = new Dictionary<int, float>();
                }

                rarityTableData[key][rarity] = probability;

                if (i <= 5) // 처음 몇 개만 로그
                {
                    Logger.Log($"[GachaRarityTable] {key} → {rarityStr}({rarity}): {probability}%");
                }
            }
            catch (System.Exception e)
            {
                Logger.LogError($"[GachaRarityTable] 라인 {i} 파싱 오류: {e.Message}");
            }
        }

        Logger.Log($"[GachaRarityTable] {rarityTableData.Count}개 테이블 로드 완료");
    }

    /// <summary>
    /// 확률 테이블 가져오기
    /// </summary>
    public Dictionary<int, float> GetProbabilities(string gachaTableId, string slotType)
    {
        string key = $"{gachaTableId}/{slotType}";
        
        if (rarityTableData.ContainsKey(key))
        {
            return rarityTableData[key];
        }

        Logger.LogWarning($"[GachaRarityTable] 확률 테이블을 찾을 수 없습니다: {key}");
        return null;
    }

    /// <summary>
    /// 등급 문자열을 숫자로 변환
    /// </summary>
    private int RarityStringToInt(string rarity)
    {
        switch (rarity.ToLower())
        {
            case "common": return 0;
            case "rare": return 1;
            case "epic": return 2;
            case "legendary": return 3;
            case "mythic": return 4;
            default:
                Logger.LogWarning($"[GachaRarityTable] 알 수 없는 등급: {rarity}, Common으로 처리");
                return 0;
        }
    }

    private string GetField(string[] fields, int index)
    {
        if (index >= 0 && index < fields.Length)
        {
            return fields[index].Trim();
        }
        return "";
    }

    private float ParseFloat(string value)
    {
        if (float.TryParse(value, out float result))
            return result;
        return 0f;
    }
}