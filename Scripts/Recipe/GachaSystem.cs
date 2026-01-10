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

    // ⭐ 가챠 확률 시스템 추가
    GachaRaritySystem raritySystem;

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
        raritySystem = GetComponent<GachaRaritySystem>(); // ⭐ 확률 시스템 참조

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

    // ⭐ 등급을 지정해서 뽑는 새로운 메서드
    void DrawWithRarity(string _cardType, int rarity)
    {
        CardData newCardData;

        if (_cardType == "Weapon")
        {
            // 해당 등급의 카드만 필터링
            List<CardData> filteredPool = weaponPools.FindAll(card => card.Grade == rarity);

            if (filteredPool.Count == 0)
            {
                Logger.LogWarning($"[GachaSystem] 등급 {rarity}에 해당하는 무기가 없습니다.");
                return;
            }

            int pickIndex = UnityEngine.Random.Range(0, filteredPool.Count);
            newCardData = CloneCardData(filteredPool[pickIndex]);
            cardDataManager.AddNewCardToMyCardsList(newCardData);
            AddEssentialEquip(newCardData);

            cardsPicked.Add(newCardData);
            AddCardSlot(newCardData);
        }
        else if (_cardType == "Item")
        {
            List<CardData> filteredPool = itemPools.FindAll(card => card.Grade == rarity);

            if (filteredPool.Count == 0)
            {
                Logger.LogWarning($"[GachaSystem] 등급 {rarity}에 해당하는 아이템이 없습니다.");
                return;
            }

            int pickIndex = UnityEngine.Random.Range(0, filteredPool.Count);
            newCardData = CloneCardData(filteredPool[pickIndex]);
            cardDataManager.AddNewCardToMyCardsList(newCardData);

            cardsPicked.Add(newCardData);
            AddCardSlot(newCardData);
        }
    }

    // ⭐ 기존 Draw는 유지 (디버그용)
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

    // ⭐ ShopManager에서 호출할 새로운 메서드
    public void OpenBox(string gachaTableId, int drawCount, int guaranteedCount, string guaranteedRarity)
    {
         // ⭐ 디버깅 로그 추가
    Logger.Log($"[GachaSystem] OpenBox 시작");
    Logger.Log($"  - gachaTableId: {gachaTableId}");
    Logger.Log($"  - drawCount: {drawCount}");
    Logger.Log($"  - guaranteedCount: {guaranteedCount}");
    Logger.Log($"  - guaranteedRarity: {guaranteedRarity}");
        if (raritySystem == null)
        {
            Logger.LogError("[GachaSystem] GachaRaritySystem이 없습니다.");
            return;
        }

        mainMenuManager.SetActiveTopTabs(false);
        mainMenuManager.SetActiveBottomTabs(false);

        cardDataManager.BeginBatchOperation();
        Logger.Log($"[GachaSystem] 상자/팩 열기: {gachaTableId}, {drawCount}개 뽑기");

        cardsPicked.Clear();

        try
        {
            int normalSlots = drawCount - guaranteedCount;
            Logger.Log($"[GachaSystem] normalSlots: {normalSlots}, guaranteedCount: {guaranteedCount}");

            // ⭐ gachaTableId로 타입 판단
            string cardType = GetCardTypeFromTableId(gachaTableId);

            // 일반 슬롯 뽑기
            for (int i = 0; i < normalSlots; i++)
            {
                Logger.Log($"[GachaSystem] 일반 슬롯 뽑기 시작 #{i+1}");
                int rarity = raritySystem.GetRandomRarity(gachaTableId, false);
                DrawWithRarity(cardType, rarity);
                Logger.Log($"[GachaSystem] 일반 슬롯 #{i + 1}/{normalSlots}: Grade {rarity}");
            }

            // 확정 슬롯 뽑기
            Logger.Log($"[GachaSystem] 확정 슬롯 루프 시작 (count: {guaranteedCount})");
            for (int i = 0; i < guaranteedCount; i++)
            {
                Logger.Log($"[GachaSystem] 확정 슬롯 뽑기 시작 #{i+1}");
                int rarity = raritySystem.GetRandomRarity(gachaTableId, true);
                DrawWithRarity(cardType, rarity);
                Logger.Log($"[GachaSystem] 확정 슬롯 #{i + 1}/{guaranteedCount}: Grade {rarity}");
            }

            cardDataManager.EndBatchOperation();
            cardDataManager.RefreshCardList();
            ImmediateSaveEquipmentData();

            Logger.Log($"[GachaSystem] 상자/팩 열기 완료: 총 {drawCount}개 뽑음");
        }
        catch (Exception e)
        {
            Logger.LogError($"[GachaSystem] 상자/팩 열기 오류: {e.Message}");
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

    // ⭐ 테이블 ID로 카드 타입 판단
    private string GetCardTypeFromTableId(string gachaTableId)
    {
        if (gachaTableId.Contains("duck") || gachaTableId.Contains("random"))
            return "Weapon";
        else if (gachaTableId.Contains("item"))
            return "Item";
        else if (gachaTableId.Contains("starter") || gachaTableId.Contains("pro"))
            return "Weapon"; // 팩도 무기 카드 지급

        Logger.LogWarning($"[GachaSystem] 알 수 없는 테이블 ID: {gachaTableId}, Weapon으로 처리");
        return "Weapon";
    }

    #region Debug 특정 카드 뽑기
    public void DrawSpecificCard(string _cardType, int index, int grade, int num, int skill, int evo)
    {
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
        if (itemCardData != null)
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

    // ⭐ 기존 DrawWeapons는 유지 (디버그/레거시 코드)
    public void DrawWeapons(int num)
    {
        Logger.Log($"[GachaSystem] ★★★ DrawWeapons 호출됨! num={num}, Time={Time.time}");
        int weaponCount = CountWeaponCards();
        int totalMaxCardLimit = StaticValues.MaxCardNum;

        if (weaponCount > totalMaxCardLimit)
        {
            Logger.Log($"[GachaSystem] 오리 카드의 갯수가 {totalMaxCardLimit}개를 넘습니다.");
            GetComponent<CardLimitWarningDialog>().SetWarningText("오리", weaponCount, totalMaxCardLimit);
            return;
        }

        mainMenuManager.SetActiveTopTabs(false);
        mainMenuManager.SetActiveBottomTabs(false);

        cardDataManager.BeginBatchOperation();
        Logger.Log($"[GachaSystem] {num}개 무기 뽑기 시작");

        cardsPicked.Clear();

        try
        {
            for (int i = 0; i < num; i++)
            {
                Logger.Log($"[GachaSystem] Draw #{i + 1}");
                Draw("Weapon");
            }

            cardDataManager.EndBatchOperation();
            cardDataManager.RefreshCardList();
            ImmediateSaveEquipmentData();

            Logger.Log("[GachaSystem] 가챠 완료 - 모든 데이터 저장됨");
        }
        catch (Exception e)
        {
            Logger.LogError($"[GachaSystem] 가챠 오류: {e.Message}");
            cardDataManager.EndBatchOperation();
            throw;
        }

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

    public void DrawItems(int num)
    {
        int itemCount = CountItemCards();
        int maxTotalCardLimit = StaticValues.MaxCardNum;

        if (itemCount > maxTotalCardLimit)
        {
            Logger.Log($"아이템 카드의 갯수가 {maxTotalCardLimit}개를 넘습니다.");
            GetComponent<CardLimitWarningDialog>().SetWarningText("아이템", itemCount, maxTotalCardLimit);
            return;
        }

        mainMenuManager.SetActiveTopTabs(false);
        mainMenuManager.SetActiveBottomTabs(false);

        cardDataManager.BeginBatchOperation();
        Logger.Log($"[GachaSystem] {num}개 아이템 뽑기 시작");

        cardsPicked.Clear();

        try
        {
            for (int i = 0; i < num; i++)
            {
                Logger.Log($"[GachaSystem] DrawItem #{i + 1}");
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