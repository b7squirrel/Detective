// ============================================================
//  파일 1 of 4 : EncyclopediaManager.cs
//  역할 : itemPool.txt 파싱 → SetInfo 빌드 → ScrollView 생성
// ============================================================
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ── 런타임 데이터 클래스 ──────────────────────────────────────
/// <summary>슬롯 하나에 놓이는 아이템 1종의 런타임 정보</summary>
public class EncycItemInfo
{
    public string internalName;      // "Archer Club Bow"
    public EquipmentType slot;       // Head / Face / Chest / Hand
    public bool isEssential;
    public int[] hp = new int[5]; // grade 0-4
    public int[] atk = new int[5]; // grade 0-4
    public Item itemSO; // ← 추가
}

/// <summary>세트 1개의 런타임 정보</summary>
public class EncycSetInfo
{
    public string setName;
    // 표시 순서: 0=Head, 1=Face, 2=Chest, 3=Hand
    public List<EncycItemInfo>[] slotItems = new List<EncycItemInfo>[4];

    public EncycSetInfo(string name)
    {
        setName = name;
        for (int i = 0; i < 4; i++)
            slotItems[i] = new List<EncycItemInfo>();
    }

    public bool HasItemInSlot(int idx) => slotItems[idx].Count > 0;
}

// ── EncyclopediaManager ───────────────────────────────────────
public class EncyclopediaManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] TextAsset itemPoolDataBase;
    [SerializeField] List<SetBonusDefinition> setDefinitions = new List<SetBonusDefinition>();
    CardsDictionary cardsDictionary => CardsDictionary.Instance;
    List<EncyclopediaSetEntry> spawnedEntries = new List<EncyclopediaSetEntry>();

    [Header("Set Bonus SO 폴더 경로 (Resources/ 이후)")]
    [SerializeField] string setBonusFolderPath = "SetBonusDefinitions";

    [Header("UI")]
    [SerializeField] Transform scrollContent;
    [SerializeField] GameObject setEntryPrefab;
    [SerializeField] EncyclopediaPopup popup;

    [Header("도감에서 숨길 세트")]
    [SerializeField] List<string> hiddenSetNames = new List<string>();

    static readonly string[] SLOT_ORDER = { "Head", "Face", "Chest", "Hand" };

    List<EncycSetInfo> setList = new List<EncycSetInfo>();
    Dictionary<string, EncycSetInfo> setMap = new Dictionary<string, EncycSetInfo>();

    // ★ 플레이어가 보유한 아이템 이름 집합
    HashSet<string> acquiredItemNames = new HashSet<string>();

    void Start()
    {
        StartCoroutine(InitWhenReady());
    }
    IEnumerator InitWhenReady()
    {
        yield return new WaitUntil(() =>
            CardDataManager.IsDataLoaded &&
            CardsDictionary.IsDataLoaded);

        LoadSetBonusDefinitions();
        BuildAcquiredSet();
        BuildSetData(); // ★ 먼저 데이터 빌드

        // ★ 빌드 후 숨길 세트 제거
        if (hiddenSetNames != null && hiddenSetNames.Count > 0)
        {
            int removed = setList.RemoveAll(s => hiddenSetNames.Contains(s.setName));
            Logger.Log($"[Encyclopedia] {removed}개 세트 숨김 처리");
        }

        // ★ 제거 후 정렬
        setList.Sort((a, b) =>
            string.Compare(a.setName, b.setName, StringComparison.OrdinalIgnoreCase));

        PopulateScrollView();
    }
    void LoadSetBonusDefinitions()
    {
        setDefinitions.Clear();

        SetBonusDefinition[] loaded =
            Resources.LoadAll<SetBonusDefinition>(setBonusFolderPath);

        if (loaded == null || loaded.Length == 0)
        {
            Logger.LogWarning($"[Encyclopedia] '{setBonusFolderPath}'에서 " +
                              "SetBonusDefinition SO를 찾을 수 없습니다.");
            return;
        }

        setDefinitions.AddRange(loaded);
        Logger.Log($"[Encyclopedia] SetBonusDefinition {setDefinitions.Count}개 로드 완료");
    }

    // ── 플레이어 보유 아이템 수집 ─────────────────────────
    void BuildAcquiredSet()
    {
        acquiredItemNames.Clear();

        CardDataManager cdm = FindObjectOfType<CardDataManager>();
        if (cdm == null) return;

        foreach (CardData card in cdm.GetMyCardList())
        {
            if (card == null) continue;
            if (card.Type == CardType.Item.ToString())
                acquiredItemNames.Add(card.Name);
        }

        Logger.Log($"[Encyclopedia] 보유 아이템 {acquiredItemNames.Count}종 확인");
    }

    // ── itemPool.txt 파싱 ─────────────────────────────────
    void BuildSetData()
    {
        if (itemPoolDataBase == null)
        {
            Logger.LogError("[Encyclopedia] itemPoolDataBase가 할당되지 않았습니다.");
            return;
        }

        string normalized = itemPoolDataBase.text
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

        string[] lines = normalized.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            string[] c = line.Split('\t');
            if (c.Length < 15) continue;
            if (c[1].Trim() != "Item") continue;

            if (!int.TryParse(c[2].Trim(), out int grade)) continue;
            if (grade < 0 || grade > 4) continue;

            string itemName = c[4].Trim();
            int hp = ParseIntSafe(c[6]);
            int atk = ParseIntSafe(c[7]);
            string slotStr = c[8].Trim();
            bool isEss = c[9].Trim() == "Essential";
            string setName = c[14].Trim();

            if (string.IsNullOrEmpty(itemName) || string.IsNullOrEmpty(setName)) continue;

            int slotIdx = Array.IndexOf(SLOT_ORDER, slotStr);
            if (slotIdx < 0) continue;

            if (!setMap.TryGetValue(setName, out EncycSetInfo setInfo))
            {
                setInfo = new EncycSetInfo(setName);
                setMap[setName] = setInfo;
                setList.Add(setInfo);
            }

            var slotList = setInfo.slotItems[slotIdx];
            EncycItemInfo item = slotList.Find(i => i.internalName == itemName);
            if (item == null)
            {
                item = new EncycItemInfo
                {
                    internalName = itemName,
                    slot = SlotStringToEnum(slotStr),
                    isEssential = isEss,
                    itemSO = grade == 0
                                   ? cardsDictionary?.GetItemByName(itemName, 0)
                                   : null
                };
                slotList.Add(item);
            }

            item.hp[grade] = hp;
            item.atk[grade] = atk;

            if (grade == 0 && item.itemSO == null)
                item.itemSO = cardsDictionary?.GetItemByName(itemName, 0);
        }

        setList.Sort((a, b) =>
            string.Compare(a.setName, b.setName, StringComparison.OrdinalIgnoreCase));

        Logger.Log($"[Encyclopedia] {setList.Count}개 세트 로드 완료");
    }

    // ── ScrollView 채우기 ─────────────────────────────────
    // PopulateScrollView() 수정 — 엔트리 참조 저장
    void PopulateScrollView()
    {
        foreach (Transform child in scrollContent) Destroy(child.gameObject);
        spawnedEntries.Clear(); // ★

        foreach (EncycSetInfo info in setList)
        {
            GameObject go = Instantiate(setEntryPrefab, scrollContent);
            var entry = go.GetComponent<EncyclopediaSetEntry>();
            if (entry != null)
            {
                entry.Init(info, acquiredItemNames, OnEntryTapped);
                spawnedEntries.Add(entry); // ★
            }
        }
    }

    // ★ 새로 추가 — 보유 목록만 다시 읽고 모든 엔트리 갱신
    public void Refresh()
    {
        BuildAcquiredSet();

        for (int i = 0; i < spawnedEntries.Count; i++)
        {
            if (spawnedEntries[i] != null)
                spawnedEntries[i].Refresh(acquiredItemNames);
        }
    }

    void OnEntryTapped(EncycSetInfo info)
    {
        SetBonusDefinition bonus = setDefinitions?.Find(s => s.setName == info.setName);
        popup.Show(info, bonus, acquiredItemNames); // ★ acquiredItemNames 추가
    }

    static int ParseIntSafe(string s) { int.TryParse(s?.Trim(), out int v); return v; }

    static EquipmentType SlotStringToEnum(string s) => s switch
    {
        "Head" => EquipmentType.Head,
        "Face" => EquipmentType.Face,
        "Chest" => EquipmentType.Chest,
        "Hand" => EquipmentType.Hand,
        _ => EquipmentType.Head
    };
}