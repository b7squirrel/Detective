using System.Collections;
using UnityEngine;

public class ShopManager : SingletonBehaviour<ShopManager>
{
    [Header("FX")]
    [SerializeField] private GemCollectFX gemCollectFX;

    [Header("테스트 설정")]
    [SerializeField] private bool enableIAPTestMode = true;

    [Header("카드 제한 경고")]
    [SerializeField] private CardLimitWarningDialog cardLimitWarningDialog;

    [Header("재화 부족 경고")]
    [SerializeField] GameObject lackOfCristalWarningPanelPrefab;
    // [SerializeField] GameObject lackOfGoldWarningPanelPrefab;

    [SerializeField] GameObject lackOfCristalWarningPanel;
    // GameObject lackOfGoldWarningPanel;

    GachaSystem gachaSystem;

    protected override void Init()
    {
        base.Init();

        // CardLimitWarningDialog 자동 찾기
        if (cardLimitWarningDialog == null)
        {
            cardLimitWarningDialog = FindObjectOfType<CardLimitWarningDialog>();
        }

        // ⭐ 경고 패널 생성
        CreateWarningPanels();
    }

    /// <summary>
    /// ⭐ 경고 패널 프리팹 인스턴스 생성
    /// </summary>
    void CreateWarningPanels()
    {
        // Canvas 찾기 (UI는 Canvas 아래에 있어야 함)
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Logger.LogError("[ShopManager] Canvas를 찾을 수 없습니다!");
            return;
        }

        // ⭐ 크리스탈 부족 경고 패널 생성
        if (lackOfCristalWarningPanelPrefab != null && lackOfCristalWarningPanel == null)
        {
            lackOfCristalWarningPanel = Instantiate(lackOfCristalWarningPanelPrefab, canvas.transform);
            lackOfCristalWarningPanel.SetActive(false);
            DontDestroyOnLoad(lackOfCristalWarningPanel); // ⭐ 씬 전환 시에도 유지
        }
        if (lackOfCristalWarningPanel != null)
        {
            lackOfCristalWarningPanel.SetActive(false);
            DontDestroyOnLoad(lackOfCristalWarningPanel); // ⭐ 씬 전환 시에도 유지
        }

        // // ⭐ 골드 부족 경고 패널 생성
        // if (lackOfGoldWarningPanelPrefab != null && lackOfGoldWarningPanel == null)
        // {
        //     lackOfGoldWarningPanel = Instantiate(lackOfGoldWarningPanelPrefab, canvas.transform);
        //     lackOfGoldWarningPanel.SetActive(false);
        //     DontDestroyOnLoad(lackOfGoldWarningPanel); // ⭐ 씬 전환 시에도 유지
        // }
    }

    /// <summary>
    /// 상품 구매 (FX 위치 포함)
    /// </summary>
    public bool PurchaseProduct(string productId, RectTransform fxStartPoint = null, System.Action onFailureClosed = null)
{
        if (!GameInitializer.IsInitialized)
        {
            Logger.LogError("[ShopManager] 게임이 아직 초기화되지 않았습니다.");
            return false;
        }

        if (ProductDataTable.Instance == null)
        {
            Logger.LogError("[ShopManager] ProductDataTable이 없습니다.");
            return false;
        }

        var productData = ProductDataTable.Instance.GetProductById(productId);
        if (productData == null)
        {
            Logger.LogError($"[ShopManager] Product Data가 없습니다. Product ID: {productId}");
            return false;
        }

        Logger.Log($"[ShopManager] 구매 시도: {productData.ProductName} (ID: {productId})");

        // ⭐ 상자/팩 구매 시 카드 수 체크
        if (productData.ProductType == ProductType.Box || productData.ProductType == ProductType.Pack)
        {
            if (!CanPurchaseCards(productData, onFailureClosed))  // ← 콜백 전달
            {
                Logger.Log($"[ShopManager] 카드 수 초과로 구매 불가: {productData.ProductId}");
                return false;
            }
        }

        switch (productData.PurchaseType)
        {
            case PurchaseType.Cristal:
            if (!PurchaseWithCristal(productData)) return false;  
                GiveProductReward(productData, fxStartPoint);
                return true;

            case PurchaseType.Gold:
            if (!PurchaseWithGold(productData)) return false; 
                GiveProductReward(productData, fxStartPoint);
                return true;

            case PurchaseType.IAP:
                if (enableIAPTestMode)
                {
                    Logger.Log($"[ShopManager] [테스트 모드] IAP 구매: {productData.ProductName}");
                    GiveProductReward(productData, fxStartPoint);
                }
                else
                {
                    if (IAPManager.Instance == null)
                    {
                        Logger.LogError("[ShopManager] IAPManager가 없습니다.");
                        return false;
                    }
                    Logger.Log($"[ShopManager] IAP 구매 시작: {productData.ProductId}");
                    IAPManager.Instance.BuyProduct(productData.ProductId);
                }
                return true;

            case PurchaseType.Ad:
                // ⭐ 광고 시청 처리
                PurchaseWithAd(productData, fxStartPoint);
                return true;
        }
        return false;
    }

    /// <summary>
    /// ⭐ 카드 구매 가능 여부 체크 (최대 카드 수 확인)
    /// </summary>
    bool CanPurchaseCards(ProductData productData, System.Action onWarningClosed = null)
    {
        if (productData == null)
            return false;

        if (!GameInitializer.IsInitialized)
        {
            Logger.LogError("[ShopManager] 게임 초기화가 완료되지 않았습니다.");
            return false;
        }

        var cardDataManager = FindObjectOfType<CardDataManager>();
        if (cardDataManager == null || !CardDataManager.IsDataLoaded)
        {
            Logger.LogError("[ShopManager] CardDataManager가 아직 준비되지 않았습니다.");
            return false;
        }

        string cardType = GetCardTypeFromTableId(productData.GachaTableId);
        int currentCardCount = GetCurrentCardCount(cardType);
        int drawCount = productData.DrawCount;
        int maxCardCount = StaticValues.MaxCardNum;

        Logger.Log($"[ShopManager] 카드 수 체크: {cardType} - 현재 {currentCardCount}개, +{drawCount}개 = {currentCardCount + drawCount}개 (최대 {maxCardCount}개)");

        if (currentCardCount + drawCount > maxCardCount)
        {
            if (cardLimitWarningDialog != null)
            {
                string cardTypeName = cardType == "Weapon" ? "오리" : "아이템";
                cardLimitWarningDialog.SetWarningText(
                    cardTypeName,
                    currentCardCount + drawCount,
                    maxCardCount,
                    onWarningClosed  // ← 콜백 전달
                );
            }
            else
            {
                Logger.LogWarning("[ShopManager] CardLimitWarningDialog를 찾을 수 없습니다.");
                // 다이얼로그가 없으면 콜백을 바로 호출해서 isProcessing이 막히지 않도록
                onWarningClosed?.Invoke();
            }
            return false;
        }

        return true;
    }

    /// <summary>
    /// ⭐ 현재 보유 카드 수 조회
    /// </summary>
    int GetCurrentCardCount(string cardType)
    {
        var cardDataManager = FindObjectOfType<CardDataManager>();

        // ⭐ 안전성 체크 (이미 CanPurchaseCards에서 체크하지만 방어적 코딩)
        if (cardDataManager == null)
        {
            Logger.LogError("[ShopManager] CardDataManager를 찾을 수 없습니다.");
            return 0;
        }

        var myCards = cardDataManager.GetMyCardList();

        // ⭐ null 체크
        if (myCards == null)
        {
            Logger.LogWarning("[ShopManager] 카드 리스트가 null입니다.");
            return 0;
        }

        int count = 0;

        foreach (var card in myCards)
        {
            if (cardType == "Weapon" && card.Type == CardType.Weapon.ToString())
            {
                count++;
            }
            else if (cardType == "Item" && card.Type == CardType.Item.ToString())
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// ⭐ 테이블 ID로 카드 타입 판단
    /// </summary>
    string GetCardTypeFromTableId(string gachaTableId)
    {
        if (string.IsNullOrEmpty(gachaTableId))
            return "Weapon"; // 기본값

        if (gachaTableId.Contains("duck") || gachaTableId.Contains("random") ||
            gachaTableId.Contains("starter") || gachaTableId.Contains("pro"))
            return "Weapon";
        else if (gachaTableId.Contains("item"))
            return "Item";

        Logger.LogWarning($"[ShopManager] 알 수 없는 테이블 ID: {gachaTableId}, Weapon으로 처리");
        return "Weapon";
    }

    /// <summary>
    /// 광고 시청으로 구매
    /// </summary>
    async void PurchaseWithAd(ProductData productData, RectTransform fxStartPoint)
    {
        Logger.Log($"[ShopManager] 광고 구매 시작: {productData.ProductName}");

        // 쿨다운 확인
        if (productData.ProductType == ProductType.Box)
        {
            if (TimeBasedBoxManager.Instance == null)
            {
                Logger.LogError("[ShopManager] TimeBasedBoxManager가 없습니다.");
                return;
            }

            // 서버 시간으로 쿨다운 확인
            bool canClaim = await TimeBasedBoxManager.Instance.CanClaimBoxAsync();

            if (!canClaim)
            {
                string remainingTime = TimeBasedBoxManager.Instance.GetRemainingTimeFormatted();
                Logger.Log($"[ShopManager] 상자 쿨다운 중: {remainingTime} 남음");
                ShowCooldownPopup(remainingTime);
                return;
            }
        }

        // 광고 준비 확인
        if (AdsManager.Instance == null)
        {
            Logger.LogError("[ShopManager] AdsManager가 없습니다.");
            return;
        }

        if (!AdsManager.IsRewardedAdReady)
        {
            Logger.LogWarning("[ShopManager] 광고가 아직 준비되지 않았습니다. 잠시 후 다시 시도해주세요.");
            ShowAdLoadingPopup();
            return;
        }

        Logger.Log($"[ShopManager] 광고 시청 시작: {productData.ProductName}");

        AdsManager.Instance.ShowDailyFreeGemRewardedAd(async () =>
        {
            Logger.Log($"[ShopManager] 광고 시청 완료!");

            if (productData.ProductType == ProductType.Box)
            {
                // 서버 시간으로 기록
                await TimeBasedBoxManager.Instance.OnBoxClaimedAsync();
            }

            StartCoroutine(GiveProductRewardCo(productData, fxStartPoint));
        });
    }

    void ShowAdLoadingPopup()
    {
        // TODO: "광고를 불러오는 중입니다..." 팝업
        Logger.Log("[ShopManager] 광고 로딩 중... 잠시만 기다려주세요.");
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
            ShowInsufficientCurrencyPopup("Cristal");
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
            ShowInsufficientCurrencyPopup("Gold");
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
                Logger.Log($"[ShopManager] 팩 보상 처리: {productData.ProductId}");
                OpenBox(productData, fxStartPoint);
                break;

            case ProductType.Box:
                Logger.Log($"[ShopManager] 상자 보상 처리: {productData.ProductId}");
                OpenBox(productData, fxStartPoint);
                break;
        }
    }

    IEnumerator GiveProductRewardCo(ProductData productData, RectTransform fxStartPoint)
    {
        yield return null;
        GiveProductReward(productData, fxStartPoint);
    }

    /// <summary>
    /// 상자 열기 (랜덤 보상)
    /// </summary>
    void OpenBox(ProductData productData, RectTransform fxStartPoint)
    {
        if (productData == null)
        {
            Logger.LogError($"[ShopManager] ProductData가 null입니다.");
            return;
        }

        // ⭐ GachaSystem에 위임
        if (!string.IsNullOrEmpty(productData.GachaTableId))
        {
            if (gachaSystem == null)
            {
                gachaSystem = FindObjectOfType<GachaSystem>();
            }

            if (gachaSystem != null)
            {
                Logger.Log($"[ShopManager] 가챠 실행: {productData.ProductId}, 테이블: {productData.GachaTableId}, {productData.DrawCount}개");

                gachaSystem.OpenBox(
                    productData.GachaTableId,
                    productData.DrawCount,
                    productData.GuaranteedCount,
                    productData.GuaranteedRarity
                );
            }
            else
            {
                Logger.LogError("[ShopManager] GachaSystem을 찾을 수 없습니다.");
            }
        }
        else
        {
            Logger.LogWarning($"[ShopManager] {productData.ProductId}는 가챠 테이블이 없습니다.");
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
        bool isCristal = currencyType == "Cristal" ? true : false;

        // // ⭐ 경고 패널 생성
        // if(lackOfCristalWarningPanel == null) CreateWarningPanels();

        if (isCristal)
        {
            if (lackOfCristalWarningPanel != null)
            {
                lackOfCristalWarningPanel.SetActive(true);
                lackOfCristalWarningPanel.GetComponentInChildren<PanelTween>()?.ShowWithScale();
            }
            else
            {
                Logger.LogError("[ShopManager] lackOfCristalWarningPanel이 생성되지 않았습니다!");
            }
        }
        // else
        // {
        //     if (lackOfGoldWarningPanel != null)
        //     {
        //         lackOfGoldWarningPanel.SetActive(true);
        //         lackOfGoldWarningPanel.GetComponentInChildren<PanelTween>()?.ShowWithScale();
        //     }
        //     else
        //     {
        //         Logger.LogError("[ShopManager] lackOfGoldWarningPanel이 생성되지 않았습니다!");
        //     }
        // }

        Logger.Log($"[ShopManager] {currencyType}이(가) 부족합니다!");
    }

    // ⭐ 파괴 시 정리
    protected override void OnDestroy()
    {
        base.OnDestroy();

        // if (lackOfCristalWarningPanel != null)
        //     Destroy(lackOfCristalWarningPanel);

        // if (lackOfGoldWarningPanel != null)
        //     Destroy(lackOfGoldWarningPanel);
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