using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class IAPManager : SingletonBehaviour<IAPManager>, IDetailedStoreListener
{
    private IStoreController storeController;

    public static bool IsInitialized => _isInitialized;
    private static bool _isInitialized = false;

    const string CRISTAL_001 = "cristal_001";
    const string CRISTAL_002 = "cristal_002";
    const string CRISTAL_003 = "cristal_003";
    const string CRISTAL_004 = "cristal_004";
    const string CRISTAL_005 = "cristal_005";
    const string CRISTAL_006 = "cristal_006";
    const string CRISTAL_007 = "cristal_007";

    protected override void Init()
    {
        base.Init();
        InitializePurchasing();
    }

    void InitializePurchasing()
    {
        var builder = ConfigurationBuilder.Instance(
        StandardPurchasingModule.Instance());

        builder.AddProduct(CRISTAL_001, UnityEngine.Purchasing.ProductType.Consumable);
        builder.AddProduct(CRISTAL_002, UnityEngine.Purchasing.ProductType.Consumable);
        builder.AddProduct(CRISTAL_003, UnityEngine.Purchasing.ProductType.Consumable);
        builder.AddProduct(CRISTAL_004, UnityEngine.Purchasing.ProductType.Consumable);
        builder.AddProduct(CRISTAL_005, UnityEngine.Purchasing.ProductType.Consumable);
        builder.AddProduct(CRISTAL_006, UnityEngine.Purchasing.ProductType.Consumable);
        builder.AddProduct(CRISTAL_007, UnityEngine.Purchasing.ProductType.Consumable);

        UnityPurchasing.Initialize(this, builder);
    }

    public void BuyProduct(string productId)
    {
        if (!_isInitialized)
        {
            Logger.LogError("[IAPManager] IAP 초기화가 완료되지 않았습니다.");
            return;
        }
        storeController.InitiatePurchase(productId);
    }

    // 초기화 성공
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        _isInitialized = true;
        Logger.Log("[IAPManager] IAP 초기화 성공");
    }

    // 구매 성공 → ProductDataTable에서 보상 조회 후 지급
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string productId = args.purchasedProduct.definition.id;
        Logger.Log($"[IAPManager] 구매 완료: {productId}");

        var productData = ProductDataTable.Instance?.GetProductById(productId);

        if (productData == null)
        {
            Logger.LogError($"[IAPManager] ProductDataTable에서 {productId}를 찾을 수 없습니다.");
            return PurchaseProcessingResult.Complete;
        }

        if (productData.RewardCristal > 0)
        {
            PlayerDataManager.Instance.AddCristal(productData.RewardCristal);
            Logger.Log($"[IAPManager] 크리스탈 {productData.RewardCristal}개 지급 완료");
        }

        return PurchaseProcessingResult.Complete;
    }

    // 구매 실패
    // IDetailedStoreListener 버전
    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Logger.LogWarning($"[IAPManager] 구매 실패: {product.definition.id} - {failureDescription.reason}");
    }

    // IStoreListener 버전
    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        Logger.LogWarning($"[IAPManager] 구매 실패: {product.definition.id} - {reason}");
    }

    public void OnInitializeFailed(InitializationFailureReason error) =>
        Logger.LogError($"[IAPManager] 초기화 실패: {error}");

    public void OnInitializeFailed(InitializationFailureReason error, string message) =>
        Logger.LogError($"[IAPManager] 초기화 실패: {error} - {message}");
}