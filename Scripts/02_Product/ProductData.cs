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
    public string RewardItemId;  // int -> string (pack_001 같은 형식 지원)
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