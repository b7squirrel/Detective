// ============================================================
//  파일 3 of 4 : EncyclopediaPopup.cs
//  역할 : 세트 상세 팝업 (등급 탭 + 아이템 목록 + 세트 효과)
// ============================================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
 
public class EncyclopediaPopup : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] TextMeshProUGUI setNameText;
 
    [Header("Grade Tabs")]
    // 인덱스 0=일반, 1=희귀, 2=고급, 3=전설, 4=신화
    [SerializeField] Button[] gradeButtons;    // 5개
    [SerializeField] Image[]  gradeButtonBgs;  // gradeButtons 각각의 Image (선택 색상용)
 
    [Header("Item List")]
    [SerializeField] Transform  itemListParent; // Viewport > Content
    [SerializeField] GameObject itemRowPrefab;  // EncyclopediaItemRow 프리팹
 
    [Header("Set Bonus")]
    [SerializeField] GameObject      bonusPanel;
    [SerializeField] TextMeshProUGUI bonusDescText;
 
    [Header("Close")]
    [SerializeField] Button closeButton;
 
    // ── 상태 ─────────────────────────────────────────────────
    EncycSetInfo       currentSet;
    SetBonusDefinition currentBonus;
    int                currentGrade;
 
    // ── 초기화 ───────────────────────────────────────────────
    void Awake()
    {
        // 등급 버튼 이벤트
        for (int i = 0; i < gradeButtons.Length; i++)
        {
            int g = i;
            gradeButtons[i].onClick.AddListener(() => SelectGrade(g));
        }
 
        closeButton.onClick.AddListener(Hide);
        gameObject.SetActive(false);
    }
 
    // ── 공개 API ─────────────────────────────────────────────
    public void Show(EncycSetInfo info, SetBonusDefinition bonus)
    {
        currentSet   = info;
        currentBonus = bonus;
        currentGrade = 0;
        gameObject.SetActive(true);
        Refresh();
    }
 
    // ── 내부 ─────────────────────────────────────────────────
    void Hide() => gameObject.SetActive(false);
 
    void SelectGrade(int grade)
    {
        currentGrade = grade;
        RefreshGradeButtons();
        RefreshItemList();
    }
 
    void Refresh()
    {
        setNameText.text = currentSet.setName;
        RefreshGradeButtons();
        RefreshItemList();
        RefreshBonus();
    }
 
    void RefreshGradeButtons()
    {
        for (int i = 0; i < gradeButtonBgs.Length; i++)
        {
            bool selected = (i == currentGrade);
            // 선택: 등급 색상 / 비선택: 어두운 회색
            gradeButtonBgs[i].color = selected
                ? MyGrade.GradeColors[i]
                : new Color(0.2f, 0.2f, 0.2f, 1f);
 
            // 버튼 안 텍스트도 선택 시 굵게 (옵션)
            var label = gradeButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text       = MyGrade.mGrades[i];
                label.fontStyle  = selected ? FontStyles.Bold : FontStyles.Normal;
            }
        }
    }
 
    void RefreshItemList()
    {
        // 기존 행 제거
        foreach (Transform child in itemListParent)
            Destroy(child.gameObject);
 
        string[] slotLabels = { "Head", "Face", "Chest", "Hand" };
 
        for (int slotIdx = 0; slotIdx < 4; slotIdx++)
        {
            var items = currentSet.slotItems[slotIdx];
 
            if (items.Count == 0)
            {
                // 빈 슬롯: 회색 빈 행 표시
                GameObject row = Instantiate(itemRowPrefab, itemListParent);
                row.GetComponent<EncyclopediaItemRow>()
                   .InitEmpty(slotLabels[slotIdx]);
                continue;
            }
 
            foreach (EncycItemInfo item in items)
            {
                string displayName = GetDisplayName(item.internalName);
 
                GameObject row = Instantiate(itemRowPrefab, itemListParent);
                row.GetComponent<EncyclopediaItemRow>()
                   .Init(slotLabels[slotIdx], displayName,
                         item.isEssential,
                         item.hp[currentGrade],
                         item.atk[currentGrade]);
            }
        }
    }
 
    void RefreshBonus()
    {
        if (currentBonus == null || string.IsNullOrEmpty(currentBonus.bonusDescription))
        {
            bonusPanel.SetActive(false);
            return;
        }
        bonusPanel.SetActive(true);
        bonusDescText.text = currentBonus.bonusDescription;
    }
 
    // ── 다국어 이름 조회 ─────────────────────────────────────
    static string GetDisplayName(string internalName)
    {
        if (LocalizationManager.IsInitialized)
            return LocalizationManager.Item.GetItemDisplayName(internalName);
        return internalName;
    }
}
