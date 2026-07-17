using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 트레일러 캡처 전용: 오리 + 4부위 장비를 랜덤하게 빠르게 스와핑해서 보여준다.
/// Card Base Container / Level / Stars / Button 등 부가 요소는 아예 포함하지 않는다.
/// 반드시 "Char Image" 오브젝트에는 기존 카드와 동일하게 Animator + CardSpriteAnim 컴포넌트가 있어야 한다.
/// (애니메이션 클립에 박혀있는 SetEquippedItemSprite 이벤트가 장비 스프라이트를 자동으로 갱신해줌)
/// </summary>
public class ShowcaseRandomizer : MonoBehaviour
{
    [Header("표시 대상")]
    [SerializeField] Animator charAnim;
    [SerializeField] Image charImage;                 // Animator가 sprite를 바꾸는 그 Image
    [SerializeField] Image charFaceImage;
    [SerializeField] GameObject charFaceExpression;
    [SerializeField] RectTransform headMain;           // 아이템 offset 보정용 (없으면 비워둬도 됨)
    [SerializeField] Image[] equipmentImages = new Image[4];

    [Tooltip("equipmentImages 배열의 인덱스가 어떤 부위인지 매칭. 반드시 기존 EquipmentSlotsManager와 동일한 순서로 맞춰야 함")]
    [SerializeField] EquipmentType[] slotEquipmentTypes = new EquipmentType[4];

    [Header("에디터 전용: 오리 데이터 폴더")]
    [SerializeField] string weaponFolder = "Assets/Data/Weapons_Items/01_Weapon";

    [Header("Resources 아이템 폴더")]
    [SerializeField] string itemResourcesPath = "03_Equipment";

    [Header("아이템 CSV (Essential 태그 판별용, GachaSystem의 itemPoolDatabase와 동일한 파일)")]
    [SerializeField] TextAsset itemPoolCsv;

    [Header("전환 주기 (초)")]
    [SerializeField] float startInterval = 0.3f;
    [SerializeField] float minInterval = 0.08f;
    [Tooltip("startInterval에서 minInterval까지 가속되는 데 걸리는 시간")]
    [SerializeField] float accelerateDuration = 8f;
    [SerializeField] AnimationCurve accelerateCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    CardSpriteAnim cardSpriteAnim;
    List<WeaponData> weaponPool;
    Dictionary<EquipmentType, List<Item>> itemPoolByType;
    Dictionary<(string Name, int Grade), CardData> itemCsvLookup; // itemPoolCsv를 (이름,등급) 기준으로 바로 찾기 위한 캐시

    Coroutine loopRoutine;

    // WeaponData.defaultItems[] 인덱스 고정 순서 (GachaSystem.defaultEquipIndex와 동일하게 맞춤)
    static readonly Dictionary<EquipmentType, int> defaultItemIndexMap = new Dictionary<EquipmentType, int>
    {
        { EquipmentType.Head, 0 },
        { EquipmentType.Chest, 1 },
        { EquipmentType.Face, 2 },
        { EquipmentType.Hand, 3 },
    };

    void Start()
    {
        LoadPools();

        cardSpriteAnim = charImage.GetComponent<CardSpriteAnim>();
        if (cardSpriteAnim == null)
        {
            Debug.LogError("[ShowcaseRandomizer] Char Image 오브젝트에 CardSpriteAnim 컴포넌트가 없습니다. 기존 카드 프리팹처럼 붙여주세요.");
            return;
        }
        cardSpriteAnim.Init(equipmentImages);

        if (weaponPool == null || weaponPool.Count == 0)
        {
            Debug.LogError("[ShowcaseRandomizer] 오리 풀이 비어있습니다.");
            return;
        }

        loopRoutine = StartCoroutine(RunShowcase());
    }

    void OnDisable()
    {
        if (loopRoutine != null) StopCoroutine(loopRoutine);
    }

    void LoadPools()
    {
        // ---- 오리(WeaponData) : 에디터에서만, 폴더 전체를 긁어서 이름별 최고 grade만 채택 ----
        weaponPool = new List<WeaponData>();
#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets("t:WeaponData", new[] { weaponFolder });
        List<WeaponData> allWeapons = new List<WeaponData>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            WeaponData wd = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
            if (wd != null) allWeapons.Add(wd);
        }

        weaponPool = allWeapons
            .GroupBy(w => w.Name)
            .Select(g => g.OrderByDescending(w => w.grade).First())
            .ToList();

        Debug.Log($"[ShowcaseRandomizer] 오리 {allWeapons.Count}개 중 이름별 최고 등급 {weaponPool.Count}개 채택");
#else
        Debug.LogError("[ShowcaseRandomizer] 이 기능은 에디터 Play 모드 전용입니다.");
#endif

        // ---- 아이템 CSV 파싱: (이름, 등급) 기준으로 EquipmentType / EssentialEquip 조회용 ----
        itemCsvLookup = new Dictionary<(string, int), CardData>();
        if (itemPoolCsv == null)
        {
            Debug.LogError("[ShowcaseRandomizer] Item Pool Csv가 연결되지 않았습니다. itemPool.txt를 인스펙터에 연결해주세요.");
        }
        else
        {
            List<CardData> csvRows = new ReadCardData().GetCardsList(itemPoolCsv);
            foreach (CardData row in csvRows)
            {
                itemCsvLookup[(row.Name, row.Grade)] = row;
            }
            Debug.Log($"[ShowcaseRandomizer] 아이템 CSV {csvRows.Count}행 로드 완료");
        }

        // ---- 장비(Item) : Resources에서 로드, 이름별 최고 grade만 채택, 부위별로 버킷 ----
        Item[] allItems = Resources.LoadAll<Item>(itemResourcesPath);
        List<Item> maxGradeItems = allItems
            .GroupBy(i => i.Name)
            .Select(g => g.OrderByDescending(i => i.grade).First())
            .ToList();

        itemPoolByType = new Dictionary<EquipmentType, List<Item>>();
        foreach (EquipmentType t in System.Enum.GetValues(typeof(EquipmentType)))
        {
            itemPoolByType[t] = new List<Item>();
        }

        int failCount = 0;
        foreach (Item item in maxGradeItems)
        {
            if (!itemCsvLookup.TryGetValue((item.Name, item.grade), out CardData cd) || string.IsNullOrEmpty(cd.EquipmentType))
            {
                failCount++;
                continue;
            }

            // 특정 오리 전용 필수 장비는 일반 랜덤 풀에서 제외 (엉뚱한 오리에 섞이는 것 방지)
            // 필수 슬롯에는 아래 Randomize()에서 weapon.defaultItems[]로 강제 지정함
            if (cd.EssentialEquip == "Essential")
            {
                continue;
            }

            if (System.Enum.TryParse(cd.EquipmentType, out EquipmentType type))
            {
                itemPoolByType[type].Add(item);
            }
            else
            {
                Debug.LogWarning($"[ShowcaseRandomizer] EquipmentType 파싱 실패: {cd.EquipmentType} ({item.Name})");
            }
        }

        if (failCount > 0)
            Debug.LogWarning($"[ShowcaseRandomizer] CSV에서 매칭되는 행을 찾지 못한 아이템 {failCount}개는 제외됨 (이름/등급 불일치 가능성)");

        foreach (var kv in itemPoolByType)
            Debug.Log($"[ShowcaseRandomizer] {kv.Key} 부위 아이템 {kv.Value.Count}개");
    }

    IEnumerator RunShowcase()
    {
        float elapsed = 0f;
        while (true)
        {
            Randomize();

            float t = accelerateDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / accelerateDuration);
            float curved = accelerateCurve.Evaluate(t);
            float interval = Mathf.Lerp(startInterval, minInterval, curved);

            elapsed += interval;
            yield return new WaitForSeconds(interval);
        }
    }

    void Randomize()
    {
        // ---- 오리 ----
        WeaponData weapon = weaponPool[Random.Range(0, weaponPool.Count)];

        if (headMain != null) headMain.anchoredPosition = Vector2.zero;

        charAnim.enabled = true;
        charAnim.gameObject.SetActive(true);
        charAnim.runtimeAnimatorController = weapon.Animators.CardImageAnim;

        // 이전 오리와 같은 Animator Controller를 공유하는 경우, Unity가 "이미 같은 컨트롤러"라고 판단해서
        // 상태를 리셋하지 않고 이전 재생 위치를 그대로 이어갈 수 있다.
        // Rebind + 즉시 Update로 항상 깨끗한 초기 상태부터 다시 재생되도록 강제한다.
        charAnim.Rebind();
        charAnim.Update(0f);

        if (charFaceExpression != null) charFaceExpression.SetActive(true);
        if (charFaceImage != null) charFaceImage.sprite = weapon.faceImage;
        charImage.SetNativeSize();

        // 이 오리의 필수 슬롯 & 필수 아이템 (예: Cowboy 오리는 항상 Hand에 Cowboy 전용 아이템)
        EquipmentType essentialSlot = weapon.equipmentType;
        Item essentialItem = GetEssentialItem(weapon);

        // ---- 장비 4부위 ----
        for (int i = 0; i < equipmentImages.Length; i++)
        {
            EquipmentType type = slotEquipmentTypes[i];
            Item item;

            if (type == essentialSlot && essentialItem != null)
            {
                // 필수 슬롯: 랜덤 뽑기 없이 이 오리의 고정 필수 아이템을 강제 지정
                item = essentialItem;
            }
            else
            {
                List<Item> pool = itemPoolByType[type];
                if (pool == null || pool.Count == 0)
                {
                    equipmentImages[i].gameObject.SetActive(false);
                    cardSpriteAnim.StoreItemSpriteRow(i, null);
                    continue;
                }
                item = pool[Random.Range(0, pool.Count)];
            }

            equipmentImages[i].gameObject.SetActive(true);

            if (headMain != null && item.needToOffset)
            {
                headMain.anchoredPosition = headMain.anchoredPosition == Vector2.zero
                    ? headMain.anchoredPosition + item.posHead
                    : headMain.anchoredPosition;
            }

            cardSpriteAnim.StoreItemSpriteRow(i, item.spriteRow);

            Debug.Log($"[ShowcaseRandomizer] {weapon.Name} → {type} = {item.Name}{(type == essentialSlot ? " (필수)" : "")}");
        }

        // Animation Event(SetEquippedItemSprite)가 재생되며 자연스럽게 호출되길 기다리면
        // 전환 주기가 짧을 때(가속 후) 이벤트가 발동되기 전에 다음 전환이 와서
        // Image.sprite가 비어있는 채로 "흰 사각형"이 노출된다.
        // 그래서 4부위를 다 세팅한 직후 프레임 0 스프라이트를 즉시 강제 반영한다.
        cardSpriteAnim.SetEquippedItemSprite(0);
    }

    /// <summary>
    /// 오리의 필수 슬롯(weapon.equipmentType)에 대응하는 defaultItems[] 항목을 반환.
    /// GachaSystem.AddEssentialEquip()과 동일한 규칙(Head=0, Chest=1, Face=2, Hand=3)을 따름.
    /// </summary>
    Item GetEssentialItem(WeaponData weapon)
    {
        if (weapon.defaultItems == null) return null;
        if (!defaultItemIndexMap.TryGetValue(weapon.equipmentType, out int idx)) return null;
        if (idx < 0 || idx >= weapon.defaultItems.Length) return null;
        return weapon.defaultItems[idx];
    }
}