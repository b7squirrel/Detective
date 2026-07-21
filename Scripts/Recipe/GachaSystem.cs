using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GachaSystem : MonoBehaviour
{
    CardDataManager cardDataManager => CardDataManager.Instance;   // ⭐ 프로퍼티로 변경
    CardList cardList => CardList.Instance;                         // ⭐ 프로퍼티로 변경
    CardsDictionary cardDictionary;
    CardSlotManager cardSlotManager;

    [SerializeField] TextAsset weaponPoolDatabase;
    [SerializeField] TextAsset itemPoolDatabase;
    List<CardData> weaponPools;
    List<CardData> itemPools;

    [Header("버튼 클릭 시 UI 관련 제어")]
    [SerializeField] GameObject content;
    [SerializeField] GameObject darkBG;

    [Header("튜토리얼 카드 풀")]
    [SerializeField] TextAsset tutorialDuckPoolDatabase;
    [SerializeField] TextAsset tutorialItemPoolDatabase;

    [Header("가챠 제외 카드")]
    [SerializeField] List<string> excludedCardNames = new List<string>();

    public static bool IsInitialized { get; private set; } = false;

    StatManager statManager;
    
    List<CardData> tutorialDuckPool;
    List<CardData> tutorialItemPool;

    MainMenuManager mainMenuManager;

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
        raritySystem = GetComponent<GachaRaritySystem>();

        cardsPicked = new List<CardData>();
        mainMenuManager = FindObjectOfType<MainMenuManager>();

        statManager = GetComponent<StatManager>();  // 이것도 같은 오브젝트에 옮겨야 함
    }

    void Start()
    {
        StartCoroutine(InitializeGachaSystem());
    }

    void OnDestroy() { IsInitialized = false; }

    IEnumerator InitializeGachaSystem()
    {
        yield return new WaitUntil(() => GameInitializer.IsInitialized);

        weaponPools = new ReadCardData().GetCardsList(weaponPoolDatabase);
        itemPools = new ReadCardData().GetCardsList(itemPoolDatabase);

        if (tutorialDuckPoolDatabase != null)
            tutorialDuckPool = new ReadCardData().GetCardsList(tutorialDuckPoolDatabase);
        if (tutorialItemPoolDatabase != null)
            tutorialItemPool = new ReadCardData().GetCardsList(tutorialItemPoolDatabase);

        IsInitialized = true;
        Logger.Log("[GachaSystem] 초기화 완료");
    }

    // ─────────────────────────────────────────────────────────
    //  등급 지정 뽑기 (내부 공통)
    // ─────────────────────────────────────────────────────────
    void DrawWithRarity(string _cardType, int rarity)
    {
        CardData newCardData;

        if (_cardType == "Weapon")
        {
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

    // 디버그용 — 등급 무시 랜덤 뽑기
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

    // ─────────────────────────────────────────────────────────
    //  OpenBox — 상자/팩 열기 진입점 (ShopManager에서 호출)
    // ─────────────────────────────────────────────────────────
    public void OpenBox(string gachaTableId, int drawCount, int guaranteedCount, string guaranteedRarity)
    {
        // 튜토리얼 분기
        if (TutorialManager.instance != null &&
            TutorialManager.instance.CurrentStep == TutorialStep.Step1_ShopUnlocked)
        {
            OpenBoxForTutorial(gachaTableId);
            return;
        }

        // 팩 전용 분기
        if (gachaTableId == "single_starter" || gachaTableId == "ten_starter")
        {
            OpenBoxForStarterPack(gachaTableId, guaranteedCount);
            return;
        }

        if (gachaTableId == "ten_pro")
        {
            OpenBoxForProPack(gachaTableId);
            return;
        }

        // 일반 상자 처리
        Logger.Log($"[GachaSystem] OpenBox 시작 - tableId: {gachaTableId}, drawCount: {drawCount}, guaranteedCount: {guaranteedCount}, guaranteedRarity: {guaranteedRarity}");

        if (raritySystem == null)
        {
            Logger.LogError("[GachaSystem] GachaRaritySystem이 없습니다.");
            return;
        }

        mainMenuManager.SetActiveTopTabs(false);
        mainMenuManager.SetActiveBottomTabs(false);

        cardDataManager.BeginBatchOperation();
        cardsPicked.Clear();

        try
        {
            int normalSlots = drawCount - guaranteedCount;
            string cardType = GetCardTypeFromTableId(gachaTableId);

            for (int i = 0; i < normalSlots; i++)
            {
                int rarity = raritySystem.GetRandomRarity(gachaTableId, false);
                DrawWithRarity(cardType, rarity);
                Logger.Log($"[GachaSystem] 일반 슬롯 #{i + 1}/{normalSlots}: Grade {rarity}");
            }

            for (int i = 0; i < guaranteedCount; i++)
            {
                int rarity = raritySystem.GetRandomRarity(gachaTableId, true);
                DrawWithRarity(cardType, rarity);
                Logger.Log($"[GachaSystem] 확정 슬롯 #{i + 1}/{guaranteedCount}: Grade {rarity}");
            }

            cardDataManager.EndBatchOperation();
            cardDataManager.RefreshCardList();
            ImmediateSaveEquipmentData();
            CloudSaveManager.Instance?.SaveToCloud();

            Logger.Log($"[GachaSystem] 상자 열기 완료: 총 {drawCount}개");
        }
        catch (Exception e)
        {
            Logger.LogError($"[GachaSystem] 상자 열기 오류: {e.Message}");
            cardDataManager.EndBatchOperation();
            throw;
        }

        ShowGachaResult();
    }

    // ─────────────────────────────────────────────────────────
    //  초보자 팩 전용 뽑기
    //  - 오리 1장 (Epic 90% / Legendary 10%)
    //  - 필수 슬롯 제외 나머지 중 오리 세트 Epic 장비 2개
    //  - 골드는 ShopManager.GiveProductReward()에서 처리
    // ─────────────────────────────────────────────────────────
    void OpenBoxForStarterPack(string gachaTableId, int guaranteedCount)
    {
        mainMenuManager.SetActiveTopTabs(false);
        mainMenuManager.SetActiveBottomTabs(false);

        cardDataManager.BeginBatchOperation();
        cardsPicked.Clear();

        try
        {
            int duckRarity = raritySystem.GetRandomRarity(gachaTableId, guaranteedCount > 0);
            CardData duckCard = DrawDuckCardWithRarity(duckRarity);

            if (duckCard == null)
            {
                Logger.LogError("[GachaSystem] 초보자 팩: 오리 카드 뽑기 실패");
                cardDataManager.EndBatchOperation();
                return;
            }

            GivePackEquipments(duckCard, 2, MyGrade.Epic);

            cardDataManager.EndBatchOperation();
            cardDataManager.RefreshCardList();
            ImmediateSaveEquipmentData();
            CloudSaveManager.Instance?.SaveToCloud();

            Logger.Log("[GachaSystem] 초보자 팩 뽑기 완료");
        }
        catch (Exception e)
        {
            Logger.LogError($"[GachaSystem] 초보자 팩 뽑기 오류: {e.Message}");
            cardDataManager.EndBatchOperation();
            throw;
        }

        ShowGachaResult();
    }

    // ─────────────────────────────────────────────────────────
    //  전문가 팩 전용 뽑기
    //  - 오리 1장 (Legendary 85% / Mythic 15% 확정)
    //  - 필수 슬롯 제외 나머지 중 오리 세트 Legendary 장비 2개
    //  - 골드는 ShopManager.GiveProductReward()에서 처리
    // ─────────────────────────────────────────────────────────
    void OpenBoxForProPack(string gachaTableId)
    {
        mainMenuManager.SetActiveTopTabs(false);
        mainMenuManager.SetActiveBottomTabs(false);

        cardDataManager.BeginBatchOperation();
        cardsPicked.Clear();

        try
        {
            int duckRarity = raritySystem.GetRandomRarity(gachaTableId, true);
            CardData duckCard = DrawDuckCardWithRarity(duckRarity);

            if (duckCard == null)
            {
                Logger.LogError("[GachaSystem] 전문가 팩: 오리 카드 뽑기 실패");
                cardDataManager.EndBatchOperation();
                return;
            }

            GivePackEquipments(duckCard, 2, MyGrade.Legendary);

            cardDataManager.EndBatchOperation();
            cardDataManager.RefreshCardList();
            ImmediateSaveEquipmentData();
            CloudSaveManager.Instance?.SaveToCloud();

            Logger.Log("[GachaSystem] 전문가 팩 뽑기 완료");
        }
        catch (Exception e)
        {
            Logger.LogError($"[GachaSystem] 전문가 팩 뽑기 오류: {e.Message}");
            cardDataManager.EndBatchOperation();
            throw;
        }

        ShowGachaResult();
    }

    // ─────────────────────────────────────────────────────────
    //  튜토리얼 전용 뽑기
    // ─────────────────────────────────────────────────────────
    private void OpenBoxForTutorial(string gachaTableId)
    {
        mainMenuManager.SetActiveTopTabs(false);
        mainMenuManager.SetActiveBottomTabs(false);

        cardDataManager.BeginBatchOperation();
        cardsPicked.Clear();

        // StatManager statManager = GetComponent<StatManager>();

        try
        {
            string cardType = GetCardTypeFromTableId(gachaTableId);

            if (cardType == "Weapon" && tutorialDuckPool != null && tutorialDuckPool.Count > 0)
            {
                CardData tutorialCard = CloneCardData(tutorialDuckPool[0]);
                int targetLevel = tutorialDuckPool[0].Level;
                tutorialCard.Level = 1;

                if (statManager != null)
                {
                    for (int j = 0; j < targetLevel - 1; j++)
                        statManager.LevelUp(tutorialCard);
                }

                cardDataManager.AddNewCardToMyCardsList(tutorialCard);
                AddEssentialEquip(tutorialCard);
                cardsPicked.Add(tutorialCard);
                AddCardSlot(tutorialCard);

                Logger.Log($"[GachaSystem] 튜토리얼 오리 카드 지급: {tutorialCard.Name} HP:{tutorialCard.Hp} ATK:{tutorialCard.Atk}");
            }
            else if (cardType == "Item" && tutorialItemPool != null && tutorialItemPool.Count > 0)
            {
                CardData tutorialCard = CloneCardData(tutorialItemPool[0]);
                int targetLevel = tutorialItemPool[0].Level;
                tutorialCard.Level = 1;

                if (statManager != null)
                {
                    for (int j = 0; j < targetLevel - 1; j++)
                        statManager.LevelUp(tutorialCard);
                }

                cardDataManager.AddNewCardToMyCardsList(tutorialCard);
                cardsPicked.Add(tutorialCard);
                AddCardSlot(tutorialCard);

                Logger.Log($"[GachaSystem] 튜토리얼 아이템 카드 지급: {tutorialCard.Name} ATK:{tutorialCard.Atk}");
            }
            else
            {
                Logger.LogWarning("[GachaSystem] 튜토리얼 카드 풀이 없어 일반 뽑기로 대체합니다.");
                int rarity = raritySystem.GetRandomRarity(gachaTableId, false);
                string cardType2 = GetCardTypeFromTableId(gachaTableId);
                DrawWithRarity(cardType2, rarity);
            }

            cardDataManager.EndBatchOperation();
            cardDataManager.RefreshCardList();
            ImmediateSaveEquipmentData();
        }
        catch (Exception e)
        {
            Logger.LogError($"[GachaSystem] 튜토리얼 뽑기 오류: {e.Message}");
            cardDataManager.EndBatchOperation();
            throw;
        }

        ShowGachaResult();
    }

    // ─────────────────────────────────────────────────────────
    //  팩 뽑기 헬퍼 메서드들
    // ─────────────────────────────────────────────────────────

    /// <summary>
    /// 오리 카드를 뽑아 내 카드 목록에 추가하고 필수 장비를 장착합니다.
    /// AddCardSlot은 호출하지 않습니다.
    /// ShowGachaResult()에서 cardsPicked 순회 시 AddItemSlotOf()로 처리됩니다.
    /// </summary>
    CardData DrawDuckCardWithRarity(int rarity)
    {
        List<CardData> filteredPool = weaponPools.FindAll(card => card.Grade == rarity);

        if (filteredPool.Count == 0)
        {
            Logger.LogWarning($"[GachaSystem] 등급 {rarity}의 오리가 없습니다.");
            return null;
        }

        int pickIndex = UnityEngine.Random.Range(0, filteredPool.Count);
        CardData duckCard = CloneCardData(filteredPool[pickIndex]);

        cardDataManager.AddNewCardToMyCardsList(duckCard);
        AddEssentialEquip(duckCard);

        cardsPicked.Add(duckCard);

        Logger.Log($"[GachaSystem] 팩 오리 카드: {duckCard.Name} (Grade {duckCard.Grade})");
        return duckCard;
    }

    /// <summary>
    /// 오리의 필수 장비 슬롯을 제외한 나머지 슬롯 중 count개를
    /// 지정 등급(targetGrade)의 세트 장비로 채워줍니다.
    ///
    /// 우선순위:
    /// 1순위 — 뽑힌 오리의 필수 장비와 같은 SetName + 지정 등급
    /// 2순위 — 등급과 부위만 맞는 아무 비필수 장비 (폴백)
    ///
    /// 장비 카드는 cardsPicked/AddCardSlot에 추가하지 않습니다.
    /// ShowGachaResult()의 AddItemSlotOf()가 오리 기준으로 장비 슬롯을 생성합니다.
    /// 장비 장착 후 UpdateCardDisplay()로 오리 슬롯 이미지를 즉시 갱신합니다.
    /// </summary>
    void GivePackEquipments(CardData duckCard, int count, int targetGrade)
    {
        List<string> allSlots = new List<string> { "Head", "Chest", "Face", "Hand" };

        // 필수 슬롯 제외
        string essentialSlot = duckCard.EssentialEquip;
        List<string> availableSlots = allSlots.FindAll(s => s != essentialSlot);

        // 랜덤 순서로 섞어서 count개 선택
        Shuffle(availableSlots);
        int giveCount = Mathf.Min(count, availableSlots.Count);

        // ⭐ 필수 장비의 SetName을 기준으로 세트 결정
        string targetSetName = GetEssentialSetName(duckCard);
        Logger.Log($"[GachaSystem] {duckCard.Name} 세트 장비 기준: '{targetSetName}'");

        for (int i = 0; i < giveCount; i++)
        {
            string targetSlot = availableSlots[i];

            // 1순위: 같은 세트 + 지정 등급 + 비필수
            List<CardData> candidates = itemPools.FindAll(item =>
                item.EquipmentType == targetSlot &&
                item.Grade == targetGrade &&
                item.EssentialEquip != "Essential" &&
                !string.IsNullOrEmpty(targetSetName) &&
                item.SetName == targetSetName
            );

            // 2순위: 등급 + 부위 + 비필수 (폴백)
            if (candidates.Count == 0)
            {
                candidates = itemPools.FindAll(item =>
                    item.EquipmentType == targetSlot &&
                    item.Grade == targetGrade &&
                    item.EssentialEquip != "Essential"
                );
                Logger.LogWarning($"[GachaSystem] '{targetSetName}' 세트 장비 없음 → 전체 폴백: {targetSlot} / Grade {targetGrade}");
            }

            if (candidates.Count == 0)
            {
                Logger.LogWarning($"[GachaSystem] 팩 장비 후보 없음 (전체 폴백도 없음): {targetSlot} / Grade {targetGrade}");
                continue;
            }

            int pickIndex = UnityEngine.Random.Range(0, candidates.Count);
            CardData equipCard = CloneCardData(candidates[pickIndex]);

            cardDataManager.AddNewCardToMyCardsList(equipCard);

            if (cardList != null)
                cardList.Equip(duckCard, equipCard);

            Logger.Log($"[GachaSystem] 팩 장비 지급: {equipCard.Name} ({targetSlot}, Grade {targetGrade}, Set: {equipCard.SetName})");
        }

        // 모든 장비 장착 완료 후 오리 슬롯 이미지 즉시 갱신
        if (cardSlotManager == null)
            cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager.UpdateCardDisplay(duckCard);
    }

    /// <summary>
    /// 오리의 필수 장비 SetName을 반환합니다.
    /// 여러 세트가 있으면 (예: Tennis Champion / Tennis Local) 랜덤 선택.
    /// 예: Tennis 오리 → "Tennis Local" 또는 "Tennis Champion"
    /// </summary>
    string GetEssentialSetName(CardData duckCard)
    {
        // 이 오리의 필수 장비 후보 (같은 등급)
        List<CardData> essentialItems = itemPools.FindAll(item =>
            item.EssentialEquip == "Essential" &&
            item.BindingTo == duckCard.Name &&
            item.Grade == duckCard.Grade
        );

        if (essentialItems.Count == 0)
        {
            Logger.LogWarning($"[GachaSystem] {duckCard.Name} (Grade {duckCard.Grade})의 필수 장비를 찾을 수 없습니다.");
            return null;
        }

        // 중복 없이 SetName 목록 추출
        List<string> setNames = new List<string>();
        foreach (var item in essentialItems)
        {
            if (!string.IsNullOrEmpty(item.SetName) && !setNames.Contains(item.SetName))
                setNames.Add(item.SetName);
        }

        if (setNames.Count == 0) return null;

        // 여러 세트 중 랜덤 선택 (예: Tennis Local / Tennis Champion)
        string chosen = setNames[UnityEngine.Random.Range(0, setNames.Count)];
        Logger.Log($"[GachaSystem] {duckCard.Name} 세트 선택: {chosen} (후보: {string.Join(", ", setNames)})");
        return chosen;
    }

    /// <summary>
    /// 가챠 결과 UI 표시 공통 헬퍼
    /// cardsPicked(오리 카드만)를 순회하며 AddItemSlotOf()로 장비 슬롯까지 생성합니다.
    /// </summary>
    void ShowGachaResult()
    {
        gachaPanelManager.gameObject.SetActive(true);
        gachaPanelManager.InitGachaPanel(cardsPicked);

        if (cardSlotManager == null)
            cardSlotManager = FindObjectOfType<CardSlotManager>();

        for (int i = 0; i < cardsPicked.Count; i++)
            cardSlotManager.AddItemSlotOf(cardsPicked[i]);

        content.SetActive(false);
        darkBG.SetActive(true);
    }

    /// <summary>
    /// List를 제자리 셔플 (Fisher-Yates)
    /// </summary>
    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // ─────────────────────────────────────────────────────────
    //  테이블 ID → 카드 타입 판단
    // ─────────────────────────────────────────────────────────
    private string GetCardTypeFromTableId(string gachaTableId)
    {
        if (gachaTableId.Contains("duck") || gachaTableId.Contains("random"))
            return "Weapon";
        else if (gachaTableId.Contains("item"))
            return "Item";
        else if (gachaTableId.Contains("starter") || gachaTableId.Contains("pro"))
            return "Weapon";

        Logger.LogWarning($"[GachaSystem] 알 수 없는 테이블 ID: {gachaTableId}, Weapon으로 처리");
        return "Weapon";
    }

    // ─────────────────────────────────────────────────────────
    //  Debug — 특정 카드 뽑기
    // ─────────────────────────────────────────────────────────
    #region Debug 특정 카드 뽑기
    public void DrawSpecificCard(string _cardType, int index, int grade, int num, int skill, int evo)
    {
        cardDataManager.BeginBatchOperation();
        Logger.Log($"[GachaSystem] {num}개 특정 카드 뽑기 시작");

        if (statManager == null)
            statManager = GetComponent<StatManager>();

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
                    statManager.ApplyEvoStats(newCardData, evo);
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
                    statManager.ApplyEvoStats(newCardData, evo);
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

    // ─────────────────────────────────────────────────────────
    //  CloneCardData
    // ─────────────────────────────────────────────────────────
    CardData CloneCardData(CardData original)
    {
        if (original == null) return null;

        return new CardData(
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
            original.PassiveSkill.ToString(),
            original.SetName
        );
    }

    // ─────────────────────────────────────────────────────────
    //  필수 장비 / 기본 장비 장착
    // ─────────────────────────────────────────────────────────
    public void AddEssentialEquip(CardData _oriCardData)
    {
        if (itemPools == null)
        {
            itemPools = new List<CardData>();
            itemPools = new ReadCardData().GetCardsList(itemPoolDatabase);
        }

        if (cardDictionary == null) cardDictionary = FindObjectOfType<CardsDictionary>();

        WeaponItemData weaponItemData = cardDictionary.GetWeaponItemData(_oriCardData);
        WeaponData wd = weaponItemData.weaponData;

        if (wd == null)
        {
            Logger.LogError($"[GachaSystem] WeaponData를 찾을 수 없습니다: {_oriCardData.Name}");
            return;
        }

        string part = _oriCardData.EssentialEquip;
        if (!defaultEquipIndex.TryGetValue(part, out int index))
        {
            Logger.LogError($"[GachaSystem] 알 수 없는 장비 부위: {part}");
            return;
        }

        Item defaultItem = wd.defaultItems[index];
        if (defaultItem == null)
        {
            Logger.LogError($"[GachaSystem] 필수 장비가 인스펙터에 없습니다. weaponData: {wd.Name}, index: {index}");
            return;
        }

        CardData itemCardData = cardDictionary.GetItemCardData(defaultItem.Name, _oriCardData.Grade);

        if (itemCardData == null)
        {
            Logger.LogError($"[GachaSystem] 필수 장비의 CardData를 찾을 수 없습니다: {defaultItem.Name}, Grade: {defaultItem.grade}");
            return;
        }

        CardData defaultEquip = CloneCardData(itemCardData);
        if (defaultEquip == null)
        {
            Logger.LogError($"[GachaSystem] CardData 복제 실패: {itemCardData.Name}");
            return;
        }

        try
        {
            cardDataManager.AddNewCardToMyCardsList(defaultEquip);
            cardList.Equip(_oriCardData, defaultEquip);
            Logger.Log($"[GachaSystem] {defaultEquip.Name}을 장착합니다");
        }
        catch (Exception e)
        {
            Logger.LogError($"[GachaSystem] 필수 장비 장착 오류: {e.Message}");
        }
    }

    public void AddDefaultEquip(CardData _oriCardData)
{
    if (_oriCardData == null)
    {
        Debug.LogError("[AddDefaultEquip] _oriCardData가 null입니다.");
        return;
    }

    if (itemPools == null)
    {
        itemPools = new List<CardData>();
        itemPools = new ReadCardData().GetCardsList(itemPoolDatabase);
    }

    if (cardDictionary == null) cardDictionary = FindObjectOfType<CardsDictionary>();
    if (cardDictionary == null)
    {
        Debug.LogError("[AddDefaultEquip] cardDictionary를 찾을 수 없습니다.");
        return;
    }

    WeaponItemData weaponItemData = cardDictionary.GetWeaponItemData(_oriCardData);
    if (weaponItemData == null)
    {
        Debug.LogError($"[AddDefaultEquip] weaponItemData가 null입니다. 카드: {_oriCardData.Name}");
        return;
    }

    WeaponData wd = weaponItemData.weaponData;
    if (wd == null)
    {
        Debug.LogError($"[AddDefaultEquip] wd(WeaponData)가 null입니다. 카드: {_oriCardData.Name}");
        return;
    }

    if (wd.defaultItems == null)
    {
        Debug.LogError($"[AddDefaultEquip] wd.defaultItems가 null입니다. WeaponData: {wd.name}");
        return;
    }

    Debug.Log($"[AddDefaultEquip] cardDataManager={(cardDataManager == null ? "NULL" : "OK")}, cardList={(cardList == null ? "NULL" : "OK")}");

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
                    Debug.Log($"[AddDefaultEquip] equipIndex={equipIndex} AddNewCardToMyCardsList 호출 직전");
                    cardDataManager.AddNewCardToMyCardsList(defaultEquips[equipIndex]);

                    Debug.Log($"[AddDefaultEquip] equipIndex={equipIndex} cardList.Equip 호출 직전");
                    cardList.Equip(_oriCardData, defaultEquips[equipIndex]);

                    Debug.Log($"[AddDefaultEquip] equipIndex={equipIndex} 완료");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error adding default equipment (equipIndex={equipIndex}): {e.Message}\n{e.StackTrace}");
                }
            }
        }
    }
}

    // ─────────────────────────────────────────────────────────
    //  레거시 / 디버그용 뽑기 메서드
    // ─────────────────────────────────────────────────────────
    public void DrawWeapons(int num)
    {
        Logger.Log($"[GachaSystem] DrawWeapons 호출됨! num={num}");
        int weaponCount = CountWeaponCards();
        int totalMaxCardLimit = StaticValues.MaxCardNum;

        if (weaponCount > totalMaxCardLimit)
        {
            Logger.Log($"[GachaSystem] 오리 카드의 갯수가 {totalMaxCardLimit}개를 넘습니다.");
            FindObjectOfType<CardLimitWarningDialog>().SetWarningText("오리", weaponCount, totalMaxCardLimit);
            return;
        }

        mainMenuManager.SetActiveTopTabs(false);
        mainMenuManager.SetActiveBottomTabs(false);

        cardDataManager.BeginBatchOperation();
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

            Logger.Log("[GachaSystem] 가챠 완료");
        }
        catch (Exception e)
        {
            Logger.LogError($"[GachaSystem] 가챠 오류: {e.Message}");
            cardDataManager.EndBatchOperation();
            throw;
        }

        ShowGachaResult();
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
            FindObjectOfType<CardLimitWarningDialog>().SetWarningText("아이템", itemCount, maxTotalCardLimit);
            return;
        }

        mainMenuManager.SetActiveTopTabs(false);
        mainMenuManager.SetActiveBottomTabs(false);

        cardDataManager.BeginBatchOperation();
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

        ShowGachaResult();
    }

    public void DrawCombo(int num)
    {
        mainMenuManager.SetActiveTopTabs(false);
        mainMenuManager.SetActiveBottomTabs(false);
        cardsPicked.Clear();
        for (int i = 0; i < num; i++)
        {
            for (int j = 0; j < 3; j++) Draw("Weapon");
            for (int k = 0; k < 7; k++) Draw("Item");
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
            if (card.Type == CardType.Weapon.ToString()) count++;
        }
        return count;
    }

    int CountItemCards()
    {
        List<CardData> myCards = cardDataManager.GetMyCardList();
        int count = 0;
        foreach (var card in myCards)
        {
            if (card.Type == CardType.Item.ToString()) count++;
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