using System.Collections;
using UnityEngine;

public class ShopManager : SingletonBehaviour<ShopManager>
{
    [Header("FX")]
    [SerializeField] private GemCollectFX gemCollectFX;

    [Header("테스트 설정")]
    [SerializeField] private bool enableIAPTestMode = true;

    protected override void Init()
    {
        base.Init();
    }

    /// <summary>
    /// 상품 구매 (FX 위치 포함)
    /// </summary>
    public void PurchaseProduct(string productId, RectTransform fxStartPoint = null)
    {
        if (!GameInitializer.IsInitialized)
        {
            Logger.LogError("[ShopManager] 게임이 아직 초기화되지 않았습니다.");
            return;
        }

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
                if (enableIAPTestMode)
                {
                    Logger.Log($"[ShopManager] [테스트 모드] IAP 구매: {productData.ProductName}");
                    GiveProductReward(productData, fxStartPoint);
                }
                else
                {
                    Logger.Log("[ShopManager] IAP 구매는 실제 결제 연동이 필요합니다.");
                }
                break;

            case PurchaseType.Ad:
                // ⭐ 광고 시청 처리
                PurchaseWithAd(productData, fxStartPoint);
                break;
        }
    }

    /// <summary>
    /// 광고 시청으로 구매
    /// </summary>
    // ShopManager.cs의 PurchaseWithAd 수정

    async void PurchaseWithAd(ProductData productData, RectTransform fxStartPoint)
    {
        Logger.Log($"[ShopManager] 광고 구매 시작: {productData.ProductName}");

        if (productData.ProductType == ProductType.Box)
        {
            if (TimeBasedBoxManager.Instance == null)
            {
                Logger.LogError("[ShopManager] TimeBasedBoxManager가 없습니다.");
                return;
            }

            // ⭐ 서버 시간으로 쿨다운 확인
            bool canClaim = await TimeBasedBoxManager.Instance.CanClaimBoxAsync();

            if (!canClaim)
            {
                string remainingTime = TimeBasedBoxManager.Instance.GetRemainingTimeFormatted();
                Logger.Log($"[ShopManager] 상자 쿨다운 중: {remainingTime} 남음");
                ShowCooldownPopup(remainingTime);
                return;
            }
        }

        if (AdsManager.Instance == null)
        {
            Logger.LogError("[ShopManager] AdsManager가 없습니다.");
            return;
        }

        Logger.Log($"[ShopManager] 광고 시청 시작: {productData.ProductName}");

        AdsManager.Instance.ShowDailyFreeGemRewardedAd(async () =>
        {
            Logger.Log($"[ShopManager] 광고 시청 완료!");

            if (productData.ProductType == ProductType.Box)
            {
                // ⭐ 서버 시간으로 기록
                await TimeBasedBoxManager.Instance.OnBoxClaimedAsync();
            }

            StartCoroutine(GiveProductRewardCo(productData, fxStartPoint));
        });
    }

    

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
                int currentGold = playerDataManager.GetCurrentCoinNumber();
                playerDataManager.SetCoinNumberAsSilent(currentGold + productData.RewardGold);

                Logger.Log($"[ShopManager] 골드 지급: +{productData.RewardGold} (총: {playerDataManager.GetCurrentCoinNumber()})");

                if (fxStartPoint != null)
                {
                    PlayGoldCollectFX(fxStartPoint, productData.RewardGold);
                }
                else
                {
                    playerDataManager.SetCoinNumberAs(playerDataManager.GetCurrentCoinNumber());
                }

                ShowRewardPopup($"골드 {productData.RewardGold}개 획득!");
                break;

            case ProductType.Cristal:
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
                // ⭐ GachaSystem 연동 (추후 구현)
                // GachaSystem.Instance.OpenPack(productData.RewardItemId);
                break;

            case ProductType.Box:
                Logger.Log($"[ShopManager] 상자 보상 처리: {productData.RewardItemId}");
                // ⭐ 상자 열기 (랜덤 보상)
                OpenBox(productData.RewardItemId, fxStartPoint);
                break;
        }
    }
    IEnumerator GiveProductRewardCo(ProductData productData, RectTransform fxStartPoint)
    {
        yield return null;
        ShopManager.Instance.GiveProductReward(productData, fxStartPoint);
    }

    /// <summary>
    /// 상자 열기 (랜덤 보상)
    /// </summary>
    void OpenBox(string boxId, RectTransform fxStartPoint)
    {
        // ⭐ 추후 BoxRewardTable을 만들어서 관리하는 것이 좋습니다
        // 지금은 간단하게 랜덤으로 구현

        int random = UnityEngine.Random.Range(0, 100);

        if (random < 40) // 40% 골드
        {
            int goldAmount = UnityEngine.Random.Range(1000, 5000);
            var playerDataManager = PlayerDataManager.Instance;
            int currentGold = playerDataManager.GetCurrentCoinNumber();
            playerDataManager.SetCoinNumberAsSilent(currentGold + goldAmount);

            if (fxStartPoint != null)
            {
                PlayGoldCollectFX(fxStartPoint, goldAmount);
            }

            ShowRewardPopup($"상자에서 골드 {goldAmount}개 획득!");
            Logger.Log($"[ShopManager] 상자 보상: 골드 {goldAmount}");
        }
        else if (random < 70) // 30% 크리스탈
        {
            int cristalAmount = UnityEngine.Random.Range(10, 50);
            var playerDataManager = PlayerDataManager.Instance;
            int currentCristal = playerDataManager.GetCurrentCristalNumber();
            playerDataManager.SetCristalNumberAsSilent(currentCristal + cristalAmount);

            if (fxStartPoint != null)
            {
                PlayCristalCollectFX(fxStartPoint, cristalAmount);
            }

            ShowRewardPopup($"상자에서 크리스탈 {cristalAmount}개 획득!");
            Logger.Log($"[ShopManager] 상자 보상: 크리스탈 {cristalAmount}");
        }
        else if (random < 90) // 20% 오리 카드
        {
            // ⭐ CardDataManager 연동 필요
            Logger.Log($"[ShopManager] 상자 보상: 오리 카드");
            ShowRewardPopup($"상자에서 오리 카드 획득!");
        }
        else // 10% 아이템 카드
        {
            // ⭐ CardDataManager 연동 필요
            Logger.Log($"[ShopManager] 상자 보상: 아이템 카드");
            ShowRewardPopup($"상자에서 아이템 카드 획득!");
        }
    }

    void PlayGoldCollectFX(RectTransform startPoint, int amount)
    {
        if (gemCollectFX == null)
        {
            gemCollectFX = FindObjectOfType<GemCollectFX>();
        }

        if (gemCollectFX != null)
        {
            gemCollectFX.PlayGemCollectFX(startPoint, amount, false);
        }
        else
        {
            Logger.LogWarning("[ShopManager] GemCollectFX를 찾을 수 없습니다.");
        }
    }

    void PlayCristalCollectFX(RectTransform startPoint, int amount)
    {
        if (gemCollectFX == null)
        {
            gemCollectFX = FindObjectOfType<GemCollectFX>();
        }

        if (gemCollectFX != null)
        {
            gemCollectFX.PlayGemCollectFX(startPoint, amount, true);
        }
        else
        {
            Logger.LogWarning("[ShopManager] GemCollectFX를 찾을 수 없습니다.");
        }
    }

    void ShowInsufficientCurrencyPopup(string currencyType)
    {
        // TODO: 부족 알림 팝업 표시
        Logger.Log($"[ShopManager] {currencyType}이(가) 부족합니다!");
    }

    /// <summary>
    /// 쿨다운 팝업
    /// </summary>
    void ShowCooldownPopup(string remainingTime)
    {
        // TODO: 쿨다운 팝업 표시
        Logger.Log($"[ShopManager] 상자를 열 수 없습니다. {remainingTime} 후 다시 시도하세요.");
    }

    void ShowRewardPopup(string message)
    {
        // TODO: 보상 팝업 표시
        Logger.Log($"[ShopManager] {message}");
    }
}