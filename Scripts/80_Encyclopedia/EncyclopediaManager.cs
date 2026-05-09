// ============================================================
//  파일 1 of 4 : EncyclopediaManager.cs
//  역할 : itemPool.txt 파싱 → SetInfo 빌드 → ScrollView 생성
// ============================================================
using System;
using System.Collections.Generic;
using UnityEngine;
 
// ── 런타임 데이터 클래스 ──────────────────────────────────────
/// <summary>슬롯 하나에 놓이는 아이템 1종의 런타임 정보</summary>
public class EncycItemInfo
{
    public string internalName;      // "Archer Club Bow"
    public EquipmentType slot;       // Head / Face / Chest / Hand
    public bool   isEssential;
    public int[]  hp  = new int[5]; // grade 0-4
    public int[]  atk = new int[5]; // grade 0-4
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
    [SerializeField] TextAsset                  itemPoolDataBase;
    [SerializeField] List<SetBonusDefinition>   setDefinitions;
 
    [Header("UI")]
    [SerializeField] Transform          scrollContent;   // ScrollView > Viewport > Content
    [SerializeField] GameObject         setEntryPrefab;  // EncyclopediaSetEntry 프리팹
    [SerializeField] EncyclopediaPopup  popup;
 
    // 슬롯 표시 순서 (TSV col[8] 값과 일치해야 함)
    static readonly string[] SLOT_ORDER = { "Head", "Face", "Chest", "Hand" };
 
    List<EncycSetInfo>              setList = new List<EncycSetInfo>();
    Dictionary<string, EncycSetInfo> setMap = new Dictionary<string, EncycSetInfo>();
 
    // ── 진입점 ──────────────────────────────────────────────
    void Start()
    {
        BuildSetData();
        PopulateScrollView();
    }
 
    // ── itemPool.txt 파싱 ────────────────────────────────────
    void BuildSetData()
    {
        if (itemPoolDataBase == null)
        {
            Logger.LogError("[Encyclopedia] itemPoolDataBase가 할당되지 않았습니다.");
            return;
        }
 
        string normalized = itemPoolDataBase.text
            .Replace("\r\n", "\n")
            .Replace("\r",   "\n");
 
        string[] lines = normalized.Split('\n', StringSplitOptions.RemoveEmptyEntries);
 
        foreach (string line in lines)
        {
            string[] c = line.Split('\t');
            if (c.Length < 15)                 continue;
            if (c[1].Trim() != "Item")         continue;
 
            if (!int.TryParse(c[2].Trim(), out int grade)) continue;
            if (grade < 0 || grade > 4)        continue;
 
            string itemName = c[4].Trim();
            int    hp       = ParseIntSafe(c[6]);
            int    atk      = ParseIntSafe(c[7]);
            string slotStr  = c[8].Trim();
            bool   isEss    = c[9].Trim() == "Essential";
            string setName  = c[14].Trim(); // ★ Trim으로 후행 공백 제거
 
            if (string.IsNullOrEmpty(itemName) || string.IsNullOrEmpty(setName)) continue;
 
            int slotIdx = Array.IndexOf(SLOT_ORDER, slotStr);
            if (slotIdx < 0) continue; // Ori 등 미지원 슬롯 스킵
 
            // 세트 생성/조회
            if (!setMap.TryGetValue(setName, out EncycSetInfo setInfo))
            {
                setInfo = new EncycSetInfo(setName);
                setMap[setName] = setInfo;
                setList.Add(setInfo);
            }
 
            // 아이템 생성/조회 (같은 슬롯에 여러 아이템 허용 e.g. Laser Baby Rex + Teen Rex)
            var slotList = setInfo.slotItems[slotIdx];
            EncycItemInfo item = slotList.Find(i => i.internalName == itemName);
            if (item == null)
            {
                item = new EncycItemInfo
                {
                    internalName = itemName,
                    slot         = SlotStringToEnum(slotStr),
                    isEssential  = isEss
                };
                slotList.Add(item);
            }
 
            item.hp[grade]  = hp;
            item.atk[grade] = atk;
        }
 
        // 알파벳순 정렬
        setList.Sort((a, b) =>
            string.Compare(a.setName, b.setName, StringComparison.OrdinalIgnoreCase));
 
        Logger.Log($"[Encyclopedia] {setList.Count}개 세트 로드 완료");
    }
 
    // ── ScrollView 채우기 ────────────────────────────────────
    void PopulateScrollView()
    {
        // 기존 자식 제거
        foreach (Transform child in scrollContent)
            Destroy(child.gameObject);
 
        foreach (EncycSetInfo info in setList)
        {
            GameObject go    = Instantiate(setEntryPrefab, scrollContent);
            var        entry = go.GetComponent<EncyclopediaSetEntry>();
            if (entry != null)
                entry.Init(info, OnEntryTapped);
        }
    }
 
    // ── 팝업 열기 ────────────────────────────────────────────
    void OnEntryTapped(EncycSetInfo info)
    {
        SetBonusDefinition bonus = setDefinitions?.Find(s => s.setName == info.setName);
        popup.Show(info, bonus);
    }
 
    // ── 유틸 ─────────────────────────────────────────────────
    static int ParseIntSafe(string s)
    {
        int.TryParse(s?.Trim(), out int v);
        return v;
    }
 
    static EquipmentType SlotStringToEnum(string s) => s switch
    {
        "Head"  => EquipmentType.Head,
        "Face"  => EquipmentType.Face,
        "Chest" => EquipmentType.Chest,
        "Hand"  => EquipmentType.Hand,
        _       => EquipmentType.Head
    };
}