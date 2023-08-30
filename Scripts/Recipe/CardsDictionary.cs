using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 모든 weaponData, item 들을 모아놓고
// cardData로 weaponData와 Item을 반환한다.
// GatchaSystem 과 upgradeSlot 두 클래스가 접근해서 값을 얻어간다. 
public class CardsDictionary : MonoBehaviour
{
    [SerializeField] List<WeaponData> weaponData;
    [SerializeField] List<Item> itemData;
    [SerializeField] GameObject cardPrefab;

    public WeaponItemData GetWeaponItemData(CardData cardData)
    {
        string grade = cardData.Grade;
        string _name = cardData.Name;

        if (cardData.Type == CardType.Weapon.ToString())
        {
            List<WeaponData> wd = weaponData.FindAll(x => x.Name == _name);
            WeaponData picked = wd.Find(x => x.grade.ToString() == grade);
            WeaponItemData weaponItemData = new(picked, null);
            return weaponItemData;
        }
        else
        {
            List<Item> item = itemData.FindAll(x => x.Name == _name);
            Item picked = item.Find(x => x.grade.ToString() == grade);
            WeaponItemData weaponItemData = new(null, picked);
            return weaponItemData;
        }
    }

    public WeaponData GetWeaponData(CardData cardData)
    {
        string grade = cardData.Grade;
        string _name = cardData.Name;
        List<WeaponData> wd = weaponData.FindAll(x => x.Name == _name);
        WeaponData picked = wd.Find(x=>x.grade.ToString() == grade);
        return picked;
    }

    public Item GetItemData(CardData cardData)
    {
        string grade = cardData.Grade;
        string _name = cardData.Name;
        List<Item> item = itemData.FindAll(x => x.Name == _name);
        Item picked = item.Find(x => x.grade.ToString() == grade);
        return picked;
    }
}
