// ============================================================
//  EncyclopediaPopup.cs  (교체 버전)
//  변경사항:
//    - 등급 탭 제거
//    - 아이템 목록 / HP / ATK 제거
//    - SetBonusDefinition 배열을 읽어 등급별 5줄 자동 생성
//  삭제 가능: EncyclopediaItemRow.cs (더 이상 사용 안 함)
// ============================================================
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
 
public class EncyclopediaPopup : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] TextMeshProUGUI setNameText;
 
    [Header("슬롯 이미지 (팝업)")]
    [SerializeField] Image[] popupSlotImages;     // 4개 — 아이템 Image
    [SerializeField] Image[] popupSlotCardImages; // 4개 — 카드 배경 Image
    [SerializeField] Sprite  acquiredCardSprite;
    [SerializeField] Sprite  unacquiredCardSprite;
    [SerializeField] Sprite  emptySlotSprite;
 
    [Header("팝업 배경")]
    [SerializeField] Image popupBackground;
    [SerializeField] Color colorComplete   = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] Color colorIncomplete = new Color(0.1f, 0.1f, 0.15f, 1f);
 
    [Header("Bonus Content")]
    [SerializeField] TextMeshProUGUI bonusContentText;
    [SerializeField] TextMeshProUGUI noBonusText;
 
    [Header("Close")]
    [SerializeField] Button closeButton;
 
    static readonly Color ITEM_ACQUIRED   = Color.white;
    static readonly Color ITEM_UNACQUIRED = new Color(1f, 1f, 1f, 0.3f);
    static readonly Color ITEM_EMPTY      = new Color(1f, 1f, 1f, 0.2f);
 
    void Awake()
    {
        // closeButton.onClick.AddListener(Hide);
        gameObject.SetActive(false);
    }
 
    // ── 공개 API ─────────────────────────────────────────────
    public void Show(EncycSetInfo info,
                     SetBonusDefinition bonus,
                     HashSet<string> acquiredNames)
    {
        setNameText.text = GetSetDisplayName(info);
 
        RefreshSlotImages(info, acquiredNames);
 
        if (bonus == null)
        {
            bonusContentText.gameObject.SetActive(false);
            noBonusText.gameObject.SetActive(true);
            noBonusText.text = "세트 효과가 아직 정의되지 않았습니다.";
        }
        else
        {
            noBonusText.gameObject.SetActive(false);
            bonusContentText.gameObject.SetActive(true);
            bonusContentText.text = BuildBonusText(bonus);
        }
 
        gameObject.SetActive(true);
    }
 
    void Hide() => gameObject.SetActive(false);
 
    // ── 슬롯 이미지 ──────────────────────────────────────────
    void RefreshSlotImages(EncycSetInfo info, HashSet<string> acquiredNames)
    {
        if (popupSlotImages == null) return;
 
        for (int i = 0; i < popupSlotImages.Length; i++)
        {
            if (popupSlotImages[i] == null) continue;
 
            var  items   = info.slotItems[i];
            bool hasDef  = items.Count > 0;
            EncycItemInfo first = hasDef ? items[0] : null;
            bool acquired = hasDef
                            && acquiredNames != null
                            && acquiredNames.Contains(first.internalName);
 
            // 카드 배경 스프라이트
            if (popupSlotCardImages != null && i < popupSlotCardImages.Length
                && popupSlotCardImages[i] != null)
            {
                popupSlotCardImages[i].sprite = (hasDef && acquired)
                    ? acquiredCardSprite
                    : unacquiredCardSprite;
            }
 
            // 아이템 이미지
            Sprite spr = first?.itemSO?.charImage;
            if (hasDef && spr != null)
            {
                popupSlotImages[i].sprite = spr;
                popupSlotImages[i].color  = acquired
                    ? ITEM_ACQUIRED
                    : ITEM_UNACQUIRED;
            }
            else
            {
                popupSlotImages[i].sprite = emptySlotSprite;
                popupSlotImages[i].color  = ITEM_EMPTY;
            }
        }
 
        // 팝업 배경색
        if (popupBackground != null)
        {
            bool allAcquired = true;
            for (int i = 0; i < 4; i++)
            {
                var items = info.slotItems[i];
                if (items.Count == 0) continue;
                if (acquiredNames == null
                    || !acquiredNames.Contains(items[0].internalName))
                {
                    allAcquired = false;
                    break;
                }
            }
            popupBackground.color = allAcquired ? colorComplete : colorIncomplete;
        }
    }
 
    // ── 세트 표시명 (static — EncycSetInfo 매개변수 사용) ─────
    static string GetSetDisplayName(EncycSetInfo info)
    {
        // 1차: Essential 아이템에서 탐색
        for (int i = 0; i < 4; i++)
        {
            foreach (var item in info.slotItems[i])
            {
                if (!item.isEssential || item.itemSO == null) continue;
 
                if (LocalizationManager.IsInitialized
                    && LocalizationManager.CurrentLanguage == Language.English)
                {
                    string eng = LocalizationManager.Item
                                    .GetSetDisplayName(item.internalName);
                    if (!string.IsNullOrEmpty(eng)) return eng;
                }
 
                if (!string.IsNullOrEmpty(item.itemSO.setDisplayName))
                    return item.itemSO.setDisplayName;
            }
        }
 
        // 2차: Essential 없는 세트 — 아무 아이템이나 탐색
        for (int i = 0; i < 4; i++)
        {
            foreach (var item in info.slotItems[i])
            {
                if (item.itemSO == null) continue;
 
                if (LocalizationManager.IsInitialized
                    && LocalizationManager.CurrentLanguage == Language.English)
                {
                    string eng = LocalizationManager.Item
                                    .GetSetDisplayName(item.internalName);
                    if (!string.IsNullOrEmpty(eng)) return eng;
                }
 
                if (!string.IsNullOrEmpty(item.itemSO.setDisplayName))
                    return item.itemSO.setDisplayName;
            }
        }
 
        // 최종 fallback
        return info.setName;
    }
 
    // ── 5줄 보너스 텍스트 ────────────────────────────────────
    static string BuildBonusText(SetBonusDefinition bonus)
    {
        var sb = new StringBuilder();
 
        for (int g = 0; g < StaticValues.MaxGrade; g++)
        {
            Color  col  = MyGrade.GradeColors[g];
            string hex  = ColorUtility.ToHtmlStringRGB(col);
            string name = MyGrade.mGrades[g];
 
            string statLine = CollectStats(bonus, g);
            if (string.IsNullOrEmpty(statLine)) statLine = "-";
 
            sb.Append($"<color=#{hex}>[{name}]</color>  {statLine}");
 
            if (g < StaticValues.MaxGrade - 1)
                sb.AppendLine();
        }
 
        return sb.ToString();
    }
 
    static string CollectStats(SetBonusDefinition b, int g)
    {
        var parts = new List<string>();
 
        // 퍼센트 스탯
        TryAdd(parts, "이동속도",    b.moveSpeedBonus[g],      true);
        TryAdd(parts, "공격력",      b.attackBonus[g],         true);
        TryAdd(parts, "최대 HP",     b.maxHpBonus[g],          true);
        TryAdd(parts, "HP 회복",     b.hpRegenBonus[g],        true);
        TryAdd(parts, "자석 범위",   b.magnetSizeBonus[g],     true);
        TryAdd(parts, "넉백",        b.knockBackBonus[g],      true);
 
        // 고정값 스탯
        TryAdd(parts, "방어력",      b.armorBonus[g],          false);
        TryAdd(parts, "쿨타임 감소", b.cooldownBonus[g],       false);
        TryAdd(parts, "치명타",      b.criticalChanceBonus[g], false);
 
        return string.Join(", ", parts);
    }
 
    static void TryAdd(List<string> parts, string label, float value, bool isPercent)
    {
        if (value == 0f) return;
 
        string formatted;
        if (isPercent)
        {
            float displayValue = value * 100f;
            formatted = displayValue == Mathf.Floor(displayValue)
                ? $"{label} +{displayValue:0}%"
                : $"{label} +{displayValue:0.#}%";
        }
        else
        {
            formatted = value == Mathf.Floor(value)
                ? $"{label} +{value:0}"
                : $"{label} +{value:0.#}";
        }
 
        parts.Add(formatted);
    }
}