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
    [SerializeField] private Button tabWeeklyButton;  // 주간 퀘스트 탭
    [SerializeField] private TextMeshProUGUI titleText;  // 패널 제목

    [Header("탭 애니메이터")]
    [SerializeField] private Animator tabPermanentAnimator;
    [SerializeField] private Animator tabDailyAnimator;
    [SerializeField] private Animator tabWeeklyAnimator;

    private enum TabType { Permanent, Daily, Weekly }
    private TabType currentTab = TabType.Daily;  // 기본: 일일 퀘스트
    private TabType previousTab = TabType.Daily; // 활성화 탭을 기억해 두고 Off 애니메이션
    private bool isInitialized = false;

    Dictionary<string, AchievementItemUI> itemDict = new();
    CardSlotManager cardSlotManager;

    private List<RuntimeAchievement> pendingRemoveList = new();
    // ✅ 추가: 레이아웃 강제 재계산용
    public RectTransform GetContentRect() => content as RectTransform;

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

        // ✅ 수정: 튜토리얼 Step4일 때는 영구 업적 탭으로 시작
        if (TutorialManager.instance?.CurrentStep == TutorialStep.Step4_AchievementUnlocked)
            SwitchTab(TabType.Permanent);
        else
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
        isInitialized = false; // ⭐ 패널 닫힐 때 리셋
    }

    private void Start()
    {
        if (AchievementManager.Instance == null) return;

        // ⭐ 탭 버튼 이벤트 연결
        if (tabPermanentButton != null)
            tabPermanentButton.onClick.AddListener(() => SwitchTab(TabType.Permanent));

        if (tabDailyButton != null)
            tabDailyButton.onClick.AddListener(() => SwitchTab(TabType.Daily));

        if (tabWeeklyButton != null)
            tabWeeklyButton.onClick.AddListener(() => SwitchTab(TabType.Weekly));

        // 모든 업적 생성 (표시는 RefreshUI에서 제어)
        foreach (var ra in AchievementManager.Instance.GetAll())
        {
            if (ra.isRewarded) continue; // 영구 업적, 일일 퀘스트 모두 동일하게 처리

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
        previousTab = currentTab;
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
        var tabMap = new (Animator anim, TabType type)[]
        {
        (tabPermanentAnimator, TabType.Permanent),
        (tabDailyAnimator,     TabType.Daily),
        (tabWeeklyAnimator,    TabType.Weekly),
        };

        foreach (var (anim, type) in tabMap)
        {
            if (anim == null) continue;

            if (type == currentTab)
                anim.SetTrigger("On");
            else if (!isInitialized || type == previousTab) // ⭐ 초기화 전이면 모든 비활성 탭에 Off
                anim.SetTrigger("Off");
        }

        isInitialized = true; // ⭐ 첫 호출 이후 true
    }

    // ⭐ 패널 제목 업데이트
    private void UpdateTitle()
    {
        if (titleText == null) return;

        if (currentTab == TabType.Daily)
            titleText.text = LocalizationManager.Game.dailyQuestsTitle;
        else if (currentTab == TabType.Weekly)
            titleText.text = LocalizationManager.Game.weeklyQuestsTitle;
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
            filteredItems = items
                .Where(i => i.ra.original.isDailyQuest && !i.ra.original.isInfiniteMode)
                .ToList();
        }
        else if (currentTab == TabType.Weekly)
        {
            filteredItems = items
                .Where(i => i.ra.original.isWeeklyQuest && !i.ra.original.isInfiniteMode)
                .ToList();
        }
        else
        {
            filteredItems = items
                .Where(i => !i.ra.original.isDailyQuest
                        && !i.ra.original.isWeeklyQuest
                        && !i.ra.original.isInfiniteMode)
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

    public void SwitchTabPublic(TabTypePublic tab)
    {
        switch (tab)
        {
            case TabTypePublic.Permanent: SwitchTab(TabType.Permanent); break;
            case TabTypePublic.Daily: SwitchTab(TabType.Daily); break;
            case TabTypePublic.Weekly: SwitchTab(TabType.Weekly); break;
        }
    }

    // ✅ 추가: 특정 업적의 보상 버튼 RectTransform 반환
    public RectTransform GetRewardButtonRect(string achievementId)
    {
        if (itemDict.TryGetValue(achievementId, out var ui))
            return ui.GetRewardButtonRect();

        Debug.LogWarning($"[AchievementPanel] ID '{achievementId}' 업적을 찾을 수 없습니다.");
        return null;
    }

    // ✅ 디버그 리셋 후 UI 완전 재초기화
    // 보상 수령으로 파괴된 GameObject를 포함해 전체 재생성
    public void ReinitializeAll()
    {
        // ✅ null 체크
        if (AchievementManager.Instance == null)
        {
            Debug.LogWarning("[AchievementPanel] AchievementManager.Instance가 null입니다.");
            return;
        }

        foreach (var ui in itemDict.Values)
        {
            if (ui != null) Destroy(ui.gameObject);
        }
        itemDict.Clear();
        pendingRemoveList.Clear();

        // isRewarded 포함 전체 재생성 (리셋 후이므로 모두 표시)
        foreach (var ra in AchievementManager.Instance.GetAll())
        {
            var go = Instantiate(achievementItemPrefab, content);
            var ui = go.GetComponent<AchievementItemUI>();
            ui.Bind(ra);
            itemDict.Add(ra.original.id, ui);
        }

        Debug.Log($"[AchievementPanel] ReinitializeAll 완료 - {itemDict.Count}개 항목");
        RefreshUI();
    }
}