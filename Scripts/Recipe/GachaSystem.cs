using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GachaSystem : MonoBehaviour
{
    CardDataManager cardDataManager;
    CardList cardList;
    CardsDictionary cardDictionary;
    CardSlotManager cardSlotManager;

    [SerializeField] TextAsset weaponPoolDatabase;
    [SerializeField] TextAsset itemPoolDatabase;
    List<CardData> weaponPools;
    List<CardData> itemPools;

    [Header("버튼 클릭 시 UI 관련 제어")]
    [SerializeField] GameObject content; // 가챠 화면으로 넘어갈 때 상점을 숨기기 위해
    [SerializeField] GameObject darkBG; // 가챠 화면으로 넘어갈 때 칠판 배경으로 덮기 위해

    MainMenuManager mainMenuManager;

    Dictionary<string, int> defaultEquipIndex = new Dictionary<string, int>{
            { "Head", 0 },
            { "Chest", 1 },
            { "Face", 2 },
            { "Hand", 3 }
    };

    List<CardData> cardsPicked; // 뽑은 카드를 저장하는 리스트

    [Header("가챠 패널")]
    [SerializeField] GachaPanelManager gachaPanelManager;

    void Awake()
    {
        cardDataManager = GetComponent<CardDataManager>();
        cardList = GetComponent<CardList>();

        cardsPicked = new List<CardData>();

        mainMenuManager = FindObjectOfType<MainMenuManager>();
    }
    void Start()
    {
        StartCoroutine(InitializeGachaSystem());
    }
    IEnumerator InitializeGachaSystem()
    {
        // 게임 초기화 대기
        yield return new WaitUntil(() => GameInitializer.IsInitialized);
        
        // 이제 안전하게 초기화
        weaponPools = new ReadCardData().GetCardsList(weaponPoolDatabase);
        itemPools = new ReadCardData().GetCardsList(itemPoolDatabase);
        
        Debug.Log("GachaSystem 초기화 완료");
    }

    void Draw(string _cardType)
    {
        CardData newCardData;

        if (_cardType == "Weapon")
        {
            int pickIndex = UnityEngine.Random.Range(0, weaponPools.Count);
            newCardData = CloneCardData(weaponPools[pickIndex]);
            cardDataManager.AddNewCardToMyCardsList(newCardData); // 내 카드 데이터에 등록하고 아이디 부여, 바로 세이브
            AddEssentialEquip(newCardData);

            cardsPicked.Add(newCardData);

            // 카드 슬롯 풀에 카드 슬롯 추가
            AddCardSlot(newCardData);
        }
        else if (_cardType == "Item")
        {
            int pickIndex = UnityEngine.Random.Range(0, itemPools.Count);
            newCardData = CloneCardData(itemPools[pickIndex]);
            cardDataManager.AddNewCardToMyCardsList(newCardData);

            cardsPicked.Add(newCardData);

            // 카드 슬롯 풀에 카드 슬롯 추가
            AddCardSlot(newCardData);
        }
    }
    #region Debug 특정 카드 뽑기
    public void DrawSpecificCard(string _cardType, int index, int grade, int num, int skill, int evo)
    {
        for (int i = 0; i < num; i++)
        {
            CardData newCardData;

            if (_cardType == "Weapon")
            {
                newCardData = CloneCardData(weaponPools[index]);
                newCardData.Grade = grade;
                newCardData.PassiveSkill = skill + 1; 
                newCardData.EvoStage = evo;
                cardDataManager.AddNewCardToMyCardsListWithSkill(newCardData); // 내 카드 데이터에 등록하고 아이디 부여, 바로 세이브
                AddEssentialEquip(newCardData);

                cardsPicked.Add(newCardData);

                // 카드 슬롯 풀에 카드 슬롯 추가
                AddCardSlot(newCardData);
            }
            else if (_cardType == "Item")
            {
                newCardData = CloneCardData(itemPools[index]);
                newCardData.Grade = grade;
                // cardDataManager.AddNewCardToMyCardsList(newCardData);

                newCardData.PassiveSkill = skill + 1;
                newCardData.EvoStage = evo;
                cardDataManager.AddNewCardToMyCardsListWithSkill(newCardData); // 내 카드 데이터에 등록하고 아이디 부여, 바로 세이브

                cardsPicked.Add(newCardData);

                // 카드 슬롯 풀에 카드 슬롯 추가
                AddCardSlot(newCardData);
                // if (cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
                // cardSlotManager.AddItemSlotOf(newCardData);
            }
        }

        DelayedSaveEquipmentData(); // 장비 장착을 모두 뽑고 나서 세이브re
    }
    #endregion

    CardData CloneCardData(CardData original)
    {
        if (original == null)
            return null;

        CardData clone = new CardData(
        "",
        original.Type,
        original.Grade.ToString(),
        original.EvoStage.ToString(),
        original.Name,
        original.Level.ToString(),
        original.Hp.ToString(),
        original.Atk.ToString(),
        original.EquipmentType,
        original.EssentialEquip,
        original.BindingTo,
        original.StartingMember,
        original.DefaultItem,
        original.PassiveSkill.ToString()
        );

        return clone;
    }

    public void AddEssentialEquip(CardData _oriCardData)
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

        // if (itemLookup == null)
        // {
        //     itemLookup = new Dictionary<(string, int), CardData>();
        //     foreach (var item in itemPools)
        //     {
        //         itemLookup[(item.Name, item.Grade)] = item;
        //     }
        // }

        // weaponData의 필수 장비 검색
        string part = _oriCardData.EssentialEquip;
        int index = defaultEquipIndex[part];

        CardData defaultEquip;
        // 필수 장비가 없는 경우 경고
        if (wd.defaultItems[index] == null)
        {
            Debug.LogError("필수 장비가 인스펙터에 없습니다");
            return;
        }

        // 스크립터블 오브젝트의 index로 검색
        int i = wd.defaultItems[index].itemIndex;
        CardData itemCardData = cardDictionary.GetItemCardData(i);
        if(itemCardData != null)
        {
            defaultEquip = CloneCardData(itemCardData); // 복제 사용
            if (defaultEquip != null)
            {
                try
                {
                    cardDataManager.AddNewCardToMyCardsList(defaultEquip);
                    cardList.Equip(_oriCardData, defaultEquip);
                    Debug.Log($"{defaultEquip.Name}을 장착합니다");
                    // AddCardSlot(defaultEquip);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error adding default equipment: {e.Message}");
                }
            }
        }
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
                defaultEquips[equipIndex] = CloneCardData(matchingItem); // 복제 사용
                if (defaultEquips[equipIndex] != null)
                {
                    try
                    {
                        cardDataManager.AddNewCardToMyCardsList(defaultEquips[equipIndex]);
                        cardList.Equip(_oriCardData, defaultEquips[equipIndex]);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error adding default equipment: {e.Message}");
                    }
                }
            }
        }
    }

    // 상점 버튼
    public void DrawWeapons(int num)
    {
        // 무기 카드 수 제한 확인
        int weaponCount = CountWeaponCards();
        int maxWeaponLimit = 100; // 최대 무기 카드 제한

        if (weaponCount > maxWeaponLimit)
        {
            Debug.Log($"오리 카드의 갯수가 {maxWeaponLimit}개를 넘습니다. 현재 {weaponCount}개의 오리 카드가 있습니다.");
            GetComponent<CardLimitWarningDialog>().SetWarningText("오리", weaponCount);
            return; // 메서드 실행 중단
        }

        mainMenuManager.SetActiveTopTabs(false);
        mainMenuManager.SetActiveBottomTabs(false);

        cardsPicked.Clear();
        for (int i = 0; i < num; i++)
        {
            Draw("Weapon");
        }

        DelayedSaveEquipmentData(); // 장비 장착을 모두 뽑고 나서 세이브re


        Debug.Log("after save");

        gachaPanelManager.gameObject.SetActive(true);
        gachaPanelManager.InitGachaPanel(cardsPicked);

        if (cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        for (int i = 0; i < cardsPicked.Count; i++)
        {
            cardSlotManager.AddItemSlotOf(cardsPicked[i]);
        }

        //가챠 패널로 넘어갈 때 상점 메뉴를 숨기고 배경으로 덮기
        content.SetActive(false);
        darkBG.SetActive(true);
    }
    void AddCardSlot(CardData card)
    {
        // 카드 슬롯 풀에 추가된 카드 슬롯 배치
        if (cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager.AddSlot(card);

        Debug.Log($"{card.ID} : {card.Name} 이 슬롯에 추가되었습니다.");
    }
    public void DrawWeaponsAboveGrade(int _grade)
    {
        mainMenuManager.SetActiveTopTabs(false);
        mainMenuManager.SetActiveBottomTabs(false);
        cardsPicked.Clear();
        for (int i = 0; i < 1; i++)
        {
            Draw("Weapon");
        }
        DebugGacha(cardsPicked);
    }
    public void DrawItems(int num)
    {
        // 아이템 카드 수 제한 확인
        int itemCount = CountItemCards();
        int maxItemLimit = 200; // 최대 아이템 카드 제한

        if (itemCount > maxItemLimit)
        {
            Debug.Log($"아이템 카드의 갯수가 {maxItemLimit}개를 넘습니다. 현재 {itemCount}개의 아이템 카드가 있습니다.");
            GetComponent<CardLimitWarningDialog>().SetWarningText("아이템", itemCount);
            return; // 메서드 실행 중단
        }

        mainMenuManager.SetActiveTopTabs(false);
        mainMenuManager.SetActiveBottomTabs(false);
        cardsPicked.Clear();
        for (int i = 0; i < num; i++)
        {
            Draw("Item");
        }
        gachaPanelManager.gameObject.SetActive(true);
        gachaPanelManager.InitGachaPanel(cardsPicked);

        //가챠 패널로 넘어갈 때 상점 메뉴를 숨기고 배경으로 덮기
        content.SetActive(false);
        darkBG.SetActive(true);
    }
    public void DrawCombo(int num)
    {
        mainMenuManager.SetActiveTopTabs(false);
        mainMenuManager.SetActiveBottomTabs(false);
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
    // 무기 카드 수를 계산하는 메서드
    int CountWeaponCards()
    {
        List<CardData> myCards = cardDataManager.GetMyCardList();
        int count = 0;

        foreach (var card in myCards)
        {
            if (card.Type == CardType.Weapon.ToString())
            {
                count++;
            }
        }

        return count;
    }
    // 아이템 카드 수를 계산하는 메서드
    int CountItemCards()
    {
        List<CardData> myCards = cardDataManager.GetMyCardList();
        int count = 0;

        foreach (var card in myCards)
        {
            if (card.Type == CardType.Item.ToString())
            {
                count++;
            }
        }

        return count;
    }

    public void ImmediateSaveEquipmentData()
    {
        cardList.ImmediateSaveEquipment();
    }

    public void DelayedSaveEquipmentData()
    {
        cardList.DelayedSaveEquipments();
        Debug.Log("Save on Gacha System");
    }
}
