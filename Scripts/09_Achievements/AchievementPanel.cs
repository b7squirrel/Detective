using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementPanel : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject achievementItemPrefab;
    [SerializeField] private int maxDisplayCount = 5;

    [Header("⭐ 탭 시스템")]
    [SerializeField] private Button tabPermanentButton;  // 영구 업적 탭
    [SerializeField] private Button tabDailyButton;      // 일일 퀘스트 탭
    [SerializeField] private TextMeshProUGUI titleText;  // 패널 제목

    [Header("탭 색상 설정")]
    [SerializeField] private Color activeTabColor = Color.white;
    [SerializeField] private Color inactiveTabColor = Color.gray;

    private enum TabType { Permanent, Daily }
    private TabType currentTab = TabType.Daily;  // 기본: 일일 퀘스트

    Dictionary<string, AchievementItemUI> itemDict = new();
    CardSlotManager cardSlotManager;

    private List<RuntimeAchievement> pendingRemoveList = new();

    private void OnEnable()
    {
        if (AchievementManager.Instance == null) return;

        AchievementManager.Instance.OnAnyProgressChanged += UpdateItem;
        AchievementManager.Instance.OnAnyCompleted += UpdateItem;
        AchievementManager.Instance.OnAnyRewarded += RemoveItem;
        
        LocalizationManager.OnLanguageChanged += RefreshAllText;

        if (cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager?.SettrigerAnim("Off");

        foreach (var ra in pendingRemoveList.ToList())
        {
            FinishRemove(ra);
        }

        // ⭐ 기본 탭으로 설정 (일일 퀘스트)
        SwitchTab(TabType.Daily);
    }

    private void OnDisable()
    {
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.OnAnyProgressChanged -= UpdateItem;
            AchievementManager.Instance.OnAnyCompleted -= UpdateItem;
            AchievementManager.Instance.OnAnyRewarded -= RemoveItem;
        }
        
        LocalizationManager.OnLanguageChanged -= RefreshAllText;
    }

    private void Start()
    {
        if (AchievementManager.Instance == null) return;
        
        // ⭐ 탭 버튼 이벤트 연결
        if (tabPermanentButton != null)
            tabPermanentButton.onClick.AddListener(() => SwitchTab(TabType.Permanent));
        
        if (tabDailyButton != null)
            tabDailyButton.onClick.AddListener(() => SwitchTab(TabType.Daily));
        
        // 모든 업적 생성 (표시는 RefreshUI에서 제어)
        foreach (var ra in AchievementManager.Instance.GetAll())
        {
            if (ra.isRewarded) continue;

            var go = Instantiate(achievementItemPrefab, content);
            var ui = go.GetComponent<AchievementItemUI>();

            ui.Bind(ra);
            itemDict.Add(ra.original.id, ui);
        }

        RefreshUI();
    }

    // ⭐ 탭 전환
    private void SwitchTab(TabType tabType)
    {
        currentTab = tabType;
        
        // 탭 버튼 색상 업데이트
        UpdateTabButtonColors();
        
        // 제목 업데이트
        UpdateTitle();
        
        // UI 갱신
        RefreshUI();
    }

    // ⭐ 탭 버튼 색상 업데이트
    private void UpdateTabButtonColors()
    {
        if (tabPermanentButton != null)
        {
            var img = tabPermanentButton.GetComponent<Image>();
            if (img != null)
                img.color = currentTab == TabType.Permanent ? activeTabColor : inactiveTabColor;
        }

        if (tabDailyButton != null)
        {
            var img = tabDailyButton.GetComponent<Image>();
            if (img != null)
                img.color = currentTab == TabType.Daily ? activeTabColor : inactiveTabColor;
        }
    }

    // ⭐ 패널 제목 업데이트
    private void UpdateTitle()
    {
        if (titleText == null) return;

        if (currentTab == TabType.Daily)
            titleText.text = LocalizationManager.Game.dailyQuestsTitle;
        else
            titleText.text = LocalizationManager.Game.achievementsTitle;
    }

    private void UpdateItem(RuntimeAchievement ra)
    {
        if (ra.isRewarded) return;

        if (itemDict.TryGetValue(ra.original.id, out var ui))
            ui.Refresh();

        RefreshUI();
    }

    private void RefreshAllText()
    {
        UpdateTitle();
        
        foreach (var kvp in itemDict)
        {
            AchievementItemUI ui = kvp.Value;
            if (ui != null && ui.ra != null)
            {
                ui.Refresh();
            }
        }
    }

    private void RemoveItem(RuntimeAchievement ra)
    {
        if (!pendingRemoveList.Contains(ra))
            pendingRemoveList.Add(ra);

        StartCoroutine(RemoveItemCo(ra));
    }

    IEnumerator RemoveItemCo(RuntimeAchievement ra)
    {
        if (itemDict.TryGetValue(ra.original.id, out var ui))
        {
            ui.GetComponent<Animator>().SetTrigger("Swipe");
            yield return new WaitForSeconds(0.5f);
        }

        FinishRemove(ra);
    }

    private void FinishRemove(RuntimeAchievement ra)
    {
        if (itemDict.TryGetValue(ra.original.id, out var ui))
        {
            Destroy(ui.gameObject);
            itemDict.Remove(ra.original.id);
        }

        pendingRemoveList.Remove(ra);

        RefreshUI();
    }

    /// <summary>
    /// ⭐ UI 정렬 + 탭에 따라 필터링
    /// </summary>
    public void RefreshUI()
    {
        // itemDict에서 직접 가져오기 (Destroy된 것 제외)
        var items = itemDict.Values
            .Where(ui => ui != null && ui.gameObject != null)
            .ToList();

        // ⭐ 현재 탭에 맞는 항목만 필터링
        List<AchievementItemUI> filteredItems;
        
        if (currentTab == TabType.Daily)
        {
            // 일일 퀘스트만
            filteredItems = items
                .Where(i => i.ra.original.isDailyQuest)
                .ToList();
        }
        else
        {
            // 영구 업적만
            filteredItems = items
                .Where(i => !i.ra.original.isDailyQuest)
                .ToList();
        }

        // 정렬: 완료된 것 위, 그 안에서는 SO 순서대로
        var sortedItems = filteredItems
            .OrderByDescending(i => i.ra.isCompleted)
            .ThenBy(i => AchievementManager.Instance.achievementSOList.IndexOf(i.ra.original))
            .ToList();

        // 모든 아이템 비활성화
        foreach (var item in items)
        {
            item.gameObject.SetActive(false);
        }

        // 현재 탭의 아이템만 활성화 및 정렬
        for (int i = 0; i < sortedItems.Count; i++)
        {
            sortedItems[i].transform.SetSiblingIndex(i);

            // maxDisplayCount 이내만 활성화
            bool shouldShow = (maxDisplayCount <= 0) || (i < maxDisplayCount);
            sortedItems[i].gameObject.SetActive(shouldShow);
        }
    }

    public void SetMaxDisplayCount(int count)
    {
        maxDisplayCount = count;
    }

    public void ForceCompleteAllAchievements()
    {
        foreach (var kvp in itemDict)
        {
            AchievementItemUI ui = kvp.Value;
            ui.ForceComplete();
        }

        RefreshUI();
    }
}