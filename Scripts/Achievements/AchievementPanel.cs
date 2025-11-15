using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementPanel : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject achievementItemPrefab;

    Dictionary<string, AchievementItemUI> itemDict = new();
    CardSlotManager cardSlotManager;

    private void OnEnable()
    {
        if (AchievementManager.Instance == null) return;
        AchievementManager.Instance.OnAnyProgressChanged += UpdateItem;
        AchievementManager.Instance.OnAnyCompleted += UpdateItem;
        AchievementManager.Instance.OnAnyRewarded += RemoveItem;

        if (cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager.SettrigerAnim("Off");
    }

    private void OnDisable()
    {
        AchievementManager.Instance.OnAnyProgressChanged -= UpdateItem;
        AchievementManager.Instance.OnAnyCompleted -= UpdateItem;
        AchievementManager.Instance.OnAnyRewarded -= RemoveItem;
    }

    private void Start()
    {
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

    private void RemoveItem(RuntimeAchievement ra)
    {
        if (itemDict.TryGetValue(ra.original.id, out var ui))
        {
            Destroy(ui.gameObject);
            itemDict.Remove(ra.original.id);
        }

        RefreshUI();
    }

    /// <summary>
    /// UI 정렬: 완료된 항목 위, 완료 항목끼리는 SO 리스트 순서
    /// </summary>
    private void RefreshUI()
    {
        var items = content.GetComponentsInChildren<AchievementItemUI>();

        var sortedItems = items
            .OrderByDescending(i => i.ra.isCompleted) // 완료된 항목 위
            .ThenBy(i => AchievementManager.Instance.achievementSOList.IndexOf(i.ra.original)) // SO 리스트 순서
            .ToList();

        for (int i = 0; i < sortedItems.Count; i++)
        {
            sortedItems[i].transform.SetSiblingIndex(i);
        }
    }
}