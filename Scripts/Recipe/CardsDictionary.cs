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
    [SerializeField] TextAsset itemPoolDataBase;
    List<CardData> CardPool;
    List<CardData> ItemPool;
    [SerializeField] List<WeaponData> weaponData;
    [SerializeField] List<Item> itemData;
    List<CardData> itemCardData;
    public static bool IsDataLoaded {get; private set;} = false;

    void Awake()
    {
        if(ItemPool == null)
        {
            Logger.Log("모든 아이템 종류를 로드합니다");
            ItemPool = new();
            ItemPool = new ReadCardData().GetCardsList(itemPoolDataBase);
        }

        SetIndex();

        IsDataLoaded = true;
        Logger.Log($"Cards Dictionary 데이터 로드 완료");
    }
    void OnApplicationQuit()
    {
        IsDataLoaded = false;       
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

    public string GetDisplayName(CardData cardData)
    {
        string dispName;
        WeaponItemData weaponItemData = GetWeaponItemData(cardData);
        if (weaponItemData.weaponData == null)
        {
            // dispName = weaponItemData.itemData.DisplayName;
            dispName = GameTextsManager.Texts.GetWeaponDisplayName(weaponItemData.itemData.Name);
        }
        else
        {
            // dispName = weaponItemData.weaponData.DisplayName;
            dispName = GameTextsManager.Texts.GetWeaponDisplayName(weaponItemData.weaponData.Name);
        }
        return dispName;
    }

    void SetIndex()
    {
        for (int i = 0; i < itemData.Count; i++)
        {
            itemData[i].itemIndex = i;
            CardData data = FindCardData(itemData[i]);
            if(data == null) 
            {
                Logger.Log($"{itemData[i]}에 해당하는 카드 데이터가 없습니다");
                return;
            }
            itemCardData.Add(data);
        }

        if(itemData.Count == itemCardData.Count)
        {
            Logger.Log("아이템 데이터와 아이템 카드 데이터가 동일하게 작성되었습니다.");
        }
        else
        {
            Logger.Log($"아이템 데이터 갯수 = {itemData.Count}, 아이템 카드 데이터 갯수 = {itemCardData.Count}");
        }
    }

    CardData FindCardData(Item item)
    {
        if (itemCardData == null) itemCardData = new List<CardData>();

        for (int i = 0; i < ItemPool.Count; i++)
        {
            if (ItemPool[i].Name == item.Name && ItemPool[i].Grade == item.grade)
                return ItemPool[i];
        }
        Logger.Log("해당 item의 Card data를 찾을 수 없습니다.");
        return null;
    }
    public Item GetItem(int index)
    {
        return itemData[index];
    }
    public CardData GetItemCardData(int index)
    {
        return itemCardData[index];
    }
}
