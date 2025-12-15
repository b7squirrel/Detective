using System.Collections;
using UnityEngine;

/// <summary>
/// 골드, 크리스탈 버튼에 product Id 부여
/// 팩, 상자 버튼에 product Id 부여
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("골드 구매 버튼")]
    [SerializeField] CoinBuyButton[] coinBuyButtons;
    
    [Header("크리스탈 구매 버튼")]
    [SerializeField] GemBuyButton[] gemBuyButtons;
    
    private bool isInitialized = false;

    void Start()
    {
        StartCoroutine(InitShopUI());
    }
    
    #region shop ui 초기화
    // ⭐ GameInitializer 대기 후 초기화
    IEnumerator InitShopUI()
    {
        Logger.Log("[ShopUI] 초기화 시작");
        
        // GameInitializer가 완료될 때까지 대기
        yield return new WaitUntil(() => GameInitializer.IsInitialized);
        
        // ProductDataTable이 로드될 때까지 대기 (이중 안전장치)
        yield return new WaitUntil(() => ProductDataTable.IsDataLoaded);
        
        Logger.Log("[ShopUI] 데이터 로드 완료, UI 설정 시작");
        
        SetShopUIInfo();
        isInitialized = true;
        
        Logger.Log("[ShopUI] 초기화 완료");
    }
    #endregion

    #region Shop Setup
    public void SetShopUIInfo()
    {
        SetGoldProducts();
        SetCristalProducts();
        // 필요시 다른 상품 타입도 추가
        // SetPackProducts();
        // SetBoxProducts();
    }
    #endregion

    #region 골드 상품 버튼 설정
    /// <summary>
    /// 골드 상품 버튼 설정
    /// </summary>
    void SetGoldProducts()
    {
        if (ProductDataTable.Instance == null)
        {
            Logger.LogError("[ShopUI] ProductDataTable.Instance가 null입니다.");
            return;
        }
        
        var productData = ProductDataTable.Instance.GetProductsByType(ProductType.Gold);
        
        if (productData == null || productData.Count == 0)
        {
            Logger.LogError($"[ShopUI] Gold Product Data가 없습니다.");
            return;
        }
        
        if (coinBuyButtons == null || coinBuyButtons.Length == 0)
        {
            Logger.LogError("[ShopUI] 인스펙터에 골드 구매 버튼들을 드래그해 주세요");
            return;
        }
        
        if (coinBuyButtons.Length != productData.Count)
        {
            Logger.LogWarning($"[ShopUI] 골드 버튼 개수({coinBuyButtons.Length})와 상품 개수({productData.Count})가 다릅니다.");
        }

        // Product Id 부여
        int count = Mathf.Min(coinBuyButtons.Length, productData.Count);
        for (int i = 0; i < count; i++)
        {
            if (coinBuyButtons[i] != null)
            {
                coinBuyButtons[i].SetInfo(productData[i]);
                Logger.Log($"[ShopUI] 골드 버튼 {i}: {productData[i].ProductId}");
            }
            else
            {
                Logger.LogWarning($"[ShopUI] 골드 버튼 {i}가 null입니다.");
            }
        }
        
        Logger.Log($"[ShopUI] 골드 상품 {count}개 설정 완료");
    }
    #endregion

    #region 크리스탈 상품 버튼 설정
    /// <summary>
    /// 크리스탈 상품 버튼 설정
    /// </summary>
    void SetCristalProducts()
    {
        if (ProductDataTable.Instance == null)
        {
            Logger.LogError("[ShopUI] ProductDataTable.Instance가 null입니다.");
            return;
        }
        
        var productData = ProductDataTable.Instance.GetProductsByType(ProductType.Cristal);
        
        if (productData == null || productData.Count == 0)
        {
            Logger.LogError($"[ShopUI] Cristal Product Data가 없습니다.");
            return;
        }
        
        if (gemBuyButtons == null || gemBuyButtons.Length == 0)
        {
            Logger.LogError("[ShopUI] 인스펙터에 크리스탈 구매 버튼들을 드래그해 주세요");
            return;
        }
        
        if (gemBuyButtons.Length != productData.Count)
        {
            Logger.LogWarning($"[ShopUI] 크리스탈 버튼 개수({gemBuyButtons.Length})와 상품 개수({productData.Count})가 다릅니다.");
        }

        // Product Id 부여
        int count = Mathf.Min(gemBuyButtons.Length, productData.Count);
        for (int i = 0; i < count; i++)
        {
            if (gemBuyButtons[i] != null)
            {
                gemBuyButtons[i].SetInfo(productData[i]);
                Logger.Log($"[ShopUI] 크리스탈 버튼 {i}: {productData[i].ProductId}");
            }
            else
            {
                Logger.LogWarning($"[ShopUI] 크리스탈 버튼 {i}가 null입니다.");
            }
        }
        
        Logger.Log($"[ShopUI] 크리스탈 상품 {count}개 설정 완료");
    }
    #endregion
    
    #region 외부에서 초기화 확인
    /// <summary>
    /// 외부에서 초기화 완료 확인용
    /// </summary>
    public bool IsInitialized() => isInitialized;
    #endregion
}