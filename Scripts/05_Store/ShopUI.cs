using System.Collections;
using UnityEngine;
using System.Linq;

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

    [Header("시간 기반 상자 버튼")]
    [SerializeField] TimeBoxButton timeBoxButton;

    // ⭐ 팩 버튼 추가
    [Header("팩 구매 버튼")]
    [SerializeField] PackBuyButton[] packBuyButtons;

    // ⭐ 상자 버튼 추가
    [Header("상자 구매 버튼")]
    [SerializeField] ChestBuyButton[] chestBuyButtons;

    // 에너지 버튼
    [Header("번개 크리스탈 구매 버튼")]
    [SerializeField] EnergyBuyButton[] energyBuyButtons;

    [Header("번개 광고 버튼")]
    [SerializeField] EnergyAdButton energyAdButton;

    private bool isInitialized = false;

    void Start()
    {
        StartCoroutine(InitShopUI());
    }

    #region shop ui 초기화
    IEnumerator InitShopUI()
    {
        Logger.Log("[ShopUI] 초기화 시작");

        yield return new WaitUntil(() => GameInitializer.IsInitialized);
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
        SetTimeBoxProduct();
        SetPackProducts();    // ⭐ 추가
        SetBoxProducts();     // ⭐ 추가
        SetEnergyProducts(); // ← 추가
    }
    #endregion

    #region 골드 상품 버튼 설정
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

    #region 시간 기반 상자 버튼 설정
    void SetTimeBoxProduct()
    {
        if (ProductDataTable.Instance == null)
        {
            Logger.LogError("[ShopUI] ProductDataTable.Instance가 null입니다.");
            return;
        }

        var productData = ProductDataTable.Instance.GetProductById("chest_005");

        if (productData == null)
        {
            Logger.LogError($"[ShopUI] chest_daily를 찾을 수 없습니다.");
            return;
        }

        if (timeBoxButton == null)
        {
            Logger.LogError("[ShopUI] timeBoxButton이 null입니다.");
            return;
        }

        Logger.Log($"[ShopUI] TimeBoxButton.SetInfo 호출: {productData.ProductId}");
        timeBoxButton.SetInfo(productData);
        Logger.Log("[ShopUI] TimeBoxButton.SetInfo 호출 완료");
    }
    #endregion

    #region 팩 상품 버튼 설정
    /// <summary>
    /// 팩 상품 버튼 설정 (Starter Pack, Pro Pack)
    /// </summary>
    void SetPackProducts()
    {
        if (ProductDataTable.Instance == null)
        {
            Logger.LogError("[ShopUI] ProductDataTable.Instance가 null입니다.");
            return;
        }

        var productData = ProductDataTable.Instance.GetProductsByType(ProductType.Pack);

        if (productData == null || productData.Count == 0)
        {
            Logger.LogWarning($"[ShopUI] Pack Product Data가 없습니다.");
            return;
        }

        if (packBuyButtons == null || packBuyButtons.Length == 0)
        {
            Logger.LogWarning("[ShopUI] 인스펙터에 팩 구매 버튼들을 드래그해 주세요");
            return;
        }

        if (packBuyButtons.Length != productData.Count)
        {
            Logger.LogWarning($"[ShopUI] 팩 버튼 개수({packBuyButtons.Length})와 상품 개수({productData.Count})가 다릅니다.");
        }

        int count = Mathf.Min(packBuyButtons.Length, productData.Count);
        for (int i = 0; i < count; i++)
        {
            if (packBuyButtons[i] != null)
            {
                packBuyButtons[i].SetInfo(productData[i]);
                Logger.Log($"[ShopUI] 팩 버튼 {i}: {productData[i].ProductId}");
            }
            else
            {
                Logger.LogWarning($"[ShopUI] 팩 버튼 {i}가 null입니다.");
            }
        }

        Logger.Log($"[ShopUI] 팩 상품 {count}개 설정 완료");
    }
    #endregion

    #region 상자 상품 버튼 설정
    /// <summary>
    /// 상자 상품 버튼 설정 (Duck Box, Item Box)
    /// </summary>
    void SetBoxProducts()
    {
        if (ProductDataTable.Instance == null)
        {
            Logger.LogError("[ShopUI] ProductDataTable.Instance가 null입니다.");
            return;
        }

        var productData = ProductDataTable.Instance.GetProductsByType(ProductType.Box);

        if (productData == null || productData.Count == 0)
        {
            Logger.LogWarning($"[ShopUI] Box Product Data가 없습니다.");
            return;
        }

        if (chestBuyButtons == null || chestBuyButtons.Length == 0)
        {
            Logger.LogWarning("[ShopUI] 인스펙터에 상자 구매 버튼들을 드래그해 주세요");
            return;
        }

        if (chestBuyButtons.Length != productData.Count)
        {
            Logger.LogWarning($"[ShopUI] 상자 버튼 개수({chestBuyButtons.Length})와 상품 개수({productData.Count})가 다릅니다.");
        }

        int count = Mathf.Min(chestBuyButtons.Length, productData.Count);
        for (int i = 0; i < count; i++)
        {
            if (chestBuyButtons[i] != null)
            {
                chestBuyButtons[i].SetInfo(productData[i]);
                Logger.Log($"[ShopUI] 상자 버튼 {i}: {productData[i].ProductId}");
            }
            else
            {
                Logger.LogWarning($"[ShopUI] 상자 버튼 {i}가 null입니다.");
            }
        }

        Logger.Log($"[ShopUI] 상자 상품 {count}개 설정 완료");
    }
    #endregion

    #region 번개 상품 버튼 설정
    void SetEnergyProducts()
    {
        if (ProductDataTable.Instance == null)
        {
            Logger.LogError("[ShopUI] ProductDataTable.Instance가 null입니다.");
            return;
        }

        var allEnergyProducts = ProductDataTable.Instance.GetProductsByType(ProductType.Energy);

        if (allEnergyProducts == null || allEnergyProducts.Count == 0)
        {
            Logger.LogWarning("[ShopUI] Energy Product Data가 없습니다.");
            return;
        }

        // 크리스탈 구매 상품만 필터링 (energy_001, energy_002)
        var cristalEnergyProducts = allEnergyProducts
            .Where(p => p.PurchaseType == PurchaseType.Cristal)
            .ToList();

        if (energyBuyButtons != null && energyBuyButtons.Length > 0)
        {
            int count = Mathf.Min(energyBuyButtons.Length, cristalEnergyProducts.Count);
            for (int i = 0; i < count; i++)
            {
                if (energyBuyButtons[i] != null)
                {
                    energyBuyButtons[i].SetInfo(cristalEnergyProducts[i]);
                    Logger.Log($"[ShopUI] 번개 구매 버튼 {i}: {cristalEnergyProducts[i].ProductId}");
                }
            }
        }
        else
        {
            Logger.LogWarning("[ShopUI] 인스펙터에 번개 구매 버튼들을 드래그해 주세요");
        }

        // 광고 상품 (energy_ad) 연결
        var adEnergyProduct = allEnergyProducts.FirstOrDefault(p => p.PurchaseType == PurchaseType.Ad);
        if (adEnergyProduct != null && energyAdButton != null)
        {
            energyAdButton.SetInfo(adEnergyProduct);
            Logger.Log($"[ShopUI] 번개 광고 버튼: {adEnergyProduct.ProductId}");
        }
        else if (energyAdButton == null)
        {
            Logger.LogWarning("[ShopUI] 인스펙터에 번개 광고 버튼을 드래그해 주세요");
        }

        Logger.Log($"[ShopUI] 번개 상품 설정 완료");
    }
    #endregion

    #region 외부에서 초기화 확인
    public bool IsInitialized() => isInitialized;
    #endregion
}