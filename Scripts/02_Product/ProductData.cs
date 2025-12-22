[System.Serializable]
public class ProductData
{
    public string ProductId;
    public ProductType ProductType;
    public string ProductName;
    public PurchaseType PurchaseType;
    public int PurchaseCost;
    public int RewardCristal;  // 오타 수정: Gem -> Cristal (데이터와 일치)
    public int RewardGold;
    public string RewardItemId;     // "duck_weapon", "item_equip" 등
    public string GachaTableId;     // 추가: "single_duck", "ten_duck" 등
    public int GuaranteedCount;     // 추가: 10연차의 경우 1
    public string GuaranteedRarity; // 추가: "Legendary" 등
}

public enum ProductType
{
    Pack,
    Box,
    Cristal,
    Gold
}

public enum PurchaseType
{
    IAP,
    Ad,
    Cristal,
    Gold
}