using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SellPriceData
{
    public int Grade;
    public int DuckBasePrice;
    public int ItemBasePrice;
    public float EVO0Multi;
    public float EVO1Multi;
    public float EVO2Multi;
    public float RecoveryRate;
}

public class SellPriceDataReader
{
    List<SellPriceData> priceTable = new List<SellPriceData>();
    bool isLoaded = false;

    public void Load(TextAsset dataFile)
    {
        priceTable.Clear();

        if (dataFile == null)
        {
            Logger.LogError("[SellPriceDataReader] sellPriceData.txt가 연결되지 않았습니다.");
            return;
        }

        string normalized = dataFile.text
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

        if (normalized.EndsWith("\n"))
            normalized = normalized.Substring(0, normalized.Length - 1);

        string[] lines = normalized.Split('\n',
            System.StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < lines.Length; i++)
        {
            string[] col = lines[i].Split('\t');

            if (col.Length < 7)
            {
                Logger.LogWarning($"[SellPriceDataReader] 라인 {i}: 열 부족 ({col.Length}/7) - 건너뜀");
                continue;
            }

            SellPriceData row = new SellPriceData
            {
                Grade        = SafeParseInt  (col[0], "Grade"),
                DuckBasePrice= SafeParseInt  (col[1], "DuckBasePrice"),
                ItemBasePrice= SafeParseInt  (col[2], "ItemBasePrice"),
                EVO0Multi    = SafeParseFloat(col[3], "EVO0Multi"),
                EVO1Multi    = SafeParseFloat(col[4], "EVO1Multi"),
                EVO2Multi    = SafeParseFloat(col[5], "EVO2Multi"),
                RecoveryRate = SafeParseFloat(col[6], "RecoveryRate"),
            };

            priceTable.Add(row);
        }

        isLoaded = true;
        Logger.Log($"[SellPriceDataReader] 판매 가격 데이터 로드 완료: {priceTable.Count}개 등급");
    }

    /// <summary>
    /// Grade에 해당하는 가격 데이터 반환. 없으면 null.
    /// </summary>
    public SellPriceData GetPriceData(int grade)
    {
        if (!isLoaded)
        {
            Logger.LogError("[SellPriceDataReader] 데이터가 로드되지 않았습니다.");
            return null;
        }

        SellPriceData data = priceTable.Find(x => x.Grade == grade);

        if (data == null)
            Logger.LogError($"[SellPriceDataReader] Grade {grade}에 해당하는 데이터가 없습니다.");

        return data;
    }

    public bool IsLoaded => isLoaded;

    // ───── 파싱 헬퍼 ─────
    int SafeParseInt(string value, string fieldName)
    {
        if (int.TryParse(value.Trim(), out int result)) return result;
        Logger.LogWarning($"[SellPriceDataReader] {fieldName} 파싱 실패: '{value}' → 0");
        return 0;
    }

    float SafeParseFloat(string value, string fieldName)
    {
        if (float.TryParse(value.Trim(),
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out float result)) return result;
        Logger.LogWarning($"[SellPriceDataReader] {fieldName} 파싱 실패: '{value}' → 0");
        return 0f;
    }
}