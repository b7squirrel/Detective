using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Resources/Weapons/Friends 폴더의 WeaponData 이름과 1:1로 맞춘 목록.
/// 새 오리가 추가되면 여기에도 항목을 추가해야 드롭다운에 나타난다.
/// </summary>
public enum DuckName
{
    Cannon,
    Bow,
    Bowling,
    Cat,
    Cowboy,
    Guitar,
    Hammer,
    Yoyo,
    Laser,
    Nano,
    Origami,
    Whistle,
    Tennis,
    Tesla,
    Volley,
    Zap,
    Arc,
}

/// <summary>
/// 트레일러 캡처 전용: 인스펙터에서 고른 특정 오리 하나의 기본 장비(Default Items)를 그대로 보여준다.
/// ShowcaseRandomizer와 달리 랜덤 조합이 아니라, WeaponData.defaultItems[]에 세팅된
/// "이 오리의 정식 기본 룩"을 그대로 재현하는 용도.
/// 반드시 "Char Image" 오브젝트에는 기존 카드와 동일하게 Animator + CardSpriteAnim 컴포넌트가 있어야 한다.
/// </summary>
public class SpecificDuckShowcase : MonoBehaviour
{
    [Header("보여줄 오리")]
    [SerializeField] DuckName duck;

    [Tooltip("같은 오리라도 폴더 안에 YoyoF_0 / YoyoF_1 / YoyoF_2 처럼 서로 다른 기본 장비 버리에이션이 여러 개 있을 수 있다. " +
        "그 인덱스(=WeaponData의 Grade 값)를 지정한다. 존재하지 않는 값이면 가장 가까운 낮은 버리에이션으로 대체된다.")]
    [SerializeField] int variant = 0;

    [Header("표시 대상")]
    [SerializeField] Animator charAnim;
    [SerializeField] Image charImage;
    [SerializeField] Image charFaceImage;
    [SerializeField] GameObject charFaceExpression;
    [SerializeField] RectTransform headMain;
    [SerializeField] Image[] equipmentImages = new Image[4];

    [Tooltip("equipmentImages 배열의 인덱스가 어떤 부위인지 매칭. 반드시 기존 EquipmentSlotsManager와 동일한 순서로 맞춰야 함")]
    [SerializeField] EquipmentType[] slotEquipmentTypes = new EquipmentType[4];

    [Header("Resources 오리 폴더")]
    [SerializeField] string friendsResourcesPath = "Weapons/Friends";

    CardSpriteAnim cardSpriteAnim;

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
        Apply();
    }

    [ContextMenu("지금 적용")]
    public void Apply()
    {
        cardSpriteAnim = charImage.GetComponent<CardSpriteAnim>();
        if (cardSpriteAnim == null)
        {
            Debug.LogError("[SpecificDuckShowcase] Char Image 오브젝트에 CardSpriteAnim 컴포넌트가 없습니다.");
            return;
        }
        cardSpriteAnim.Init(equipmentImages);

        WeaponData weapon = FindWeapon(duck, variant);
        if (weapon == null)
        {
            Debug.LogError($"[SpecificDuckShowcase] '{duck}' 이름의 WeaponData를 '{friendsResourcesPath}'에서 찾지 못했습니다.");
            return;
        }

        if (weapon.Animators == null || weapon.Animators.CardImageAnim == null)
        {
            Debug.LogError($"[SpecificDuckShowcase] {weapon.Name}의 CardImageAnim이 비어있습니다.");
            return;
        }

        if (headMain != null) headMain.anchoredPosition = Vector2.zero;

        charAnim.enabled = true;
        charAnim.gameObject.SetActive(true);
        charAnim.runtimeAnimatorController = weapon.Animators.CardImageAnim;
        charAnim.Update(0f);

        if (charFaceExpression != null) charFaceExpression.SetActive(true);
        if (charFaceImage != null) charFaceImage.sprite = weapon.faceImage;
        charImage.SetNativeSize();

        if (weapon.defaultItems == null)
        {
            Debug.LogWarning($"[SpecificDuckShowcase] {weapon.Name}의 Default Items가 비어있습니다.");
        }

        for (int i = 0; i < equipmentImages.Length; i++)
        {
            EquipmentType type = slotEquipmentTypes[i];

            Item item = null;
            if (weapon.defaultItems != null
                && defaultItemIndexMap.TryGetValue(type, out int idx)
                && idx < weapon.defaultItems.Length)
            {
                item = weapon.defaultItems[idx];
            }

            if (!HasValidSpriteRow(item))
            {
                equipmentImages[i].gameObject.SetActive(false);
                cardSpriteAnim.StoreItemSpriteRow(i, null);
                continue;
            }

            equipmentImages[i].gameObject.SetActive(true);

            if (headMain != null && item.needToOffset)
            {
                headMain.anchoredPosition = headMain.anchoredPosition == Vector2.zero
                    ? headMain.anchoredPosition + item.posHead
                    : headMain.anchoredPosition;
            }

            cardSpriteAnim.StoreItemSpriteRow(i, item.spriteRow);
            Debug.Log($"[SpecificDuckShowcase] {weapon.Name} → {type} = {item.Name}");
        }

        // Animation Event가 재생되며 자연스럽게 호출되길 기다리면 첫 프레임에 흰 박스가 노출될 수 있어서
        // 즉시 프레임 0 스프라이트를 강제 반영한다.
        cardSpriteAnim.SetEquippedItemSprite(0);

        for (int i = 0; i < equipmentImages.Length; i++)
        {
            if (equipmentImages[i].gameObject.activeSelf && equipmentImages[i].sprite == null)
            {
                equipmentImages[i].gameObject.SetActive(false);
            }
        }
    }

    WeaponData FindWeapon(DuckName targetDuck, int targetVariant)
    {
        WeaponData[] all = Resources.LoadAll<WeaponData>(friendsResourcesPath);
        List<WeaponData> matches = all
            .Where(w => w != null && w.Name == targetDuck.ToString())
            .OrderBy(w => w.grade)
            .ToList();

        if (matches.Count == 0) return null;

        Debug.Log($"[SpecificDuckShowcase] {targetDuck} 사용 가능한 버리에이션: {string.Join(", ", matches.Select(w => w.grade))}");

        WeaponData exact = matches.FirstOrDefault(w => w.grade == targetVariant);
        if (exact != null) return exact;

        // 정확히 일치하는 버리에이션이 없으면 그보다 낮은 것 중 가장 가까운 것, 그것도 없으면 가장 낮은 것
        WeaponData fallback = matches.LastOrDefault(w => w.grade < targetVariant) ?? matches.First();
        Debug.LogWarning($"[SpecificDuckShowcase] {targetDuck}의 버리에이션 {targetVariant}이 없어서 {fallback.grade}로 대체합니다.");
        return fallback;
    }

    static bool HasValidSpriteRow(Item item)
    {
        return item != null
            && item.spriteRow != null
            && item.spriteRow.sprites != null
            && item.spriteRow.sprites.Length > 0
            && item.spriteRow.sprites[0] != null;
    }
}