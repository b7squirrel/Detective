using UnityEngine;

public class ShopManager : SingletonBehaviour<ShopManager>
{
    [Header("FX")]
    [SerializeField] private GemCollectFX gemCollectFX;
    
    /// <summary>
    /// 상품 구매 (FX 위치 포함)
    /// </summary>
    public void PurchaseProduct(string productId, RectTransform fxStartPoint = null)
    {
        // ⭐ 초기화 확인
        if (!GameInitializer.IsInitialized)
        {
            Logger.LogError("[ShopManager] 게임이 아직 초기화되지 않았습니다.");
            return;
        }
        
        // ⭐ ProductDataTable 확인
        if (ProductDataTable.Instance == null)
        {
            Logger.LogError("[ShopManager] ProductDataTable이 없습니다.");
            return;
        }
        
        var productData = ProductDataTable.Instance.GetProductById(productId);
        if (productData == null)
        {
            Logger.LogError($"[ShopManager] Product Data가 없습니다. Product ID: {productId}");
            return;
        }

        Logger.Log($"[ShopManager] 구매 시도: {productData.ProductName} (ID: {productId})");

        // ⭐ 구매 타입별 처리
        switch (productData.PurchaseType)
        {
            case PurchaseType.Cristal:
                if (PurchaseWithCristal(productData))
                {
                    GiveProductReward(productData, fxStartPoint);
                }
                break;
            
            case PurchaseType.Gold:
                if (PurchaseWithGold(productData))
                {
                    GiveProductReward(productData, fxStartPoint);
                }
                break;
            
            case PurchaseType.IAP:
                Logger.Log("[ShopManager] IAP 구매는 별도 처리 필요");
                // IAP 처리 로직
                break;
            
            case PurchaseType.Ad:
                Logger.Log("[ShopManager] 광고 시청 보상");
                // 광고 처리 후 GiveProductReward 호출
                break;
        }
    }
    
    /// <summary>
    /// 크리스탈로 구매
    /// </summary>
    bool PurchaseWithCristal(ProductData productData)
    {
        var playerDataManager = PlayerDataManager.Instance;
        if (playerDataManager == null)
        {
            Logger.LogError("[ShopManager] PlayerDataManager가 없습니다.");
            return false;
        }
        
        int currentCristal = playerDataManager.GetCurrentCristalNumber();

        if (currentCristal < productData.PurchaseCost)
        {
            Logger.Log($"[ShopManager] 크리스탈 부족: {currentCristal}/{productData.PurchaseCost}");
            ShowInsufficientCurrencyPopup("크리스탈");
            return false;
        }

        playerDataManager.SetCristalNumberAs(currentCristal - productData.PurchaseCost);
        Logger.Log($"[ShopManager] 크리스탈 차감: {productData.PurchaseCost} (남은 크리스탈: {playerDataManager.GetCurrentCristalNumber()})");
        return true;
    }
    
    /// <summary>
    /// 골드로 구매
    /// </summary>
    bool PurchaseWithGold(ProductData productData)
    {
        var playerDataManager = PlayerDataManager.Instance;
        if (playerDataManager == null)
        {
            Logger.LogError("[ShopManager] PlayerDataManager가 없습니다.");
            return false;
        }
        
        int currentGold = playerDataManager.GetCurrentCoinNumber();

        if (currentGold < productData.PurchaseCost)
        {
            Logger.Log($"[ShopManager] 골드 부족: {currentGold}/{productData.PurchaseCost}");
            ShowInsufficientCurrencyPopup("골드");
            return false;
        }

        playerDataManager.SetCoinNumberAs(currentGold - productData.PurchaseCost);
        Logger.Log($"[ShopManager] 골드 차감: {productData.PurchaseCost} (남은 골드: {playerDataManager.GetCurrentCoinNumber()})");
        return true;
    }

    /// <summary>
    /// 상품 보상 지급 + FX 재생
    /// </summary>
    void GiveProductReward(ProductData productData, RectTransform fxStartPoint)
    {
        var playerDataManager = PlayerDataManager.Instance;
        if (playerDataManager == null)
        {
            Logger.LogError("[ShopManager] PlayerDataManager가 없습니다.");
            return;
        }

        switch (productData.ProductType)
        {
            case ProductType.Gold:
                // ⭐ 먼저 실제 데이터에 골드 추가 (UI 업데이트 없이)
                int currentGold = playerDataManager.GetCurrentCoinNumber();
                playerDataManager.SetCoinNumberAsSilent(currentGold + productData.RewardGold);
                
                Logger.Log($"[ShopManager] 골드 지급: +{productData.RewardGold} (총: {playerDataManager.GetCurrentCoinNumber()})");
                
                // ⭐ FX 재생 (데이터는 이미 증가했으므로 FX만 재생)
                if (fxStartPoint != null)
                {
                    PlayGoldCollectFX(fxStartPoint, productData.RewardGold);
                }
                else
                {
                    // FX 위치 없으면 즉시 UI 업데이트
                    playerDataManager.SetCoinNumberAs(playerDataManager.GetCurrentCoinNumber());
                }
                
                ShowRewardPopup($"골드 {productData.RewardGold}개 획득!");
                break;

            case ProductType.Cristal:
                // ⭐ 크리스탈도 동일한 방식
                int currentCristal = playerDataManager.GetCurrentCristalNumber();
                playerDataManager.SetCristalNumberAsSilent(currentCristal + productData.RewardCristal);
                
                Logger.Log($"[ShopManager] 크리스탈 지급: +{productData.RewardCristal} (총: {playerDataManager.GetCurrentCristalNumber()})");
                
                if (fxStartPoint != null)
                {
                    PlayCristalCollectFX(fxStartPoint, productData.RewardCristal);
                }
                else
                {
                    playerDataManager.SetCristalNumberAs(playerDataManager.GetCurrentCristalNumber());
                }
                
                ShowRewardPopup($"크리스탈 {productData.RewardCristal}개 획득!");
                break;
                
            case ProductType.Pack:
                Logger.Log("[ShopManager] 팩 보상 처리");
                // 팩 오픈 로직 (GachaSystem 연동)
                break;
                
            case ProductType.Box:
                Logger.Log("[ShopManager] 상자 보상 처리");
                // 상자 오픈 로직 (GachaSystem 연동)
                break;
        }
    }
    
    /// <summary>
    /// 골드 수집 FX 재생
    /// </summary>
    void PlayGoldCollectFX(RectTransform startPoint, int amount)
    {
        if (gemCollectFX == null)
        {
            gemCollectFX = FindObjectOfType<GemCollectFX>();
        }
        
        if (gemCollectFX != null)
        {
            // isCoin = false (골드)
            gemCollectFX.PlayGemCollectFX(startPoint, amount, false);
        }
        else
        {
            Logger.LogWarning("[ShopManager] GemCollectFX를 찾을 수 없습니다.");
        }
    }
    
    /// <summary>
    /// 크리스탈 수집 FX 재생
    /// </summary>
    void PlayCristalCollectFX(RectTransform startPoint, int amount)
    {
        if (gemCollectFX == null)
        {
            gemCollectFX = FindObjectOfType<GemCollectFX>();
        }
        
        if (gemCollectFX != null)
        {
            // isCoin = true (크리스탈)
            gemCollectFX.PlayGemCollectFX(startPoint, amount, true);
        }
        else
        {
            Logger.LogWarning("[ShopManager] GemCollectFX를 찾을 수 없습니다.");
        }
    }
    
    /// <summary>
    /// 화폐 부족 팝업
    /// </summary>
    void ShowInsufficientCurrencyPopup(string currencyType)
    {
        // TODO: 부족 알림 팝업 표시
        Logger.Log($"[ShopManager] {currencyType}이(가) 부족합니다!");
    }
    
    /// <summary>
    /// 보상 획득 팝업
    /// </summary>
    void ShowRewardPopup(string message)
    {
        // TODO: 보상 팝업 표시
        Logger.Log($"[ShopManager] {message}");
    }
}