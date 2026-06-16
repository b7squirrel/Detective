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

    [Header("빈 탭 메시지")]
    [SerializeField] private GameObject allDoneMessageINF;
    [SerializeField] private TextMeshProUGUI allDoneTextINF;

    [Header("탭 애니메이터")]
    [SerializeField] private Animator tabDailyAnimator;
    [SerializeField] private Animator tabWeeklyAnimator;
    [SerializeField] private Animator panelOutlineAnimator;

    [Header("제목")]
    [SerializeField] private TextMeshProUGUI titleText;

    private enum TabType { Daily, Weekly }
    private TabType currentTab = TabType.Daily;
    private TabType previousTab = TabType.Daily;
    private bool isInitialized = false;   

    private Dictionary<string, AchievementItemUI> itemDict = new();
    private List<RuntimeAchievement> pendingRemoveList = new();

    private void OnEnable()
    {
        if (AchievementManager.Instance == null) return;

        AchievementManager.Instance.OnAnyProgressChanged += UpdateItem;
        AchievementManager.Instance.OnAnyCompleted += UpdateItem;
        AchievementManager.Instance.OnAnyRewarded += RemoveItem;

        // ⭐ 기존 아이템 전부 제거 후 새로 생성
        foreach (var ui in itemDict.Values)
        {
            if (ui != null) Destroy(ui.gameObject);
        }
        itemDict.Clear();

        foreach (var ra in AchievementManager.Instance.GetAll())
        {
            if (!ra.original.isInfiniteMode) continue;
            if (ra.isRewarded) continue;

            // ⭐ 탭 방식이면 content 하나만 사용
            var go = Instantiate(achievementItemPrefab, content);
            var ui = go.GetComponent<AchievementItemUI>();
            ui.Bind(ra);
            itemDict.Add(ra.original.id, ui);
        }

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
        isInitialized = false;
    }

    // Start()에서 아이템 생성 코드 제거
    private void Start()
    {
        if (AchievementManager.Instance == null) return;

        if (tabDailyButton != null)
            tabDailyButton.onClick.AddListener(() => SwitchTab(TabType.Daily));
        if (tabWeeklyButton != null)
            tabWeeklyButton.onClick.AddListener(() => SwitchTab(TabType.Weekly));
    }

    private void SwitchTab(TabType tabType)
    {
        previousTab = currentTab;
        currentTab = tabType;
        UpdateTabButtonColors();
        UpdateTitle();
        RefreshUI();
        // ⭐ 패널 아웃라인 애니메이션
        if (panelOutlineAnimator != null)
        {
            if (currentTab == TabType.Daily)
                panelOutlineAnimator.SetTrigger("Daily");
            else if (currentTab == TabType.Weekly)
                panelOutlineAnimator.SetTrigger("Weekly");
        }
    }

    private void UpdateTabButtonColors()
    {
        var tabMap = new (Animator anim, TabType type)[]
    {
        (tabDailyAnimator,  TabType.Daily),
        (tabWeeklyAnimator, TabType.Weekly),
    };

        foreach (var (anim, type) in tabMap)
        {
            if (anim == null) continue;

            if (type == currentTab)
                anim.SetTrigger("On");
            else if (!isInitialized || type == previousTab)
                anim.SetTrigger("Off");
        }

        isInitialized = true;
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

        bool isEmpty = sortedItems.Count == 0;

        if (allDoneMessageINF != null)
            allDoneMessageINF.SetActive(isEmpty);

        if (isEmpty && allDoneTextINF != null)
        {
            allDoneTextINF.text = currentTab == TabType.Daily
                ? LocalizationManager.Game.allDoneInfDailyMessage
                : LocalizationManager.Game.allDoneInfWeeklyMessage;
        }
    }
}