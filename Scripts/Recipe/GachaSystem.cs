using System.Collections.Generic;
using UnityEngine;

public class GachaSystem : MonoBehaviour
{
    CardDataManager cardDataManager;
    CardList cardList;
    CardsDictionary cardDictionary;

    [SerializeField] TextAsset gachaPoolDataBase;
    [SerializeField] TextAsset weaponPoolDatabase;
    [SerializeField] TextAsset itemPoolDatabase;
    List<CardData> gachaPools;
    List<CardData> weaponPools;
    List<CardData> itemPools;

    List<CardData> cardsPicked; // 뽑은 카드를 저장하는 리스트
    bool donePicking;

    void Awake()
    {
        cardDataManager = GetComponent<CardDataManager>();
        cardList = GetComponent<CardList>();

        cardsPicked = new List<CardData>();
    }

    void Draw(string _cardType)
    {
        if (gachaPools == null)
        {
            gachaPools = new();

            gachaPools = new ReadCardData().GetCardsList(gachaPoolDataBase);
        }
        if (weaponPools == null)
        {
            weaponPools = new();

            weaponPools = new ReadCardData().GetCardsList(weaponPoolDatabase);
        }
        if (itemPools == null)
        {
            itemPools = new();

            itemPools = new ReadCardData().GetCardsList(itemPoolDatabase);
        }

        CardData newCardData;

        if (_cardType == "Weapon")
        {
            int pickIndex = UnityEngine.Random.Range(0, weaponPools.Count);
            newCardData = weaponPools[pickIndex];
            cardDataManager.AddNewCardToMyCardsList(newCardData);
            AddDefaultEquip(newCardData);

            // Debug.Log(newCardData.Name + newCardData.ID + " 을 뽑았습니다");
            cardsPicked.Add(newCardData);
        }
        else if (_cardType == "Item")
        {
            int pickIndex = UnityEngine.Random.Range(0, itemPools.Count);
            newCardData = itemPools[pickIndex];
            cardDataManager.AddNewCardToMyCardsList(newCardData);

            // Debug.Log(newCardData.Name + newCardData.ID + " 을 뽑았습니다");
            cardsPicked.Add(newCardData);
        }

        gachaPools = null; // 생성된 카드 데이터가 가챠풀에 저장되어 버리므로
        weaponPools = null; // 생성된 카드 데이터가 가챠풀에 저장되어 버리므로
        itemPools = null; // 생성된 카드 데이터가 가챠풀에 저장되어 버리므로
    }

    public void AddEssentialEquip(CardData _oriCardData)
    {
        if (itemPools == null)
        {
            itemPools = new();

            itemPools = new ReadCardData().GetCardsList(itemPoolDatabase);
        }
        // List<CardData> sameItems = gachaPools.FindAll(x => x.BindingTo == _oriCardData.Name);

        // CardData defaultItem = sameItems.Find(x => x.DefaultItem == DefaultItem.Default.ToString());

        List<CardData> sameItems = new();
        CardData defaultItem = null;
        for (int i = 0; i < itemPools.Count; i++)
        {
            if(itemPools[i].BindingTo == _oriCardData.Name)
            {
                sameItems.Add(itemPools[i]);
            }
        }
        for (int i = 0; i < sameItems.Count; i++)
        {
            if(sameItems[i].DefaultItem == DefaultItem.Default.ToString())
            {
                defaultItem = sameItems[i];
            }
        }
        if (defaultItem == null) Debug.Log(_oriCardData.Name + "의 필수 무기가 NULL입니다");
        cardDataManager.AddNewCardToMyCardsList(defaultItem); // 기본 아이템을 생성
        cardList.Equip(_oriCardData, defaultItem);
    }

    public void AddDefaultEquip(CardData _oriCardData)
    {
        // 아이템풀을 item pool database에서 불러와서 채워 넣기
        if (itemPools == null)
        {
            itemPools = new();

            itemPools = new ReadCardData().GetCardsList(itemPoolDatabase);
        }

        // 카드데이터로 weapon data 얻어내기
        if (cardDictionary == null) cardDictionary = FindObjectOfType<CardsDictionary>();
        WeaponItemData weaponItemData = cardDictionary.GetWeaponItemData(_oriCardData);
        WeaponData wd = weaponItemData.weaponData;

        // 검색 속도 향상을 위해 Dictionary 생성
        // Key: (아이템 이름, 등급), Value: CardData
        Dictionary<(string Name, int Grade), CardData> itemLookup = new Dictionary<(string, int), CardData>();
        foreach (var item in itemPools)
        {
            itemLookup[(item.Name, item.Grade)] = item;
        }

        // weaponData의 디폴트 장비들을 탐색
        CardData[] defaultEquips = new CardData[4];
        for (int equipIndex = 0; equipIndex < 4; equipIndex++)
        {
            // 기본 장비가 없는 경우 명시적으로 null 처리
            if (wd.defaultItems[equipIndex] == null)
            {
                defaultEquips[equipIndex] = null;
                continue;
            }

            // Dictionary에서 직접 검색
            var searchKey = (wd.defaultItems[equipIndex].Name, wd.defaultItems[equipIndex].grade);
            if (itemLookup.TryGetValue(searchKey, out CardData matchingItem))
            {
                defaultEquips[equipIndex] = matchingItem;
                Debug.Log($"{defaultEquips[equipIndex].Name}을 장착합니다");
                cardDataManager.AddNewCardToMyCardsList(defaultEquips[equipIndex]); // 기본 아이템을 생성
                cardList.Equip(_oriCardData, defaultEquips[equipIndex]);
            }
            else
            {
                // 일치하는 아이템을 찾지 못한 경우 명시적으로 null 처리
                defaultEquips[equipIndex] = null;
                Debug.LogWarning($"Default item not found: {searchKey.Name} (Grade: {searchKey.grade})");
            }
        }
    }

    // 상점 버튼
    public void DrawWeapons(int num)
    {
        cardsPicked.Clear();
        for (int i = 0; i < num; i++)
        {
            Draw("Weapon");
        }
        DebugGacha(cardsPicked);
    }
    public void DrawWeaponsAboveGrade(int _grade)
    {
        cardsPicked.Clear();
        for (int i = 0; i < 1; i++)
        {
            Draw("Weapon");
        }
        DebugGacha(cardsPicked);
    }
    public void DrawItems(int num)
    {
        cardsPicked.Clear();
        for (int i = 0; i < num; i++)
        {
            Draw("Item");
        }
        DebugGacha(cardsPicked);
    }
    public void DrawCombo(int num)
    {
        cardsPicked.Clear();
        for (int i = 0; i < num; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Draw("Weapon");
            }

            for (int k = 0; k < 7; k++)
            {
                Draw("Item");
            }
        }
        DebugGacha(cardsPicked);
    }
    void DebugGacha(List<CardData> cards)
    {
        foreach (var item in cards)
        {
            string grade = MyGrade.mGrades[item.Grade].ToString();
            Debug.Log($"{grade} {item.Name}을 뽑았습니다.");
        }
    }
}
