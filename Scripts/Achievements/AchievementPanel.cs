using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementPanel : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject achievementItemPrefab;
    [SerializeField] private int maxDisplayCount = 5; // ★ 인스펙터에서 설정

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
        cardSlotManager.SettrigerAnim("Off");

        foreach (var ra in pendingRemoveList.ToList())
        {
            FinishRemove(ra);
        }

        RefreshUI();
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
        
        // ★ 모든 업적 생성 (표시는 RefreshUI에서 제어)
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

    private void UpdateItem(RuntimeAchievement ra)
    {
        if (ra.isRewarded) return;

        if (itemDict.TryGetValue(ra.original.id, out var ui))
            ui.Refresh();

        RefreshUI();
    }

    private void RefreshAllText()
    {
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

        RefreshUI(); // ★ 삭제 후 다음 업적이 자동으로 표시됨
    }

    /// <summary>
    /// ★ UI 정렬 + 최대 표시 개수 제한
    /// 완료된 항목 위, 그 안에서는 SO 리스트 순서대로
    /// maxDisplayCount만큼만 활성화, 나머지는 비활성화
    /// </summary>
    public void RefreshUI()
    {
        // ★ itemDict에서 직접 가져오기 (Destroy된 것 제외)
        var items = itemDict.Values
            .Where(ui => ui != null && ui.gameObject != null)
            .ToList();

        var sortedItems = items
            .OrderByDescending(i => i.ra.isCompleted)
            .ThenBy(i => AchievementManager.Instance.achievementSOList.IndexOf(i.ra.original))
            .ToList();

        for (int i = 0; i < sortedItems.Count; i++)
        {
            sortedItems[i].transform.SetSiblingIndex(i);

            // ★ maxDisplayCount 이내만 활성화
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