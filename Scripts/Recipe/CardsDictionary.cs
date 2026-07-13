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
public class CardsDictionary : SingletonBehaviour<CardsDictionary>
{
    [SerializeField] TextAsset itemPoolDataBase;
    List<CardData> CardPool;
    List<CardData> ItemPool;
    [SerializeField] List<WeaponData> weaponData;
    List<Item> itemData = new List<Item>();
    List<CardData> itemCardData = new List<CardData>();
    public static bool IsDataLoaded { get; private set; } = false;
    Dictionary<(string Name, int Grade), CardData> itemCardDataMap
    = new Dictionary<(string, int), CardData>(); // 카드 검색
    [Header("자동 로드 설정")]
    [SerializeField] string itemSOFolderPath = "03_Equipment"; // Resources/ 이후 경로

    protected override void OnDestroy()
    {
        bool wasRealInstance = (Instance == this);
        base.OnDestroy();
        if (wasRealInstance)
        {
            IsDataLoaded = false;
        }
    }
    // ⭐ 기존 void Awake() 제거하고 Init()으로 대체
    protected override void Init()
    {
        base.Init(); // 중복 체크 + DontDestroyOnLoad 처리

        if (Instance != this) return; // 가짜(중복) 인스턴스면 아래 로직 스킵

        // ★ 1. SO 자동 로드
        LoadItemScriptableObjects();

        // 2. CSV 로드
        if (ItemPool == null)
        {
            Logger.Log("모든 아이템 종류를 로드합니다");
            ItemPool = new ReadCardData().GetCardsList(itemPoolDataBase);
        }

        // 3. 인덱스 설정
        SetIndex();

        // ★ 4. 개발 중 자동 검증
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        ValidateItemConsistency();
#endif

        IsDataLoaded = true;
        Logger.Log($"Cards Dictionary 데이터 로드 완료 (아이템 SO: {itemData.Count}개)");
    }

    /// <summary>
    /// ★ Resources 폴더에서 Item SO를 자동으로 읽어옴
    /// </summary>
    void LoadItemScriptableObjects()
    {
        itemData.Clear();

        Item[] loaded = Resources.LoadAll<Item>(itemSOFolderPath);

        if (loaded == null || loaded.Length == 0)
        {
            Logger.LogError($"[CardsDictionary] '{itemSOFolderPath}' 에서 Item SO를 찾을 수 없습니다. 경로를 확인하세요.");
            return;
        }

        // Name + grade 기준으로 정렬 (일관성 유지)
        System.Array.Sort(loaded, (a, b) =>
        {
            int nameCompare = string.Compare(a.Name, b.Name, System.StringComparison.Ordinal);
            if (nameCompare != 0) return nameCompare;
            return a.grade.CompareTo(b.grade);
        });

        itemData.AddRange(loaded);
        Logger.Log($"[CardsDictionary] Item SO 자동 로드 완료: {itemData.Count}개 ({itemSOFolderPath})");
    }
    void ValidateItemConsistency()
    {
        int errorCount = 0;

        foreach (var csvCard in ItemPool)
        {
            if (csvCard.Type != CardType.Item.ToString()) continue;

            bool found = itemData.Exists(so =>
                so != null &&
                so.Name == csvCard.Name &&
                (int)so.grade == csvCard.Grade);

            if (!found)
            {
                Logger.LogError($"[검증] CSV 항목이 SO에 없음 → Name: '{csvCard.Name}', Grade: {csvCard.Grade}");
                errorCount++;
            }
        }

        if (errorCount == 0)
            Logger.Log("✅ [검증] 모든 아이템 데이터가 일치합니다.");
        else
            Logger.LogError($"[검증] SO 누락 {errorCount}개 발견. 위 로그를 확인하세요.");
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
            dispName = LocalizationManager.Char.GetWeaponDisplayName(weaponItemData.itemData.Name);
        }
        else
        {
            // dispName = weaponItemData.weaponData.DisplayName;
            dispName = LocalizationManager.Char.GetWeaponDisplayName(weaponItemData.weaponData.Name);
        }
        return dispName;
    }

    void SetIndex()
    {
        // ★ 임시 디버그
        Logger.Log($"ItemPool 개수: {ItemPool?.Count ?? -1}");
        if (ItemPool != null && ItemPool.Count > 0)
            Logger.Log($"첫 번째 항목: Name='{ItemPool[0].Name}', Grade={ItemPool[0].Grade}, Type={ItemPool[0].Type}");

        for (int i = 0; i < itemData.Count; i++)
        {
            // ★ itemData[i].itemIndex = i;  ← 제거

            CardData data = FindCardData(itemData[i]);
            if (data == null)
            {
                Logger.LogError($"{itemData[i]}에 해당하는 카드 데이터가 없습니다");
                return;
            }
            itemCardData.Add(data);

            // ★ Dictionary에 등록
            var key = (itemData[i].Name, (int)itemData[i].grade);
            itemCardDataMap[key] = data;
        }

        if (itemData.Count == itemCardData.Count)
            Logger.Log("아이템 데이터와 아이템 카드 데이터가 동일하게 작성되었습니다.");
        else
            Logger.Log($"아이템 데이터 갯수 = {itemData.Count}, 아이템 카드 데이터 갯수 = {itemCardData.Count}");
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
    // public CardData GetItemCardData(int index)
    // {
    //     return itemCardData[index];
    // }
    public CardData GetItemCardData(string name, int grade)
    {
        var key = (name, grade);
        if (itemCardDataMap.TryGetValue(key, out CardData result))
            return result;

        Logger.LogError($"[CardsDictionary] CardData를 찾을 수 없음: Name '{name}', Grade {grade}");
        return null;
    }

    public Item GetItemByName(string name, int grade = 0)
    {
        return itemData.Find(x => x != null && x.Name == name && x.grade == grade);
    }
}
