using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ProductDataTable : SingletonBehaviour<ProductDataTable>
{
    [SerializeField] private string csvFileName = "productData.csv";
    [SerializeField] private string csvFolderPath = "DataTable"; // Resources 폴더 내 경로
    
    private List<ProductData> productDataList = new List<ProductData>();
    private Dictionary<string, ProductData> productDataDict = new Dictionary<string, ProductData>();

    // ⭐ 데이터 로드 완료 플래그
    public static bool IsDataLoaded { get; private set; } = false;

    // ⭐ SingletonBehaviour의 Awake가 virtual이 아니면 Start 사용
    protected override void Init()
    {
        base.Init(); // ⭐ 필수!
        
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

        // ⭐ Resources 경로 확인
        string resourcePath = $"{csvFolderPath}/{Path.GetFileNameWithoutExtension(csvFileName)}";
        Logger.Log($"[ProductDataTable] CSV 로드 시도: Resources/{resourcePath}");

        // Resources 폴더에서 로드
        TextAsset csvFile = Resources.Load<TextAsset>(resourcePath);
        
        if (csvFile == null)
        {
            Logger.LogError($"[ProductDataTable] CSV 파일을 찾을 수 없습니다: Resources/{resourcePath}");
            Logger.LogError("[ProductDataTable] 확인 사항:");
            Logger.LogError("  1. 파일이 Assets/Resources/DataTable/ 폴더에 있는지");
            Logger.LogError("  2. 파일 이름이 'productData.csv'인지");
            Logger.LogError("  3. Unity에서 파일을 다시 임포트했는지 (우클릭 > Reimport)");
            return;
        }

        Logger.Log($"[ProductDataTable] CSV 파일 로드 성공: {csvFile.text.Length} 바이트");
        Logger.Log($"[ProductDataTable] 파일 이름: {csvFile.name}");
        Logger.Log($"[ProductDataTable] 첫 100자: {csvFile.text.Substring(0, Mathf.Min(100, csvFile.text.Length))}");
        
        // ⭐ CSV 파일인지 확인
        if (csvFile.text.Contains("public class") || csvFile.text.Contains("using UnityEngine"))
        {
            Logger.LogError("[ProductDataTable] ❌ 잘못된 파일! C# 스크립트를 로드했습니다.");
            Logger.LogError("[ProductDataTable] Resources/DataTable/ 폴더에서 .cs 파일을 제거하세요!");
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

        // ⭐ 크로스 플랫폼 줄바꿈 처리
        string normalizedText = csvText
            .Replace("\r\n", "\n")  // Windows → Unix
            .Replace("\r", "\n");   // Old Mac → Unix
        
        string[] lines = normalizedText.Split('\n');
        Logger.Log($"[ProductDataTable] 총 {lines.Length}개 라인 읽음");
        
        int parsedCount = 0;
        int skippedCount = 0;
        
        // 첫 줄(헤더) 건너뛰기
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            
            // 빈 줄 건너뛰기
            if (string.IsNullOrEmpty(line))
            {
                skippedCount++;
                continue;
            }

            // ⭐ 쉼표로 분리 (빈 필드 유지)
            string[] fields = line.Split(',');
            
            // ⭐ 디버깅: 첫 3개 라인만 출력
            if (i <= 3)
            {
                Logger.Log($"[ProductDataTable] 라인 {i}: {fields.Length}개 필드");
                for (int j = 0; j < fields.Length; j++)
                {
                    Logger.Log($"  [{j}] = '{fields[j]}'");
                }
            }
            
            // 최소 8개 필드 필요
            if (fields.Length < 8)
            {
                Logger.LogWarning($"[ProductDataTable] 라인 {i}: 필드 부족 ({fields.Length}/8) - '{line}'");
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
                    RewardItemId = GetField(fields, 7)
                };

                productDataList.Add(data);
                productDataDict[data.ProductId] = data;
                parsedCount++;
            }
            catch (System.Exception e)
            {
                Logger.LogError($"[ProductDataTable] 라인 {i} 파싱 오류: {e.Message}");
                skippedCount++;
            }
        }
        
        Logger.Log($"[ProductDataTable] 파싱 완료: {parsedCount}개 성공, {skippedCount}개 건너뜀");
    }
    
    // ⭐ 안전한 필드 접근
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

    // 데이터 접근 메서드
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