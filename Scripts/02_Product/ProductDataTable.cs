using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ProductDataTable : SingletonBehaviour<ProductDataTable>
{
    [SerializeField] private string csvFileName = "productData.csv";
    [SerializeField] private string csvFolderPath = "DataTable";

    private List<ProductData> productDataList = new List<ProductData>();
    private Dictionary<string, ProductData> productDataDict = new Dictionary<string, ProductData>();

    public static bool IsDataLoaded { get; private set; } = false;

    protected override void Init()
    {
        base.Init();

        LoadProductData();
        IsDataLoaded = true;
        Logger.Log($"[ProductDataTable] 데이터 로드 완료");
    }

    private void OnApplicationQuit()
    {
        IsDataLoaded = false;
    }

    public void LoadProductData()
    {
        productDataList.Clear();
        productDataDict.Clear();

        string resourcePath = $"{csvFolderPath}/{Path.GetFileNameWithoutExtension(csvFileName)}";
        Logger.Log($"[ProductDataTable] CSV 로드 시도: Resources/{resourcePath}");

        TextAsset csvFile = Resources.Load<TextAsset>(resourcePath);

        if (csvFile == null)
        {
            Logger.LogError($"[ProductDataTable] CSV 파일을 찾을 수 없습니다: Resources/{resourcePath}");
            return;
        }

        Logger.Log($"[ProductDataTable] CSV 파일 로드 성공: {csvFile.text.Length} 바이트");

        if (csvFile.text.Contains("public class") || csvFile.text.Contains("using UnityEngine"))
        {
            Logger.LogError("[ProductDataTable] ❌ 잘못된 파일! C# 스크립트를 로드했습니다.");
            return;
        }

        ParseCSV(csvFile.text);
        Logger.Log($"[ProductDataTable] Product 데이터 {productDataList.Count}개 로드 완료");
    }

    private void ParseCSV(string csvText)
    {
        if (string.IsNullOrEmpty(csvText))
        {
            Logger.LogError("[ProductDataTable] CSV 텍스트가 비어있습니다.");
            return;
        }

        string normalizedText = csvText
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

        string[] lines = normalizedText.Split('\n');
        Logger.Log($"[ProductDataTable] 총 {lines.Length}개 라인 읽음");

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

            if (i <= 3)
            {
                Logger.Log($"[ProductDataTable] 라인 {i}: {fields.Length}개 필드");
            }

            // ⭐ 최소 12개 필드 필요 (11개 → 12개로 변경)
            if (fields.Length < 12)
            {
                Logger.LogWarning($"[ProductDataTable] 라인 {i}: 필드 부족 ({fields.Length}/12) - '{line}'");
                skippedCount++;
                continue;
            }

            try
            {
                ProductData data = new ProductData
                {
                    ProductId = GetField(fields, 0),
                    ProductType = ParseEnum<ProductType>(GetField(fields, 1)),
                    ProductName = GetField(fields, 2),
                    PurchaseType = ParseEnum<PurchaseType>(GetField(fields, 3)),
                    PurchaseCost = ParseInt(GetField(fields, 4)),
                    RewardCristal = ParseInt(GetField(fields, 5)),
                    RewardGold = ParseInt(GetField(fields, 6)),
                    RewardItemId = GetField(fields, 7),
                    GachaTableId = GetField(fields, 8),
                    DrawCount = ParseInt(GetField(fields, 9)),           // ⭐ 올바른 인덱스
                    GuaranteedCount = ParseInt(GetField(fields, 10)),    // ⭐ 올바른 인덱스
                    GuaranteedRarity = GetField(fields, 11)              // ⭐ 올바른 인덱스
                };

                productDataList.Add(data);
                productDataDict[data.ProductId] = data;
                parsedCount++;

                // ⭐ 처음 몇 개 데이터 확인 로그
                if (i <= 3)
                {
                    Logger.Log($"[ProductDataTable] {data.ProductId}: gacha={data.GachaTableId}, draw={data.DrawCount}, guaranteed={data.GuaranteedCount}");
                }
            }
            catch (System.Exception e)
            {
                Logger.LogError($"[ProductDataTable] 라인 {i} 파싱 오류: {e.Message}");
                skippedCount++;
            }
        }

        Logger.Log($"[ProductDataTable] 파싱 완료: {parsedCount}개 성공, {skippedCount}개 건너뜀");
    }

    private string GetField(string[] fields, int index)
    {
        if (index >= 0 && index < fields.Length)
        {
            return fields[index].Trim();
        }
        return "";
    }

    private T ParseEnum<T>(string value) where T : struct
    {
        if (System.Enum.TryParse(value, true, out T result))
            return result;

        Logger.LogWarning($"Enum 파싱 실패: {value}, 기본값 사용");
        return default(T);
    }

    private int ParseInt(string value)
    {
        if (int.TryParse(value, out int result))
            return result;
        return 0;
    }

    public ProductData GetProductById(string productId)
    {
        productDataDict.TryGetValue(productId, out ProductData data);
        return data;
    }

    public List<ProductData> GetProductsByType(ProductType type)
    {
        return productDataList.Where(p => p.ProductType == type).ToList();
    }

    public List<ProductData> GetAllProducts()
    {
        return new List<ProductData>(productDataList);
    }
}