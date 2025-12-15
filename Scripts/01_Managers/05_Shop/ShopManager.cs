public class ShopManager : SingletonBehaviour<ShopManager>
{
    public void PurchaseProduct(string productId)
    {
        var productData = ProductDataTable.instance.GetProductById(productId);
        if (productData == null)
        {
            Logger.LogError($"Product Data가 없습니다. Product ID:{productId}");
            return;
        }

        switch (productData.PurchaseType)
        {
            case PurchaseType.Cristal:
            {
                var playerDataManager = PlayerDataManager.Instance;
                int currentCristal = playerDataManager.GetCurrentCristalNumber();

                if (currentCristal < productData.PurchaseCost)
                {
                    Logger.Log("크리스탈이 부족합니다.");
                    return;
                }

                playerDataManager.SetCristalNumberAs(
                    currentCristal - productData.PurchaseCost
                );

                GetProductReward(productId);
            }
            break;
        }
    }

    void GetProductReward(string productId)
    {
        var productData = ProductDataTable.instance.GetProductById(productId);
        var playerDataManager = PlayerDataManager.Instance;

        switch (productData.ProductType)
        {
            case ProductType.Gold:
                playerDataManager.SetCoinNumberAs(
                    playerDataManager.GetCurrentCoinNumber() + productData.RewardGold
                );
                break;

            case ProductType.Cristal:
                playerDataManager.SetCristalNumberAs(
                    playerDataManager.GetCurrentCristalNumber() + productData.RewardCristal
                );
                break;
        }
    }
}