using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfiniteAchievementPanel : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject achievementItemPrefab;

    [Header("탭 시스템")]
    [SerializeField] private Button tabDailyButton;
    [SerializeField] private Button tabWeeklyButton;

    [Header("탭 색상")]
    [SerializeField] private Color activeTabColor = Color.white;
    [SerializeField] private Color inactiveTabColor = Color.gray;

    [Header("제목")]
    [SerializeField] private TextMeshProUGUI titleText;

    private enum TabType { Daily, Weekly }
    private TabType currentTab = TabType.Daily;

    private Dictionary<string, AchievementItemUI> itemDict = new();
    private List<RuntimeAchievement> pendingRemoveList = new();

    private void OnEnable()
    {
        if (AchievementManager.Instance == null) return;

        AchievementManager.Instance.OnAnyProgressChanged += UpdateItem;
        AchievementManager.Instance.OnAnyCompleted += UpdateItem;
        AchievementManager.Instance.OnAnyRewarded += RemoveItem;
        LocalizationManager.OnLanguageChanged += RefreshAllText;

        // 패널 열릴 때마다 pending 처리
        foreach (var ra in pendingRemoveList.ToList())
            FinishRemove(ra);

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

        if (tabDailyButton != null)
            tabDailyButton.onClick.AddListener(() => SwitchTab(TabType.Daily));
        if (tabWeeklyButton != null)
            tabWeeklyButton.onClick.AddListener(() => SwitchTab(TabType.Weekly));

        // 무한모드 전용 아이템만 생성
        foreach (var ra in AchievementManager.Instance.GetAll())
        {
            if (!ra.original.isInfiniteMode) continue;
            if (ra.isRewarded) continue;

            var go = Instantiate(achievementItemPrefab, content);
            var ui = go.GetComponent<AchievementItemUI>();
            ui.Bind(ra);
            itemDict.Add(ra.original.id, ui);
        }

        RefreshUI();
    }

    private void SwitchTab(TabType tabType)
    {
        currentTab = tabType;
        UpdateTabButtonColors();
        UpdateTitle();
        RefreshUI();
    }

    private void UpdateTabButtonColors()
    {
        if (tabDailyButton != null)
        {
            var img = tabDailyButton.GetComponent<Image>();
            if (img != null)
                img.color = currentTab == TabType.Daily ? activeTabColor : inactiveTabColor;
        }
        if (tabWeeklyButton != null)
        {
            var img = tabWeeklyButton.GetComponent<Image>();
            if (img != null)
                img.color = currentTab == TabType.Weekly ? activeTabColor : inactiveTabColor;
        }
    }

    private void UpdateTitle()
    {
        if (titleText == null) return;
        titleText.text = currentTab == TabType.Daily
            ? LocalizationManager.Game.dailyQuestsTitle
            : LocalizationManager.Game.weeklyQuestsTitle;
    }

    private void UpdateItem(RuntimeAchievement ra)
    {
        if (!ra.original.isInfiniteMode) return;
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
            if (kvp.Value != null && kvp.Value.ra != null)
                kvp.Value.Refresh();
        }
    }

    private void RemoveItem(RuntimeAchievement ra)
    {
        if (!ra.original.isInfiniteMode) return;

        if (!pendingRemoveList.Contains(ra))
            pendingRemoveList.Add(ra);

        StartCoroutine(RemoveItemCo(ra));
    }

    private IEnumerator RemoveItemCo(RuntimeAchievement ra)
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

    public void RefreshUI()
    {
        var items = itemDict.Values
            .Where(ui => ui != null && ui.gameObject != null)
            .ToList();

        // 현재 탭 필터 (isInfiniteMode는 이미 Start에서 걸러짐)
        List<AchievementItemUI> filteredItems;
        if (currentTab == TabType.Daily)
            filteredItems = items.Where(i => i.ra.original.isDailyQuest).ToList();
        else
            filteredItems = items.Where(i => i.ra.original.isWeeklyQuest).ToList();

        // 정렬: 완료된 것 위, SO 순서대로
        var sortedItems = filteredItems
            .OrderByDescending(i => i.ra.isCompleted)
            .ThenBy(i => AchievementManager.Instance.achievementSOList.IndexOf(i.ra.original))
            .ToList();

        // 전체 비활성화 후 현재 탭만 활성화
        foreach (var item in items)
            item.gameObject.SetActive(false);

        for (int i = 0; i < sortedItems.Count; i++)
        {
            sortedItems[i].transform.SetSiblingIndex(i);
            sortedItems[i].gameObject.SetActive(true);
        }
    }
}