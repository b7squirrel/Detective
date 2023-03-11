using UnityEngine;

public enum currencyType {coin, gold}
public enum classType {A, AA, AAA}

[System.Serializable]
public class ItemProperty
{
    public string name;
    public Sprite sprite;
    public currencyType currencyType;
    public classType classType;
    public int price;
}
