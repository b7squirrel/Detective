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
    [Tooltip("체크 해제하면 오리+장비 조합을 딱 한 번만 랜덤으로 뽑고 그 뒤로는 계속 그대로 유지한다 (전환 없음).")]
    [SerializeField] bool loopForever = true;
    [SerializeField] float startInterval = 0.3f;
    [SerializeField] float minInterval = 0.08f;
    [Tooltip("startInterval에서 minInterval까지 가속되는 데 걸리는 시간")]
    [SerializeField] float accelerateDuration = 8f;
    [SerializeField] AnimationCurve accelerateCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    CardSpriteAnim cardSpriteAnim;
    List<WeaponData> weaponPool;
    Dictionary<EquipmentType, List<Item>> itemPoolByType;
    Dictionary<(string Name, int Grade), CardData> itemCsvLookup;   // itemPoolCsv를 (이름,등급) 기준으로 바로 찾기 위한 캐시
    Dictionary<(string Name, int Grade), Item> itemByNameGrade;     // Resources의 실제 Item 에셋을 (이름,등급) 기준으로 찾기 위한 캐시
    Dictionary<(string DuckName, int Grade), List<CardData>> essentialByDuck; // CSV BindingTo 기준: 이 오리(등급)의 필수 아이템 후보들 (Baby/Teen처럼 여러 개일 수 있음)

    Coroutine loopRoutine;

    List<WeaponData> weaponDeck; // 셔플된 오리 순서. 다 소비하면 다시 셔플해서 채운다 (모든 오리가 골고루 나오도록)
    int deckIndex;

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

        if (loopForever)
        {
            loopRoutine = StartCoroutine(RunShowcase());
        }
        else
        {
            // 한 번만 뽑고 고정: 같은 필수템/랜덤 로직을 그대로 타되, 반복 전환은 하지 않는다.
            Randomize();
        }
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

        // Animator가 재생을 멈추면(=null 컨트롤러) Image.sprite가 갱신될 방법이 없어서
        // 흰 박스로 남고 애니메이션도 정지한다. 그래서 애초에 CardImageAnim이 비어있는
        // 항목은 후보에서 제외한다. (같은 이름 중 "최고 등급"을 고를 때도 유효한 것만 고려)
        List<WeaponData> invalidAnimWeapons = allWeapons
            .Where(w => w.Animators == null || w.Animators.CardImageAnim == null)
            .ToList();
        if (invalidAnimWeapons.Count > 0)
        {
            Debug.LogWarning($"[ShowcaseRandomizer] CardImageAnim이 비어있어 제외된 오리 {invalidAnimWeapons.Count}개: "
                + string.Join(", ", invalidAnimWeapons.Select(w => $"{w.Name}(Grade {w.grade})")));
        }

        weaponPool = allWeapons
            .Where(w => w.Animators != null && w.Animators.CardImageAnim != null)
            .GroupBy(w => w.Name)
            .Select(g => g.OrderByDescending(w => w.grade).First())
            .ToList();

        Debug.Log($"[ShowcaseRandomizer] 오리 {allWeapons.Count}개 중 이름별 최고 등급(애니메이터 유효한 것만) {weaponPool.Count}개 채택");
        Debug.Log($"[ShowcaseRandomizer] 채택된 오리 목록: {string.Join(", ", weaponPool.Select(w => w.Name).OrderBy(n => n))}");
#else
        Debug.LogError("[ShowcaseRandomizer] 이 기능은 에디터 Play 모드 전용입니다.");
#endif

        // ---- 아이템 CSV 파싱: (이름, 등급) 기준으로 EquipmentType / EssentialEquip 조회용 ----
        itemCsvLookup = new Dictionary<(string, int), CardData>();
        essentialByDuck = new Dictionary<(string, int), List<CardData>>();
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

                // BindingTo가 이 아이템이 전용으로 묶여있는 "오리 이름"이다.
                // 예: Arc Baby Raptor / Arc Teen Raptor 둘 다 BindingTo="Arc"인 것처럼,
                // 같은 (오리, 등급)에 후보가 여러 개(Baby/Teen 등) 있을 수 있어서 리스트로 모은다.
                if (row.EssentialEquip == "Essential" && !string.IsNullOrEmpty(row.BindingTo))
                {
                    var key = (row.BindingTo, row.Grade);
                    if (!essentialByDuck.TryGetValue(key, out List<CardData> list))
                    {
                        list = new List<CardData>();
                        essentialByDuck[key] = list;
                    }
                    list.Add(row);
                }
            }
            Debug.Log($"[ShowcaseRandomizer] 아이템 CSV {csvRows.Count}행 로드 완료, 필수템 매핑 {essentialByDuck.Count}개 키");
        }

        // ---- 장비(Item) : Resources에서 로드 ----
        Item[] allItems = Resources.LoadAll<Item>(itemResourcesPath);

        // 이름+등급으로 정확한 에셋을 바로 찾기 위한 캐시 (필수 아이템 조회용, 등급 dedup 이전 전체 목록 기준)
        itemByNameGrade = new Dictionary<(string, int), Item>();
        foreach (Item item in allItems)
        {
            itemByNameGrade[(item.Name, item.grade)] = item;
        }

        // ---- 이름별 최고 grade만 채택, 부위별로 버킷 (일반 랜덤 풀용) ----
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
            // 필수 슬롯에는 아래 Randomize()에서 CSV의 BindingTo 매핑(essentialByDuck)으로 강제 지정함
            if (cd.EssentialEquip == "Essential")
            {
                continue;
            }

            // spriteRow가 비어있으면 CardSpriteAnim이 sprite를 갱신 못 해서 흰 박스로 남는다.
            // 애초에 유효한 스프라이트가 있는 아이템만 풀에 넣는다.
            if (!HasValidSpriteRow(item))
            {
                Debug.LogWarning($"[ShowcaseRandomizer] spriteRow가 비어있어 제외됨: {item.Name} (Grade {item.grade})");
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

    [ContextMenu("다시 뽑기")]
    void Randomize()
    {
        // ---- 오리 ----
        WeaponData weapon = GetNextWeapon();

        // 안전장치: LoadPools에서 이미 걸렀지만, 혹시 모를 null 컨트롤러로 인한
        // "애니메이션 정지 + 흰 박스"를 다시 한번 방지.
        if (weapon.Animators == null || weapon.Animators.CardImageAnim == null)
        {
            Debug.LogWarning($"[ShowcaseRandomizer] {weapon.Name}의 CardImageAnim이 비어있어 이번 전환을 건너뜁니다.");
            return;
        }

        if (headMain != null) headMain.anchoredPosition = Vector2.zero;

        charAnim.enabled = true;
        charAnim.gameObject.SetActive(true);
        charAnim.runtimeAnimatorController = weapon.Animators.CardImageAnim;

        // 이전 오리와 같은 Animator Controller를 공유하는 경우, Unity가 "이미 같은 컨트롤러"라고 판단해서
        // 상태를 리셋하지 않고 이전 재생 위치를 그대로 이어갈 수 있다.
        // charAnim.Rebind();
        charAnim.Update(0f);

        if (charFaceExpression != null) charFaceExpression.SetActive(true);
        if (charFaceImage != null) charFaceImage.sprite = weapon.faceImage;
        charImage.SetNativeSize();

        // 이 오리의 필수 슬롯 & 필수 아이템을 CSV(BindingTo)에서 찾는다.
        // weapon.equipmentType / weapon.defaultItems[]는 SO에 잘못 세팅된 경우가 있어서 신뢰하지 않는다.
        EquipmentType essentialSlot = default;
        Item essentialItem = null;
        bool hasEssentialSlot = false;

        if (essentialByDuck.TryGetValue((weapon.Name, weapon.grade), out List<CardData> essentialCandidates) && essentialCandidates.Count > 0)
        {
            // 후보가 여러 개(Baby/Teen 등)면 그중 하나를 랜덤으로 선택 - 트레일러 다양성에도 도움이 됨
            CardData essentialRow = essentialCandidates[Random.Range(0, essentialCandidates.Count)];

            if (System.Enum.TryParse(essentialRow.EquipmentType, out essentialSlot))
            {
                hasEssentialSlot = true;

                if (itemByNameGrade.TryGetValue((essentialRow.Name, essentialRow.Grade), out Item foundItem))
                {
                    if (HasValidSpriteRow(foundItem))
                    {
                        essentialItem = foundItem;
                    }
                    else
                    {
                        Debug.LogWarning($"[ShowcaseRandomizer] {weapon.Name}의 필수 아이템 {foundItem.Name}의 spriteRow가 비어있음");
                    }
                }
                else
                {
                    Debug.LogWarning($"[ShowcaseRandomizer] {weapon.Name}의 필수 아이템 {essentialRow.Name}(Grade {essentialRow.Grade})을 Resources에서 찾지 못함");
                }
            }
        }
        else
        {
            Debug.LogWarning($"[ShowcaseRandomizer] {weapon.Name}(Grade {weapon.grade})의 필수 슬롯 정보를 CSV에서 찾지 못함");
        }

        // ---- 장비 4부위 ----
        for (int i = 0; i < equipmentImages.Length; i++)
        {
            EquipmentType type = slotEquipmentTypes[i];
            Item item;

            if (hasEssentialSlot && type == essentialSlot && essentialItem != null)
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

            Debug.Log($"[ShowcaseRandomizer] {weapon.Name} → {type} = {item.Name}{(hasEssentialSlot && type == essentialSlot ? " (필수)" : "")}");
        }

        // Animation Event(SetEquippedItemSprite)가 재생되며 자연스럽게 호출되길 기다리면
        // 전환 주기가 짧을 때(가속 후) 이벤트가 발동되기 전에 다음 전환이 와서
        // Image.sprite가 비어있는 채로 "흰 사각형"이 노출된다.
        // 그래서 4부위를 다 세팅한 직후 프레임 0 스프라이트를 즉시 강제 반영한다.
        cardSpriteAnim.SetEquippedItemSprite(0);

        // 안전장치: 위 과정을 거치고도 sprite가 비어있다면 흰 박스를 보여주는 대신 그냥 숨긴다.
        for (int i = 0; i < equipmentImages.Length; i++)
        {
            if (equipmentImages[i].gameObject.activeSelf && equipmentImages[i].sprite == null)
            {
                equipmentImages[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 아이템의 spriteRow가 실제로 렌더링 가능한 스프라이트를 갖고 있는지 확인.
    /// 비어있으면 CardSpriteAnim이 Image.sprite를 갱신하지 못해 흰 박스가 남는다.
    /// </summary>
    static bool HasValidSpriteRow(Item item)
    {
        return item != null
            && item.spriteRow != null
            && item.spriteRow.sprites != null
            && item.spriteRow.sprites.Length > 0
            && item.spriteRow.sprites[0] != null;
    }

    /// <summary>
    /// 셔플된 덱에서 오리를 하나씩 순서대로 꺼낸다. 덱을 다 쓰면 다시 셔플해서 채운다.
    /// 이렇게 하면 순수 랜덤과 달리, 매 (오리 수)번의 전환마다 모든 오리가 반드시 한 번씩 나온다.
    /// </summary>
    WeaponData GetNextWeapon()
    {
        if (weaponDeck == null || deckIndex >= weaponDeck.Count)
        {
            WeaponData previousLast = (weaponDeck != null && weaponDeck.Count > 0)
                ? weaponDeck[weaponDeck.Count - 1]
                : null;

            weaponDeck = new List<WeaponData>(weaponPool);
            Shuffle(weaponDeck);

            // 이전 덱의 마지막과 새 덱의 첫 번째가 같으면 연속으로 같은 오리가 나오니 한 번 섞어준다
            if (previousLast != null && weaponDeck.Count > 1 && weaponDeck[0] == previousLast)
            {
                (weaponDeck[0], weaponDeck[1]) = (weaponDeck[1], weaponDeck[0]);
            }

            deckIndex = 0;
        }

        WeaponData weapon = weaponDeck[deckIndex];
        deckIndex++;
        return weapon;
    }

    static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

}