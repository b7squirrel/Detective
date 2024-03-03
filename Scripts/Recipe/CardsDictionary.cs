using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponItemData
{
    public WeaponItemData(WeaponData _weaponData, Item _itemData)
    {
        weaponData = _weaponData;
        itemData = _itemData;
    }
    public WeaponData weaponData;
    public Item itemData;
}

// 모든 weaponData, item 들을 모아놓고
// cardData로 weaponData와 Item을 반환한다.
// GatchaSystem 과 upgradeSlot 두 클래스가 접근해서 값을 얻어간다. 
public class CardsDictionary : MonoBehaviour
{
    [SerializeField] TextAsset cardPoolDataBase;
    List<CardData> CardPool;
    [SerializeField] List<WeaponData> weaponData;
    [SerializeField] List<Item> itemData;

    void Awake()
    {
        if (CardPool == null)
        {
            CardPool = new();
            CardPool = new ReadCardData().GetCardsList(cardPoolDataBase);
        }
    }
    public List<CardData> GetCardPool()
    {
        return CardPool;
    }
    public WeaponItemData GetWeaponItemData(CardData cardData)
    {
        int grade = cardData.Grade;
        string _name = cardData.Name;

        if (cardData.Type == CardType.Weapon.ToString())
        {
            List<WeaponData> wd = weaponData.FindAll(x => x.Name == _name);
            WeaponData picked = wd.Find(x => x.grade == grade);
            WeaponItemData weaponItemData = new(picked, null);
            return weaponItemData;
        }
        else
        {
            List<Item> item = itemData.FindAll(x => x.Name == _name);
            Item picked = item.Find(x => x.grade == grade);
            WeaponItemData weaponItemData = new(null, picked);
            return weaponItemData;
        }
    }
}
