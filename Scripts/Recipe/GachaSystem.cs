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
    [SerializeField] GameObject content;
    [SerializeField] GameObject darkBG;

    MainMenuManager mainMenuManager;

    Dictionary<string, int> defaultEquipIndex = new Dictionary<string, int>{
            { "Head", 0 },
            { "Chest", 1 },
            { "Face", 2 },
            { "Hand", 3 }
    };

    List<CardData> cardsPicked;

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
        
        Logger.Log("[GachaSystem] 초기화 완료");
    }

    void Draw(string _cardType)
    {
        CardData newCardData;

        if (_cardType == "Weapon")
        {
            int pickIndex = UnityEngine.Random.Range(0, weaponPools.Count);
            newCardData = CloneCardData(weaponPools[pickIndex]);
            cardDataManager.AddNewCardToMyCardsList(newCardData);
            AddEssentialEquip(newCardData);

            cardsPicked.Add(newCardData);
            AddCardSlot(newCardData);
        }
        else if (_cardType == "Item")
        {
            int pickIndex = UnityEngine.Random.Range(0, itemPools.Count);
            newCardData = CloneCardData(itemPools[pickIndex]);
            cardDataManager.AddNewCardToMyCardsList(newCardData);

            cardsPicked.Add(newCardData);
            AddCardSlot(newCardData);
        }
    }
    
    #region Debug 특정 카드 뽑기
    public void DrawSpecificCard(string _cardType, int index, int grade, int num, int skill, int evo)
    {
        // ⭐ 배치 모드 적용
        cardDataManager.BeginBatchOperation();
        Logger.Log($"[GachaSystem] {num}개 특정 카드 뽑기 시작");
        
        try
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
                    cardDataManager.AddNewCardToMyCardsListWithSkill(newCardData);
                    AddEssentialEquip(newCardData);

                    cardsPicked.Add(newCardData);
                    AddCardSlot(newCardData);
                }
                else if (_cardType == "Item")
                {
                    newCardData = CloneCardData(itemPools[index]);
                    newCardData.Grade = grade;
                    newCardData.PassiveSkill = skill + 1;
                    newCardData.EvoStage = evo;
                    cardDataManager.AddNewCardToMyCardsListWithSkill(newCardData);

                    cardsPicked.Add(newCardData);
                    AddCardSlot(newCardData);
                }
            }
            
            // ⭐ 배치 모드 종료
            cardDataManager.EndBatchOperation();
            cardDataManager.RefreshCardList();
            DelayedSaveEquipmentData();
            
            Logger.Log("[GachaSystem] 특정 카드 뽑기 완료");
        }
        catch (Exception e)
        {
            Logger.LogError($"[GachaSystem] 특정 카드 뽑기 오류: {e.Message}");
            cardDataManager.EndBatchOperation();
            throw;
        }
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
        if (itemPools == null)
        {
            itemPools = new();
            itemPools = new ReadCardData().GetCardsList(itemPoolDatabase);
        }

        if (cardDictionary == null) cardDictionary = FindObjectOfType<CardsDictionary>();
        WeaponItemData weaponItemData = cardDictionary.GetWeaponItemData(_oriCardData);
        WeaponData wd = weaponItemData.weaponData;

        string part = _oriCardData.EssentialEquip;
        int index = defaultEquipIndex[part];

        CardData defaultEquip;
        if (wd.defaultItems[index] == null)
        {
            Logger.LogError("필수 장비가 인스펙터에 없습니다");
            return;
        }

        int i = wd.defaultItems[index].itemIndex;
        CardData itemCardData = cardDictionary.GetItemCardData(i);
        if(itemCardData != null)
        {
            defaultEquip = CloneCardData(itemCardData);
            if (defaultEquip != null)
            {
                try
                {
                    cardDataManager.AddNewCardToMyCardsList(defaultEquip);
                    cardList.Equip(_oriCardData, defaultEquip);
                    Logger.Log($"{defaultEquip.Name}을 장착합니다");
                }
                catch (Exception e)
                {
                    Logger.LogError($"Error adding default equipment: {e.Message}");
                }
            }
        }
    }

    public void AddDefaultEquip(CardData _oriCardData)
    {
        if (itemPools == null)
        {
            itemPools = new();
            itemPools = new ReadCardData().GetCardsList(itemPoolDatabase);
        }

        if (cardDictionary == null) cardDictionary = FindObjectOfType<CardsDictionary>();
        WeaponItemData weaponItemData = cardDictionary.GetWeaponItemData(_oriCardData);
        WeaponData wd = weaponItemData.weaponData;

        Dictionary<(string Name, int Grade), CardData> itemLookup = new Dictionary<(string, int), CardData>();
        foreach (var item in itemPools)
        {
            itemLookup[(item.Name, item.Grade)] = item;
        }

        CardData[] defaultEquips = new CardData[4];
        for (int equipIndex = 0; equipIndex < 4; equipIndex++)
        {
            if (wd.defaultItems[equipIndex] == null)
            {
                defaultEquips[equipIndex] = null;
                continue;
            }

            var searchKey = (wd.defaultItems[equipIndex].Name, wd.defaultItems[equipIndex].grade);
            if (itemLookup.TryGetValue(searchKey, out CardData matchingItem))
            {
                defaultEquips[equipIndex] = CloneCardData(matchingItem);
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

    // ⭐ 개선된 DrawWeapons - 배치 모드 적용
    public void DrawWeapons(int num)
    {
        // 무기 카드 수 제한 확인
        int weaponCount = CountWeaponCards();
        int maxWeaponLimit = 100;

        if (weaponCount > maxWeaponLimit)
        {
            Logger.Log($"오리 카드의 갯수가 {maxWeaponLimit}개를 넘습니다. 현재 {weaponCount}개의 오리 카드가 있습니다.");
            GetComponent<CardLimitWarningDialog>().SetWarningText("오리", weaponCount);
            return;
        }

        mainMenuManager.SetActiveTopTabs(false);
        mainMenuManager.SetActiveBottomTabs(false);

        // ⭐ 배치 모드 시작
        cardDataManager.BeginBatchOperation();
        Logger.Log($"[GachaSystem] {num}개 무기 뽑기 시작");
        
        cardsPicked.Clear();
        
        try
        {
            // 모든 뽑기 실행 (저장 예약만 됨)
            for (int i = 0; i < num; i++)
            {
                Logger.Log($"[GachaSystem] Draw #{i+1}");
                Draw("Weapon");
            }
            
            // ⭐ 배치 모드 종료 (한 번만 저장)
            cardDataManager.EndBatchOperation();
            
            // ⭐ CardList 한 번만 초기화
            cardDataManager.RefreshCardList();
            
            // 장비 데이터도 저장
            ImmediateSaveEquipmentData();
            
            Logger.Log("[GachaSystem] 가챠 완료 - 모든 데이터 저장됨");
        }
        catch (Exception e)
        {
            Logger.LogError($"[GachaSystem] 가챠 오류: {e.Message}");
            cardDataManager.EndBatchOperation();
            throw;
        }

        // UI 업데이트
        gachaPanelManager.gameObject.SetActive(true);
        gachaPanelManager.InitGachaPanel(cardsPicked);

        if (cardSlotManager == null) 
            cardSlotManager = FindObjectOfType<CardSlotManager>();
        
        for (int i = 0; i < cardsPicked.Count; i++)
        {
            cardSlotManager.AddItemSlotOf(cardsPicked[i]);
        }

        content.SetActive(false);
        darkBG.SetActive(true);
    }
    
    void AddCardSlot(CardData card)
    {
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
    
    // ⭐ 개선된 DrawItems - 배치 모드 적용
    public void DrawItems(int num)
    {
        // 아이템 카드 수 제한 확인
        int itemCount = CountItemCards();
        int maxItemLimit = 200;

        if (itemCount > maxItemLimit)
        {
            Logger.Log($"아이템 카드의 갯수가 {maxItemLimit}개를 넘습니다. 현재 {itemCount}개의 아이템 카드가 있습니다.");
            GetComponent<CardLimitWarningDialog>().SetWarningText("아이템", itemCount);
            return;
        }

        mainMenuManager.SetActiveTopTabs(false);
        mainMenuManager.SetActiveBottomTabs(false);
        
        // ⭐ 배치 모드 적용
        cardDataManager.BeginBatchOperation();
        Logger.Log($"[GachaSystem] {num}개 아이템 뽑기 시작");
        
        cardsPicked.Clear();
        
        try
        {
            for (int i = 0; i < num; i++)
            {
                Logger.Log($"[GachaSystem] DrawItem #{i+1}");
                Draw("Item");
            }
            
            cardDataManager.EndBatchOperation();
            cardDataManager.RefreshCardList();
            
            Logger.Log("[GachaSystem] 아이템 가챠 완료");
        }
        catch (Exception e)
        {
            Logger.LogError($"[GachaSystem] 아이템 가챠 오류: {e.Message}");
            cardDataManager.EndBatchOperation();
            throw;
        }
        
        gachaPanelManager.gameObject.SetActive(true);
        gachaPanelManager.InitGachaPanel(cardsPicked);

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
            Logger.Log($"{grade} {item.Name}을 뽑았습니다.");
        }
    }
    
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
        Logger.Log("[GachaSystem] Save on Gacha System");
    }
}