using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
 
public class EncyclopediaSetEntry : MonoBehaviour
{
    [Header("카드 배경 Image")]
    [SerializeField] Image[] slotCardImages;
 
    [Header("아이템 Image")]
    [SerializeField] Image[] slotItemImages;
 
    [Header("카드 배경 스프라이트")]
    [SerializeField] Sprite acquiredCardSprite;
    [SerializeField] Sprite unacquiredCardSprite;
    [SerializeField] Sprite emptySlotSprite;
 
    [Header("Set Entry 배경")]
    [SerializeField] Image      entryBackground;
    [SerializeField] Color      colorComplete   = new Color(1f, 0.85f, 0.2f, .3f);
    [SerializeField] Color      colorIncomplete = new Color(0.1f, 0.1f, 0.15f, .3f);
    [SerializeField] GameObject glow;
 
    [Header("기타")]
    [SerializeField] TextMeshProUGUI   setNameText;
    [SerializeField] TextMeshProUGUI[] slotLabels;
    [SerializeField] Button            tapButton;
 
    static readonly Color ITEM_ACQUIRED   = Color.white;
    static readonly Color ITEM_UNACQUIRED = new Color(1f, 1f, 1f, 0.3f);
    static readonly Color ITEM_EMPTY      = new Color(0f, 0f, 0f, 0f);
    static readonly Color LABEL_ACQUIRED  = Color.white;
    static readonly Color LABEL_DIM       = new Color(0.45f, 0.45f, 0.45f, 1f);
 
    EncycSetInfo cachedInfo;
 
    // ── 최초 초기화 ──────────────────────────────────────────
    public void Init(EncycSetInfo info,
                     HashSet<string> acquiredNames,
                     System.Action<EncycSetInfo> onTap)
    {
        cachedInfo = info;
 
        if (onTap != null)
        {
            tapButton.onClick.RemoveAllListeners();
            tapButton.onClick.AddListener(() => onTap(info));
        }
 
        ApplyVisuals(acquiredNames);
    }
 
    // ── 런타임 갱신 ──────────────────────────────────────────
    public void Refresh(HashSet<string> acquiredNames)
    {
        if (cachedInfo == null) return;
        ApplyVisuals(acquiredNames);
    }
 
    // ── 표시 처리 ────────────────────────────────────────────
    void ApplyVisuals(HashSet<string> acquiredNames)
    {
        setNameText.text = GetSetDisplayName();
 
        for (int i = 0; i < 4; i++)
        {
            var  items   = cachedInfo.slotItems[i];
            bool hasDef  = items.Count > 0;
            EncycItemInfo first = hasDef ? items[0] : null;
            bool acquired = hasDef && acquiredNames.Contains(first.internalName);
 
            // 카드 배경
            if (slotCardImages != null && i < slotCardImages.Length
                && slotCardImages[i] != null)
            {
                slotCardImages[i].sprite = (hasDef && acquired)
                    ? acquiredCardSprite
                    : unacquiredCardSprite;
            }
 
            // 아이템 이미지
            if (slotItemImages != null && i < slotItemImages.Length
                && slotItemImages[i] != null)
            {
                Sprite spr = first?.itemSO?.charImage;
                if (hasDef && spr != null)
                {
                    slotItemImages[i].sprite = spr;
                    slotItemImages[i].color  = acquired
                        ? ITEM_ACQUIRED
                        : ITEM_UNACQUIRED;
                }
                else
                {
                    slotItemImages[i].sprite = emptySlotSprite;
                    slotItemImages[i].color  = new Color(1f, 1f, 1f, 0.5f);
                }
            }
 
            // 슬롯 라벨
            if (slotLabels != null && i < slotLabels.Length
                && slotLabels[i] != null)
            {
                if (hasDef && first?.itemSO != null)
                {
                    slotLabels[i].text  = GetItemDisplayName(first.itemSO.Name);
                    slotLabels[i].color = acquired ? LABEL_ACQUIRED : LABEL_DIM;
                }
                else
                {
                    slotLabels[i].text  = "-";
                    slotLabels[i].color = LABEL_DIM;
                }
            }
        }
 
        // Set Entry 배경색
        if (entryBackground != null)
        {
            bool allAcquired = true;
            for (int i = 0; i < 4; i++)
            {
                var items = cachedInfo.slotItems[i];
                if (items.Count == 0) continue;
                if (!acquiredNames.Contains(items[0].internalName))
                {
                    allAcquired = false;
                    break;
                }
            }
            entryBackground.color = allAcquired ? colorComplete : colorIncomplete;
            if (glow != null) glow.SetActive(allAcquired);
        }
    }
 
    // ── 세트 표시명 (인스턴스 메서드 — cachedInfo 사용) ───────
    string GetSetDisplayName()
    {
        // 1차: Essential 아이템에서 탐색
        for (int i = 0; i < 4; i++)
        {
            foreach (var item in cachedInfo.slotItems[i])
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
            foreach (var item in cachedInfo.slotItems[i])
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
        return cachedInfo.setName;
    }
 
    static string GetItemDisplayName(string internalName)
    {
        if (LocalizationManager.IsInitialized)
            return LocalizationManager.Item.GetItemDisplayName(internalName);
        return internalName;
    }
}